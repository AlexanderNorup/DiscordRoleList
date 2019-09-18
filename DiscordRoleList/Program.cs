using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;

namespace DiscordRoleList
{
    class Program
    {
        public static bool debug = false;
        public static String mysqlAuth = "";
        private CommandService commands; //Unused. - For future releases.
        private DiscordSocketClient client;
        private IServiceProvider services;
        public static IDUtils idutl;
        private Timer timer = new Timer();

        static void Main(string[] args) => new Program().Start().GetAwaiter().GetResult();

        private String DB_column = "";
        private String DB_table = "";

        private List<ulong> roles = new List<ulong>();

        public async Task Start()
        {
#if (DEBUG)
            debug = true;
#endif
            //Config FILE
            _ = Logger.Log("Loading configs...");
            mysqlAuth = ConfigurationManager.AppSettings["mysqlConfig"];

            String token = ConfigurationManager.AppSettings["bottoken"];
            String statusText = ConfigurationManager.AppSettings["statusText"];

            DB_column = ConfigurationManager.AppSettings["mysqlColoum"];
            DB_table = ConfigurationManager.AppSettings["mysqlTable"];

            int timerInterval = 0;

            int.TryParse(ConfigurationManager.AppSettings["timerInterval"], out timerInterval);

            if (timerInterval <= 0)
            {
                _ = Logger.Log("TimerInterval is not set correctly. Please set timerInterval to an int that's more than 0.");
                return;
            }

            if (DB_column == "" || DB_table == "")
            {
                _ = Logger.Log("Blank 'mysqlColoum' or 'mysqlTable' setting. Please fix.");
                return;
            }

            String roles = ConfigurationManager.AppSettings["rolesToAdd"];

            foreach (String s in roles.Split(";"))
            {
                ulong roleid = 0;
                ulong.TryParse(s, out roleid);
                if (roleid > 0)
                {
                    Logger.Log("Added role: " + roleid + " to the watcher..");
                    this.roles.Add(roleid);
                }
            }

            _ = Logger.Log("Loading Discord Configs...");
            //Discord.NET Config
            DiscordSocketConfig config = new DiscordSocketConfig();
            if (debug)
                config.LogLevel = LogSeverity.Verbose;
            else
                config.LogLevel = LogSeverity.Info;
            config.MessageCacheSize = 10000;
            config.AlwaysDownloadUsers = true;
            config.DefaultRetryMode = RetryMode.AlwaysRetry;
            config.ConnectionTimeout = 5 * 60 * 1000; //5m
            client = new DiscordSocketClient(config);
            client.Log += Logger.Log;

            services = new ServiceCollection()
                .BuildServiceProvider();
            CommandServiceConfig cmdConfig = new CommandServiceConfig();
            cmdConfig.DefaultRunMode = RunMode.Async;
            commands = new CommandService(cmdConfig);
            commands.Log += Logger.Log;
            await InstallCommands();



            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
            await client.SetGameAsync(statusText);

            Program.idutl = new IDUtils(client, commands);

            timer.Elapsed += RefreshRoles;
            timer.AutoReset = true;
            timer.Interval = timerInterval;
            timer.Enabled = true;

            await Task.Delay(-1); //Don't quit
        }


        public async Task InstallCommands()
        {
            // Discover all of the commands in this assembly and load them.
            _ = await commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
                                        services: null);
            foreach (CommandInfo c in commands.Commands)
            {
                await Logger.Log("Command " + c.Name + " loaded!");
            }
        }

        public bool BotReady()
        {
            return client.ConnectionState == ConnectionState.Connected;
        }

        public async void RefreshRoles(object sender, System.Timers.ElapsedEventArgs events)
        {
            if (!BotReady())
            {
                _ = Logger.Log("Shards not connected.", LogSeverity.Warning, "RefreshRoles");
                return;
            }

            DatabaseUtil db = new DatabaseUtil(Program.mysqlAuth);
            List<ulong> roleUsers = new List<ulong>();
            try
            {
                MySqlDataReader data_response = db.Execute("SELECT `" + DB_column + "` FROM `" + DB_table + "`");
                if (data_response.HasRows)
                {
                    while (data_response.Read())
                    {
                        ulong id = 0;
                        ulong.TryParse(data_response.GetString(DB_column), out id);
                        if (id == 0)
                        {
                            _ = Logger.Log("Invalid row in database: " + DB_column + " => " + data_response.GetString(DB_column) + ". Could not be parsed to 'ulong' number. Please fix your database!", LogSeverity.Verbose, "RefreshRoles");
                            continue;
                        }
                        roleUsers.Add(id);
                    }
                }
                else
                {
                    _ = Logger.Log("No rows returned from Database.", LogSeverity.Warning, "RefreshRoles");
                }
            }
            catch (MySqlException exception)
            {
                _ = Logger.Log(new LogMessage(LogSeverity.Error, "RefreshRoles", "MySQL Error Happened", exception));
            }



            foreach (SocketGuild guild in client.Guilds)
            {
                //Get all roles in that server
                List<SocketRole> guildRoles = new List<SocketRole>();
                foreach (ulong roleid in roles)
                {
                    SocketRole role = guild.GetRole(roleid);
                    if (role != null)
                    {
                        guildRoles.Add(role);
                        //Now we check if there's anyone with the role that shouldn't have it
                        IEnumerable<SocketGuildUser> users = role.Members.Where(gUser => !roleUsers.Any(id => gUser.Id == id));
                        foreach (SocketGuildUser u in users)
                        {
                            _ = Logger.Log("I will remove " + u.Username + " because he has the role " + role.Name + ", but shouldn't have. ");
                            try
                            {
                                await u.RemoveRoleAsync(role);
                            }
                            catch (HttpException e)
                            {
                                if (e.HttpCode != System.Net.HttpStatusCode.NotFound && e.HttpCode != System.Net.HttpStatusCode.Forbidden)
                                {
                                    //Unknown Error
                                    _ = Logger.Log(new LogMessage(LogSeverity.Warning, "RefreshRoles()", "An HTTPException occoured: ", e));
                                }
                                else
                                {
                                    //No Permission or not Found
                                    _ = Logger.Log(new LogMessage(LogSeverity.Warning, "RefreshRoles()", "Could not remove role from user.. Probably missing permission..", e));

                                }

                            }
                            continue;
                        }
                    }
                }


                foreach (ulong id in roleUsers)
                {
                    SocketGuildUser user = guild.GetUser(id);
                    if (user == null)
                    {
                        //User not in server
                        continue;
                    }
                    //Get roles

                    foreach (SocketRole role in guildRoles)
                    {
                        if (user.Roles.Any(r => r.Id == role.Id))
                        {
                            //User has the role already, so we should not try to add it again.
                            continue;
                        }
                        _ = Logger.Log("Giving " + role.Name + " to " + user.Username + "#" + user.Discriminator + " in " + guild.Name, LogSeverity.Info, "RefreshRoles");
                        try
                        {
                            await user.AddRoleAsync(role);
                        }
                        catch (HttpException e)
                        {
                            if (e.HttpCode != System.Net.HttpStatusCode.NotFound && e.HttpCode != System.Net.HttpStatusCode.Forbidden)
                            {
                                //Unknown Error
                                _ = Logger.Log(new LogMessage(LogSeverity.Warning, "RefreshRoles()", "An HTTPException occoured: ", e));
                            }
                            else
                            {
                                //No Permission or not Found
                                _ = Logger.Log(new LogMessage(LogSeverity.Warning, "RefreshRoles()", "Could not add role to user.. Probably missing permission..", e));

                            }
                            continue;
                        }
                    }

                }



            }

            db.Disconnect();
        }

    }
}

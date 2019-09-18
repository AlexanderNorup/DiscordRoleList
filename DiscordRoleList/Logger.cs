using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;

namespace DiscordRoleList
{
    class Logger
    {
        public static Task Log(LogMessage message)
        {
            if (Program.debug)
            {
                Console.WriteLine(message);
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                StringWriter sw = new StringWriter(sb);
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    writer.WriteStartObject();

                    writer.WritePropertyName("status");
                    writer.WriteValue(message.Severity.ToString());

                    if (message.Exception != null)
                    {
                        writer.WritePropertyName("message");
                        writer.WriteValue(message.Exception.Message);

                        writer.WritePropertyName("error");
                        writer.WriteStartObject();

                        writer.WritePropertyName("message");
                        writer.WriteValue(message.Exception.InnerException.Message);

                        writer.WritePropertyName("stack");
                        writer.WriteValue(message.Exception.InnerException.StackTrace);

                        writer.WritePropertyName("kind");
                        writer.WriteValue(message.Exception.InnerException.GetType().ToString());

                        writer.WriteEndObject();

                        if (message.Exception is CommandException ce)
                        {

                            writer.WritePropertyName("usermessage");
                            writer.WriteValue(ce.Context.Message.Content);

                            writer.WritePropertyName("user");
                            writer.WriteStartObject();

                            writer.WritePropertyName("username");
                            writer.WriteValue(ce.Context.User.Username + "#" + ce.Context.User.Discriminator);

                            writer.WritePropertyName("userid");
                            writer.WriteValue(ce.Context.User.Id.ToString());

                            if (ce.Context.Channel is ITextChannel it)
                            {

                                writer.WritePropertyName("guild");
                                writer.WriteValue(it.Guild.Name);

                                writer.WritePropertyName("guildid");
                                writer.WriteValue(it.GuildId.ToString());

                                writer.WritePropertyName("channel");
                                writer.WriteValue(it.Name);

                                writer.WritePropertyName("guildid");
                                writer.WriteValue(it.Id.ToString());

                                writer.WritePropertyName("channeltype");
                                writer.WriteValue("TextChannel");

                            }
                            else if (ce.Context.Channel is IDMChannel idm)
                            {
                                writer.WritePropertyName("channeltype");
                                writer.WriteValue("DMChannel");
                            }
                            writer.WriteEndObject();
                        }
                    }
                    else
                    {
                        writer.WritePropertyName("message");
                        writer.WriteValue(message.Message);
                    }

                    writer.WritePropertyName("ddsource");
                    writer.WriteValue(message.Source);

                    writer.WritePropertyName("timestamp");
                    writer.WriteValue(DateTimeOffset.Now.ToUnixTimeMilliseconds());

                    writer.WriteEndObject();
                }
                Console.WriteLine(sb.ToString());
            }
            return Task.CompletedTask;
        }

        public static Task Log(String message, LogSeverity sev = LogSeverity.Info, String what = "Console")
        {
            return Log(new LogMessage(sev, what, message));
        }
    }
}

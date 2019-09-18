using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;

namespace DiscordRoleList
{
    class IDUtils
    {
        private readonly DiscordSocketClient client;
        private readonly CommandService commands;

        public IDUtils(DiscordSocketClient client, CommandService commands)
        {
            this.client = client;
            this.commands = commands;
        }

  

        public int getGuildCount()
        {
            return client.Guilds.Count;
        }

        public IEnumerable<CommandInfo> getCommands()
        {
            return commands.Commands;
        }

        public SocketUser getUser(ulong id)
        {
            return client.GetUser(id);
        }

        public IDMChannel getDMChannel(ulong id)
        {
            SocketUser user = getUser(id);
            if (user == null)
            {
                return null;
            }
            return user.GetOrCreateDMChannelAsync().GetAwaiter().GetResult();
        }

        public SocketRole getRole(ulong guildid, ulong roleid)
        {
            return client.GetGuild(guildid).GetRole(roleid);
        }

        public SocketGuild getGuild(ulong guildid)
        {
            return client.GetGuild(guildid);
        }

        public SocketChannel getChannel(ulong id)
        {
            return client.GetChannel(id);
        }

        public IMessage getMessage(ulong serverid, ulong channel, ulong message)
        {
            return client.GetGuild(serverid).GetTextChannel(channel).GetMessageAsync(message).GetAwaiter().GetResult();
        }
    }
}

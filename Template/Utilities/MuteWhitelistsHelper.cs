using Database;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rendy.Utilities
{
    public class MuteWhitelistsHelper
    {
        private readonly MuteWhitelists _muteWhitelists;

        public MuteWhitelistsHelper(MuteWhitelists muteWhitelists)
        {
            _muteWhitelists = muteWhitelists;
        }

        public async Task<List<IChannel>> GetMuteWhitelistAsync(IGuild guild)
        {
            var channels = new List<IChannel>();
            var invalidMuteWhitelistChannels = new List<MuteWhitelist>();

            var muteWhitelistChannels = await _muteWhitelists.GetMuteWhitelistAsync(guild.Id);

            foreach (var muteWhitelist in muteWhitelistChannels)
            {
                var channel = (guild as SocketGuild).TextChannels.FirstOrDefault(x => x.Id == muteWhitelist.ChannelId);
                if (channel == null)
                    invalidMuteWhitelistChannels.Add(muteWhitelist);
                else
                {
                    channels.Add(channel);
                }
            }

            if (invalidMuteWhitelistChannels.Count > 0)
                await _muteWhitelists.ClearMuteWhitelistAsync(invalidMuteWhitelistChannels);

            return channels;
        }
    }
}

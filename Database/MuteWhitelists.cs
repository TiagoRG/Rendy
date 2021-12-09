using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public class MuteWhitelists
    {
        private readonly RendyContext _context;

        public MuteWhitelists(RendyContext context)
        {
            _context = context;
        }

        public async Task<List<MuteWhitelist>> GetMuteWhitelistAsync(ulong id)
        {
            var muteWhitelist = await _context.MuteWhilelists
                .Where(x => x.ServerId == id)
                .ToListAsync();

            return await Task.FromResult(muteWhitelist);
        }

        public async Task AddMuteWhitelistAsync(ulong id, ulong channelId)
        {
            var server = await _context.Servers
                .FindAsync(id);

            if (server == null)
                _context.Add(new Server { Id = id });

            _context.Add(new MuteWhitelist { ChannelId = channelId, ServerId = id });
            await _context.SaveChangesAsync();
        }

        public async Task RemoveMuteWhitelistAsync(ulong id, ulong channelId)
        {
            var whitelist = await _context.MuteWhilelists
                .Where(x => x.ChannelId == channelId)
                .FirstOrDefaultAsync();

            _context.Remove(whitelist);
            await _context.SaveChangesAsync();
        }

        public async Task ClearMuteWhitelistAsync(List<MuteWhitelist> muteWhitelist)
        {
            _context.RemoveRange(muteWhitelist);
            await _context.SaveChangesAsync();
        }
    }
}
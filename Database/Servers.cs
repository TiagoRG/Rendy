using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public class Servers
    {
        private readonly RendyContext _context;

        public Servers(RendyContext context)
        {
            _context = context;
        }

        // Mute Counter

        public async Task<int> GetMuteCounter(ulong id)
        {
            var counter = _context.Servers.Where(x => x.Id == id).Select(x => x.MuteId).FirstOrDefault();
            return await Task.FromResult(counter);
        }
        public async Task AddMuteCounter(ulong id)
        {
            var server = await _context.Servers.FindAsync(id);

            if (server == null)
                _context.Add(new Server { Id = id, MuteId = 1 });
            else
                server.MuteId = await GetMuteCounter(id) + 1;

            await _context.SaveChangesAsync();
        }

        // Prefix modifier

        public async Task ModifyGuildPrefix(ulong id, string prefix)
        {
            var server = await _context.Servers
                .FindAsync(id);

            if (server == null)
                _context.Add(new Server { Id = id, Prefix = prefix });
            else
                server.Prefix = prefix;

            await _context.SaveChangesAsync();
        }

        public async Task<string> GetGuildPrefix(ulong id)
        {
            var prefix = await _context.Servers
                .Where(x => x.Id == id)
                .Select(x => x.Prefix)
                .FirstOrDefaultAsync();

            return await Task.FromResult(prefix);
        }

        // Welcome message modifier

        public async Task ModifyWelcomeAsync(ulong id, ulong channelId)
        {
            var server = await _context.Servers
                .FindAsync(id);

            if (server == null)
                _context.Add(new Server
                {
                    Id = id,
                    Welcome = channelId
                });
            else
                server.Welcome = channelId;

            await _context.SaveChangesAsync();
        }

        public async Task ClearWelcomeAsync(ulong id)
        {
            var server = await _context.Servers
                .FindAsync(id);

            server.Welcome = 0;

            await _context.SaveChangesAsync();
        }

        public async Task<ulong> GetWelcomeAsync(ulong id)
        {
            var server = await _context.Servers
                .FindAsync(id);

            return await Task.FromResult(server.Welcome);
        }

        public async Task ModifyBackgroundAsync(ulong id, string url)
        {
            var server = await _context.Servers
                .FindAsync(id);

            if (server == null)
                _context.Add(new Server
                {
                    Id = id,
                    Background = url
                });
            else
                server.Background = url;

            await _context.SaveChangesAsync();
        }

        public async Task ClearBackgroundAsync(ulong id)
        {
            var server = await _context.Servers
                .FindAsync(id);

            server.Background = null;

            await _context.SaveChangesAsync();
        }

        public async Task<string> GetBackgroundAsync(ulong id)
        {
            var server = await _context.Servers
                .FindAsync(id);

            return await Task.FromResult(server.Background);
        }

        public async Task ModifyModLogsAsync(ulong id, ulong channelId)
        {
            var server = await _context.Servers
                .FindAsync(id);

            if (server == null)
                _context.Add(new Server
                {
                    Id = id,
                    ModLogs = channelId
                });
            else
                server.ModLogs = channelId;

            await _context.SaveChangesAsync();
        }

        public async Task ClearModLogsAsync(ulong id)
        {
            var server = await _context.Servers
                .FindAsync(id);

            server.ModLogs = 0;

            await _context.SaveChangesAsync();
        }

        public async Task<ulong> GetModLogsAsync(ulong id)
        {
            var server = await _context.Servers
                .FindAsync(id);

            return await Task.FromResult(server.ModLogs);
        }
    }
}

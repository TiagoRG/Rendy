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

        public async Task<List<Server>> GetServersAsync()
        {
            List<Server> serverData = new List<Server>();
            foreach (Server server in _context.Servers)
            {
                serverData.Add(server);
            }
            return await Task.FromResult(serverData);
        }

        public async Task ClearServersAsync()
        {
            var servers = _context.Servers.ToList();
            foreach (Server server in servers)
                _context.Servers.Remove(server);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateServersAsync(List<Server> serversList)
        {
            foreach (Server server in serversList)
            {
                _context.Servers.Add(server);
            }
            await _context.SaveChangesAsync();
        }
    }
}

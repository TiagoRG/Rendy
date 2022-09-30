using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public class Mutes
    {
        private readonly RendyContext _context;
        private readonly Servers _servers;

        public Mutes(RendyContext context, Servers servers)
        {
            _context = context;
            _servers = servers;
        }

        public async Task<List<Mute>> GetMutesAsync()
        {
            List<Mute> muteData = new List<Mute>();
            foreach (Mute mute in _context.Mutes)
            {
                muteData.Add(mute);
            }
            return await Task.FromResult(muteData);
        }

        public async Task ClearMutesAsync()
        {
            var mutes = _context.Mutes.ToList();
            foreach (Mute mute in mutes)
                _context.Mutes.Remove(mute);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateMutesAsync(List<Mute> mutesList)
        {
            foreach (Mute mute in mutesList)
            {
                _context.Mutes.Add(mute);
            }
            await _context.SaveChangesAsync();
        }
    }
}

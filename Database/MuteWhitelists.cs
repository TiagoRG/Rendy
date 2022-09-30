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

        public async Task<List<MuteWhitelist>> GetMuteWhitelistsAsync()
        {
            List<MuteWhitelist> muteWhitelistData = new List<MuteWhitelist>();
            foreach (MuteWhitelist muteWhitelist in _context.MuteWhilelists)
            {
                muteWhitelistData.Add(muteWhitelist);
            }
            return await Task.FromResult(muteWhitelistData);
        }

        public async Task ClearMuteWhitelistsAsync()
        {
            var muteWhitelists = _context.MuteWhilelists.ToList();
            foreach (MuteWhitelist muteWhitelist in muteWhitelists)
                _context.MuteWhilelists.Remove(muteWhitelist);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateMuteWhitelistsAsync(List<MuteWhitelist> muteWhitelists)
        {
            foreach (MuteWhitelist muteWhitelist in muteWhitelists)
            {
                _context.MuteWhilelists.Add(muteWhitelist);
            }
            await _context.SaveChangesAsync();
        }
    }
}
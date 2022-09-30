using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public class Bans
    {
        private readonly RendyContext _context;

        public Bans(RendyContext context)
        {
            _context = context;
        }

        public async Task<List<Ban>> GetBansAsync()
        {
            List<Ban> banData = new List<Ban>();
            foreach (Ban ban in _context.Bans)
            {
                banData.Add(ban);
            }
            return await Task.FromResult(banData);
        }

        public async Task ClearBansAsync()
        {
            var bans = _context.Bans.ToList();
            foreach (Ban ban in bans)
                _context.Bans.Remove(ban);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateBansAsync(List<Ban> bansList)
        {
            foreach (Ban ban in bansList)
            {
                _context.Bans.Add(ban);
            }
            await _context.SaveChangesAsync();
        }
    }
}

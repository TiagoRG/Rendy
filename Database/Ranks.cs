using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public class Ranks
    {
        private readonly RendyContext _context;

        public Ranks(RendyContext context)
        {
            _context = context;
        }

        public async Task<List<Rank>> GetRanksAsync()
        {
            List<Rank> rankData = new List<Rank>();
            foreach (Rank rank in _context.Ranks)
            {
                rankData.Add(rank);
            }
            return await Task.FromResult(rankData);
        }

        public async Task ClearRanksAsync()
        {
            var ranks = _context.Ranks.ToList();
            foreach (Rank rank in ranks)
                _context.Ranks.Remove(rank);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRanksAsync(List<Rank> ranksList)
        {
            foreach (Rank rank in ranksList)
            {
                _context.Ranks.Add(rank);
            }
            await _context.SaveChangesAsync();
        }
    }
}

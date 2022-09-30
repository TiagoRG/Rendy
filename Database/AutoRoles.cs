using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public class AutoRoles
    {
        private readonly RendyContext _context;

        public AutoRoles(RendyContext context)
        {
            _context = context;
        }

        public async Task<List<AutoRole>> GetAutoRolesAsync()
        {
            List<AutoRole> autoRoleData = new List<AutoRole>();
            foreach (AutoRole autoRole in _context.AutoRoles)
            {
                autoRoleData.Add(autoRole);
            }
            return await Task.FromResult(autoRoleData);
        }

        public async Task ClearAutoRolesAsync()
        {
            var autoRoles = _context.AutoRoles.ToList();
            foreach (AutoRole autoRole in autoRoles)
                _context.AutoRoles.Remove(autoRole);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAutoRolesAsync(List<AutoRole> autoRolesList)
        {
            foreach (AutoRole autoRole in autoRolesList)
            {
                _context.AutoRoles.Add(autoRole);
            }
            await _context.SaveChangesAsync();
        }
    }
}

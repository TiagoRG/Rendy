using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public class RestoreRoles
    {
        private readonly RendyContext _context;

        public RestoreRoles(RendyContext context)
        {
            _context = context;
        }

        public async Task<List<RestoreRole>> GetRestoreRolesAsync()
        {
            List<RestoreRole> restoreRoleData = new List<RestoreRole>();
            foreach (RestoreRole roles in _context.RestoreRoles)
            {
                restoreRoleData.Add(roles);
            }
            return await Task.FromResult(restoreRoleData);
        }

        public async Task ClearRestoreRolesAsync()
        {
            var restoreRoles = _context.RestoreRoles.ToList();
            foreach (RestoreRole restoreRole in restoreRoles)
                _context.RestoreRoles.Remove(restoreRole);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRestoreRolesAsync(List<RestoreRole> restoreRolesList)
        {
            foreach (RestoreRole restoreRole in restoreRolesList)
            {
                _context.RestoreRoles.Add(restoreRole);
            }
            await _context.SaveChangesAsync();
        }
    }
}

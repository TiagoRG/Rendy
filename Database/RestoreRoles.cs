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

        public async Task<List<ulong>> GetRestoreRolesIdAsync(int muteId)
        {
            var restoreRoles = _context.RestoreRoles
                .Where(x => x.MuteId == muteId);

            List<ulong> list = new List<ulong>();

            foreach (RestoreRole roleId in restoreRoles)
            {
                list.Add(roleId.RoleId);
            }

            return await Task.FromResult(list);
        }

        public async Task AddRestoreRolesAsync(int muteId, List<ulong> restoreRolesId)
        {
            foreach (ulong restoreRoleId in restoreRolesId)
            {
                _context.Add(new RestoreRole { MuteId = muteId, RoleId = restoreRoleId });
            }
            await _context.SaveChangesAsync();
        }

        public async Task RemoveRestoreRolesAsync(int muteId)
        {
            var toRemove = _context.RestoreRoles
                .Where(x => x.MuteId == muteId);

            foreach (RestoreRole remove in toRemove)
            {
                _context.Remove(remove);
            }
            await _context.SaveChangesAsync();
        }
    }
}

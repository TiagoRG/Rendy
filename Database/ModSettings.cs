using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public class ModSettings
    {
        private readonly RendyContext _context;

        public ModSettings(RendyContext context)
        {
            _context = context;
        }

        public async Task<bool> GetInviteBlockerToggleAsync(ulong id)
        {
            var inviteBlocker = await _context.ModSettings
                .Where(x => x.ServerId == id)
                .Select(x => x.InviteBlocker)
                .FirstOrDefaultAsync();

            return await Task.FromResult(inviteBlocker);
        }

        public async Task ModifyInviteBlockerToggleAsync(ulong id, bool option)
        {
            var server = await _context.Servers
                .FindAsync(id);

            if (server == null)
            {
                _context.Add(new Server { Id = id });
                _context.Add(new ModSetting { ServerId = id, InviteBlocker = option, Punishment = 1 });
            }
            else
            {
                var modSetting = await _context.ModSettings
                    .FirstOrDefaultAsync(x => x.ServerId == id);

                if (modSetting == null)
                {
                    _context.Add(new ModSetting { ServerId = id, InviteBlocker = option, Punishment = 1 });
                }
                else
                {
                    var inviteBlocker = await _context.ModSettings
                    .Where(x => x.ServerId == id)
                    .Select(x => x.InviteBlocker)
                    .FirstOrDefaultAsync();

                    inviteBlocker = option;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetInviteBlockerSettingAsync(ulong id)
        {
            var punishment = await _context.ModSettings
                .Where(x => x.ServerId == id)
                .Select(x => x.Punishment)
                .FirstOrDefaultAsync();

            return await Task.FromResult(punishment);
        }

        public async Task ModifyInviteBlockerSettingAsync(ulong id, int option)
        {
            var punishment = await _context.ModSettings
                .Where(x => x.ServerId == id)
                .Select(x => x.Punishment)
                .FirstOrDefaultAsync();
            punishment = option;

            await _context.SaveChangesAsync();
        }
    }
}

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

        public async Task<List<ModSetting>> GetModSettingsAsync()
        {
            List<ModSetting> modSettingsData = new List<ModSetting>();
            foreach (ModSetting modSetting in _context.ModSettings)
            {
                modSettingsData.Add(modSetting);
            }
            return await Task.FromResult(modSettingsData);
        }

        public async Task ClearModSettingsAsync()
        {
            var modSettings = _context.ModSettings.ToList();
            foreach (ModSetting modSetting in modSettings)
                _context.ModSettings.Remove(modSetting);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateModSettingsAsync(List<ModSetting> modSettingsList)
        {
            foreach (ModSetting modSetting in modSettingsList)
            {
                _context.ModSettings.Add(modSetting);
            }
            await _context.SaveChangesAsync();
        }
    }
}

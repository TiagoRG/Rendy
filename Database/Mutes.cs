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

        public class MuteData
        {
            public int Id;
            public int MuteId;
            public ulong ServerId;
            public ulong UserId;
            public ulong ModId;
            public ulong RoleId;
            public DateTime Begin;
            public DateTime End;
            public string Reason; 
        }

        public async Task<int> GetMuteIdAsync(ulong serverId, ulong userId)
        {
            int muteId = _context.Mutes
                .Where(x => x.ServerId == serverId)
                .Where(x => x.UserId == userId)
                .Select(x => x.MuteId)
                .FirstOrDefault();

            return await Task.FromResult(muteId);
        }

        public async Task AddMuteAsync(ulong serverId, int muteId, ulong userId, ulong modId, ulong roleId, DateTime begin, DateTime end, string reason = null)
        {
            _context.Add(new Mute { ServerId = serverId, MuteId = muteId, UserId = userId, ModId = modId, RoleId = roleId, Begin = begin, End = end, Reason = reason});

            await _context.SaveChangesAsync();
        }

        public async Task RemoveMuteAsync(ulong serverId, ulong userId)
        {
            var mute = _context.Mutes
                .Where(x => x.ServerId == serverId)
                .Where(x => x.UserId == userId)
                .FirstOrDefault();

            _context.Mutes.Remove(mute);

            await _context.SaveChangesAsync();
        }

        public async Task<MuteData> GetMuteAsync(ulong serverId, ulong userId)
        {
            var mute = _context.Mutes
                .Where(x => x.ServerId == serverId)
                .Where(x => x.UserId == userId)
                .FirstOrDefault();

            if (mute == null)
            {
                MuteData muteData1 = new MuteData { Id = -1 };
                return muteData1;
            }

            MuteData muteData2 = new MuteData { Id = mute.Id, MuteId = mute.MuteId, ServerId = mute.ServerId, UserId = mute.UserId, ModId = mute.ModId, RoleId = mute.RoleId, Begin = mute.Begin, End = mute.End, Reason = mute.Reason};

            return await Task.FromResult(muteData2);
        }

        public async Task<List<MuteData>> GetMutesAsync()
        {
            var mutes = _context.Mutes;
            List<MuteData> Data = new List<MuteData>();

            foreach (Mute mute in mutes)
            {
                Data.Add(new MuteData { Id = mute.Id, MuteId = mute.MuteId, ServerId = mute.ServerId, UserId = mute.UserId, ModId = mute.ModId, RoleId = mute.RoleId, Begin = mute.Begin, End = mute.End, Reason = mute.Reason});
            }

            return await Task.FromResult(Data);
        }
    }
}

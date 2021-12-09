using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database
{
    public class RendyContext : DbContext
    {
        public DbSet<Server> Servers { get; set; }
        public DbSet<Rank> Ranks { get; set; }
        public DbSet<AutoRole> AutoRoles { get; set; }
        public DbSet<MuteWhitelist> MuteWhilelists { get; set; }
        public DbSet<Mute> Mutes { get; set; }
        public DbSet<RestoreRole> RestoreRoles { get; set; }
        public DbSet<Ban> Bans { get; set; }
        public DbSet<ModSetting> ModSettings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseMySql("server=localhost;user=root;database=Rendy;port=3306;Connect Timeout=5");
    }

    public class Server
    {
        public ulong Id { get; set; }
        public string Prefix { get; set; }
        public ulong Welcome { get; set; }
        public string Background { get; set; }
        public ulong ModLogs { get; set; }
        public ulong AuditLogs { get; set; }
        public int MuteId { get; set; }
    }

    public class Rank
    {
        public int Id { get; set; }
        public ulong RoleId { get; set; }
        public ulong ServerId { get; set; }

    }

    public class AutoRole
    {
        public int Id { get; set; }
        public ulong RoleId { get; set; }
        public ulong ServerId { get; set; }

    }

    public class MuteWhitelist
    {
        public int Id { get; set; }
        public ulong ChannelId { get; set; }
        public ulong ServerId { get; set; }
    }

    public class Mute
    {
        public int Id { get; set; }
        public int MuteId { get; set; }
        public ulong ServerId { get; set; }
        public ulong UserId { get; set; }
        public ulong ModId { get; set; }
        public ulong RoleId { get; set; }
        public DateTime Begin { get; set; }
        public DateTime End { get; set; }
        public string Reason { get; set; }
    }

    public class RestoreRole
    {
        public int Id { get; set; }
        public int MuteId { get; set; }
        public ulong RoleId { get; set; }
    }

    public class Ban
    {
        public int Id { get; set; }
        public ulong ServerId { get; set; }
        public ulong UserId { get; set; }
        public ulong ModId { get; set; }
        public DateTime Begin { get; set; }
        public DateTime End { get; set; }
        public string Reason { get; set; }
    }

    public class ModSetting
    {
        public int Id { get; set; }
        public ulong ServerId { get; set; }
        public bool InviteBlocker { get; set; }
        public int Punishment { get; set; }
    }
}

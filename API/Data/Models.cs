﻿using System.Data.Entity;
using OTA.Data.Entity.Models;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.ComponentModel.DataAnnotations.Schema;
using OTA.Data.Entity;
using System.Data.Entity.Migrations.History;

namespace OTA.Data
{
    /// <summary>
    /// The connection context for talking to an OTA database
    /// </summary>
    //    [DbConfigurationType(typeof(EFConfiguration))] 
    public class OTAContext : DbContext // IdentityDbContext<IdentityUser>
    {
        public static DbConfiguration Config;

        public OTAContext() : this("terraria_ota") //ConnectionManager.ConnectionString)
        {
//            if (null != Config)
//            {
//                System.Console.Write("Setting DB Config...");
//                DbConfiguration.SetConfiguration(Config);
//                System.Console.WriteLine("Done");
//            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OTA.Data.Models.OTAContext"/> class.
        /// </summary>
        /// <param name="nameOrConnectionString">Name or connection string. Default is terraria_ota</param>
        public OTAContext(string nameOrConnectionString = "terraria_ota") : base(nameOrConnectionString)
        {
            Configuration.ProxyCreationEnabled = false;
            Configuration.LazyLoadingEnabled = true;
        }

        public DbSet<PlayerGroup> PlayerGroups { get; set; }

        public DbSet<Group> Groups { get; set; }

        public DbSet<DbPlayer> Players { get; set; }

        public DbSet<PermissionNode> Nodes { get; set; }

        public DbSet<PlayerNode> PlayerNodes { get; set; }

        public DbSet<GroupNode> GroupNodes { get; set; }

        public DbSet<APIAccount> APIAccounts { get; set; }

        public DbSet<APIAccountRole> APIAccountsRoles { get; set; }

        protected override void OnModelCreating(DbModelBuilder builder)
        {
            builder.Conventions.Remove<PluralizingTableNameConvention>();
//            builder.Entity<HistoryRow>()
//                .Property(h => h.MigrationId)
//                .HasMaxLength(100)
//                .IsRequired();
//            builder.Entity<HistoryRow>()
//                .Property(h => h.ContextKey)
//                .HasMaxLength(200)
//                .IsRequired();

            if (this.Database.Connection.GetType().Name == "SQLiteConnection") //Since we support SQLite as default, let's use this hack...
            {
                Database.SetInitializer(new SqliteContextInitializer<OTAContext>(builder));
            }

            builder.Entity<Group>()
                .HasKey(x => x.Id)
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            builder.Entity<DbPlayer>()
                .ToTable("Player")
                .HasKey(x => x.Id)
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            builder.Entity<PlayerGroup>()
                .HasKey(x => new { x.Id })
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            
            builder.Entity<PermissionNode>()
                .HasKey(x => new { x.Id })
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            builder.Entity<PlayerNode>()
                .HasKey(x => new { x.Id })
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            builder.Entity<GroupNode>()
                .HasKey(x => new { x.Id })
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            builder.Entity<APIAccount>()
                .HasKey(x => new { x.Id })
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            builder.Entity<APIAccountRole>()
                .HasKey(x => new { x.Id })
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}

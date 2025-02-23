using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AmpPromatic.CNCRefactor.Desktop.Data
{
    internal class DatabaseContext : DbContext
    {
        public DbSet<Machine> Machines { get; set; }
        public DbSet<Replacement> Replacements { get; set; }
        public DbSet<Insertion> Insertions { get; set; }
        public DbSet<Removal> Removals { get; set; }
        public DbSet<Transition> Transitions { get; set; }

        public string DbPath { get; }

        public DatabaseContext()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = System.IO.Path.Join(path, "cncrefactor-v2.db");
        }

        // The following configures EF to create a Sqlite database file in the
        // special "local" folder for your platform.
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public void Initialize()
        {
            Database.Migrate();
            Seed.InitialiizeData(this);
        }

        public void RestoreDefaults()
        {
            Removals.ExecuteDelete();
            Insertions.ExecuteDelete();
            Replacements.ExecuteDelete();
            Machines.ExecuteDelete();
            Transitions.ExecuteDelete();
            Seed.InitialiizeData(this);
        }
    }

    public class Machine
    {
        public int MachineId { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }

        public List<Replacement> Replacements { get; } = new();

        public override string ToString()
        {
            return Name;
        }
    }

    public class Replacement
    {
        public int ReplacementId { get; set; }

        public int MachineId { get; set; }

        public string Text { get; set; }
        public string TextToReplace { get; set; }

        public Machine Machine { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }

    public class Insertion
    {
        public int InsertionId { get; set; }

        public int MachineId { get; set; }

        public string Text { get; set; }

        public string Qualifier { get; set; }

        public InsertionType InsertionType { get; set; }

        public Machine Machine { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }

    public enum InsertionType
    {
        NextLine,
        PreviousLine,
        StartOfLine,
        EndOfLine,
        SameLineAfterText,
        SameLineBeforeText,
    }
    public class Removal
    {
        public int RemovalId { get; set; }

        public int MachineId { get; set; }

        public string Text { get; set; }

        public Machine Machine { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }

    public class Transition
    {
        public int TransitionId { get; set; }
        public int MachineId { get; set; }

        public string OldText { get; set; }

        public string NewText { get; set; }

        public Machine Machine { get; set; }
    }
}

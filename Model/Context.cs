using Microsoft.EntityFrameworkCore;

namespace InternetSecurityProject.Model
{
    public class Context : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=database.sqlite");
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
        
        public DbSet<Certificate> Certificates { get; set; }
        
        public DbSet<Tfa> Tfas { get; set; }
        
        public DbSet<Attack> Attacks { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>();
            modelBuilder.Entity<Certificate>();
            modelBuilder.Entity<Message>().HasOne(x => x.Sender).WithMany();
            modelBuilder.Entity<Message>().HasOne(x => x.Receiver).WithMany();
            modelBuilder.Entity<Certificate>().HasOne(x => x.User).WithMany();
            modelBuilder.Entity<Tfa>().HasOne(x => x.User).WithMany();
            modelBuilder.Entity<Attack>().HasOne(x => x.Attacker).WithMany();
            modelBuilder.Entity<Attack>().HasOne(x => x.Attacked).WithMany();
        }
    }
    
    
}
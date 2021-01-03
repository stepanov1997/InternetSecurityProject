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
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>();
            modelBuilder.Entity<Message>().HasOne<User>(x => x.Sender).WithMany();
            modelBuilder.Entity<Message>().HasOne<User>(x => x.Receiver).WithMany();
        }
    }
    
    
}
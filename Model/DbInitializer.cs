using System.Linq;

namespace InternetSecurityProject.Model
{
    public static class DbInitializer
    {
        public static void Initialize(Context context)
        {
            context.Database.EnsureCreated();

            if (context.Users.Any())
            {
                return; 
            }

            var users = new[]
            {
                new User() {Id = 1, Username = "Sony Xperia S", Password = "12345", Token = null},
                new User() {Id = 2, Username = "cobaman", Password = "12345", Token = null},
                new User() {Id = 3, Username = "Pjesadinac", Password = "12345", Token = null},
                new User() {Id = 4, Username = "Timosija", Password = "12345", Token = null},
                new User() {Id = 5, Username = "GrizzlyKiller", Password = "12345", Token = null},
            };
            foreach (User s in users)
            {
                context.Users.Add(s);
            }

            context.SaveChanges();
        }
    }
}
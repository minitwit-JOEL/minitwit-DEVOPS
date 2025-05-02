using minitwit.Domain.Entities;
using minitwit.Infrastructure.Data;

public static class Util
{
    public static void SeedDatabase(ApplicationDbContext context)
    {
        if (context.Users.Any()) return;
        
        var u1Salt = BCrypt.Net.BCrypt.GenerateSalt();
        var u2Salt = BCrypt.Net.BCrypt.GenerateSalt();
        var u3Salt = BCrypt.Net.BCrypt.GenerateSalt();

        var alice = new User {
            Username = "alice",
            Email = "alice@example.com",
            Salt = u1Salt,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("alicepass" + u1Salt)
        };
        var bob = new User {
            Username = "bob",
            Email = "bob@example.com",
            Salt = u2Salt,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("bobpass" + u2Salt)
        };
        var charlie = new User {
            Username = "charlie",
            Email = "charlie@example.com",
            Salt = u3Salt,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("charliepass" + u3Salt)
        };

        context.Users.AddRange(alice, bob, charlie);
        context.SaveChanges();
        
        var follows = new List<Follow> {
            new Follow { WhoId = alice.Id,   WhomId = bob.Id   },
            new Follow { WhoId = bob.Id,     WhomId = charlie.Id }
        };
        context.Set<Follow>().AddRange(follows);
        context.SaveChanges();

        var now = DateTime.UtcNow;
        var messages = new[] {
            new Message { AuthorId = alice.Id,   Text = "Hello from Alice!",   CreatedAt = now.AddMinutes(-30) },
            new Message { AuthorId = alice.Id,   Text = "Another thought...",    CreatedAt = now.AddMinutes(-10) },
            new Message { AuthorId = bob.Id,     Text = "Bob checking in.",      CreatedAt = now.AddHours(-1)     },
            new Message { AuthorId = charlie.Id, Text = "Good morning, world!",  CreatedAt = now.AddMinutes(-5)  }
        };
        
        context.Messages.AddRange(messages);
        context.SaveChanges();
    }
    
    // Source: https://stackoverflow.com/questions/39791634/read-appsettings-json-values-in-net-core-test-project
    public static IConfiguration InitConfiguration()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables() 
            .Build();
        return config;
    }
}

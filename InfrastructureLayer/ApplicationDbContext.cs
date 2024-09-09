using DomainLayer.DbEnts;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayer;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext>options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Interest> Interests { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Interest>().HasData(
            new Interest {Id = 1, Name = "General"},
            new Interest {Id = 2, Name = "Frontend Libraries"},
            new Interest {Id = 3, Name = "Backend Libraries"},
            new Interest {Id = 4, Name = "Programming Languages"},
            new Interest {Id = 5, Name = "Mobile Development"},
            new Interest {Id = 6, Name = "Databases"},
            new Interest {Id = 7, Name = "Devops"},
            new Interest {Id = 8, Name = "Version Control"},
            new Interest {Id = 9, Name = "Containerization Virtualization"},
            new Interest {Id = 10, Name = "Testing Frameworks"},
            new Interest {Id = 11, Name = "Data Processing Tools"},
            new Interest {Id = 12, Name = "Machine Learning Tools"}
        );
    }
}
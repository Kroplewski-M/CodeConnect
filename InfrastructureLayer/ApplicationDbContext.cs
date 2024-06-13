using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayer;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext>options) : IdentityDbContext<ApplicationUser>(options)
{
    
}
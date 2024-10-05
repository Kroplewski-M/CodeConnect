using DomainLayer.DbEnts;
using DomainLayer.Entities.Auth;
using DomainLayer.Entities.Posts;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayer;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext>options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Interest> Interests { get; set; }
    public DbSet<TechInterests> TechInterests { get; set; }
    public DbSet<UserInterests> UserInterests { get; set; }
    
    public DbSet<Post> Posts { get; set; }
    public DbSet<PostLike> PostLikes { get; set; }
    
    public DbSet<Comment> Comments { get; set; }
    public DbSet<CommentLike> CommentLikes { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        SeedInterests(builder);
        SeedTechInterests(builder);
        
        //Post user
        builder.Entity<Post>().HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x=> x.CreatedByUserId)
            .OnDelete(DeleteBehavior.NoAction);
        // Post comments (one to many)
        builder.Entity<Post>().HasMany(x=> x.Comments)
            .WithOne(x => x.Post)
            .HasForeignKey(x=>x.PostId)
            .OnDelete(DeleteBehavior.Cascade);
        //Post likes
        builder.Entity<Post>().HasMany(x => x.Likes)
            .WithOne(x => x.Post)
            .HasForeignKey(x=>x.PostId)
            .OnDelete(DeleteBehavior.Cascade);
        //comment likes
        builder.Entity<Comment>().HasMany<CommentLike>(x=> x.Likes)
            .WithOne(x=> x.Comment)
            .HasForeignKey(x=> x.CommentId)
            .OnDelete(DeleteBehavior.NoAction);
    }
    private void SeedInterests(ModelBuilder builder){
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

    private void SeedTechInterests(ModelBuilder builder)
    {
        builder.Entity<TechInterests>().HasData(
            // General Tech Interests
            new TechInterests { Id = 1, Name = "Frontend Dev", InterestId = 1 },
            new TechInterests { Id = 2, Name = "Backend Dev", InterestId = 1 },
            new TechInterests { Id = 3, Name = "Devops", InterestId = 1 },
            new TechInterests { Id = 4, Name = "Mobile Dev", InterestId = 1 },
            new TechInterests { Id = 5, Name = "Cloud", InterestId = 1 },

            // Programming Languages
            new TechInterests { Id = 6, Name = "Python", InterestId = 4 },
            new TechInterests { Id = 7, Name = "JavaScript", InterestId = 4 },
            new TechInterests { Id = 8, Name = "Java", InterestId = 4 },
            new TechInterests { Id = 9, Name = "C#", InterestId = 4 },
            new TechInterests { Id = 10, Name = "C++", InterestId = 4 },
            new TechInterests { Id = 11, Name = "Ruby", InterestId = 4 },
            new TechInterests { Id = 12, Name = "PHP", InterestId = 4 },
            new TechInterests { Id = 13, Name = "Go", InterestId = 4 },
            new TechInterests { Id = 14, Name = "Rust", InterestId = 4 },
            new TechInterests { Id = 15, Name = "Swift", InterestId = 4 },
            new TechInterests { Id = 16, Name = "Kotlin", InterestId = 4 },
            new TechInterests { Id = 17, Name = "TypeScript", InterestId = 4 },
            new TechInterests { Id = 18, Name = "Dart", InterestId = 4 },
            new TechInterests { Id = 19, Name = "R", InterestId = 4 },
            new TechInterests { Id = 20, Name = "Objective-C", InterestId = 4 },
            new TechInterests { Id = 21, Name = "Scala", InterestId = 4 },
            new TechInterests { Id = 22, Name = "Perl", InterestId = 4 },
            new TechInterests { Id = 23, Name = "Haskell", InterestId = 4 },
            new TechInterests { Id = 24, Name = "Lua", InterestId = 4 },
            new TechInterests { Id = 25, Name = "Elixir", InterestId = 4 },
            new TechInterests { Id = 26, Name = "Clojure", InterestId = 4 },
            new TechInterests { Id = 27, Name = "F#", InterestId = 4 },
            new TechInterests { Id = 28, Name = "Visual Basic .NET", InterestId = 4 },
            new TechInterests { Id = 29, Name = "OCaml", InterestId = 4 },

            // Frontend Frameworks and Libraries
            new TechInterests { Id = 30, Name = "React", InterestId = 2 },
            new TechInterests { Id = 31, Name = "Angular", InterestId = 2 },
            new TechInterests { Id = 32, Name = "Vue.js", InterestId = 2 },
            new TechInterests { Id = 33, Name = "Svelte", InterestId = 2 },
            new TechInterests { Id = 34, Name = "Ember.js", InterestId = 2 },
            new TechInterests { Id = 35, Name = "Backbone.js", InterestId = 2 },
            new TechInterests { Id = 36, Name = "Next.js", InterestId = 2 },
            new TechInterests { Id = 37, Name = "Nuxt.js", InterestId = 2 },
            new TechInterests { Id = 38, Name = "jQuery", InterestId = 2 },
            new TechInterests { Id = 39, Name = "Bootstrap", InterestId = 2 },
            new TechInterests { Id = 40, Name = "Tailwind CSS", InterestId = 2 },
            new TechInterests { Id = 41, Name = "Foundation", InterestId = 2 },
            new TechInterests { Id = 42, Name = "Semantic UI", InterestId = 2 },
            new TechInterests { Id = 43, Name = "Bulma", InterestId = 2 },
            new TechInterests { Id = 44, Name = "Blazor", InterestId = 2 },
            new TechInterests { Id = 45, Name = "Solid", InterestId = 2 },
            new TechInterests { Id = 46, Name = "Astro", InterestId = 2 },

            // Backend Frameworks and Libraries
            new TechInterests { Id = 47, Name = "Django", InterestId = 3 },
            new TechInterests { Id = 48, Name = "Flask", InterestId = 3 },
            new TechInterests { Id = 49, Name = "Ruby on Rails", InterestId = 3 },
            new TechInterests { Id = 50, Name = "Spring", InterestId = 3 },
            new TechInterests { Id = 51, Name = "Express", InterestId = 3 },
            new TechInterests { Id = 52, Name = "NestJS", InterestId = 3 },
            new TechInterests { Id = 53, Name = "Laravel", InterestId = 3 },
            new TechInterests { Id = 54, Name = "ASP.NET Core", InterestId = 3 },
            new TechInterests { Id = 55, Name = "Koa", InterestId = 3 },
            new TechInterests { Id = 56, Name = "Phoenix", InterestId = 3 },
            new TechInterests { Id = 57, Name = "Play Framework", InterestId = 3 },
            new TechInterests { Id = 58, Name = "Symfony", InterestId = 3 },
            new TechInterests { Id = 59, Name = "CakePHP", InterestId = 3 },
            new TechInterests { Id = 60, Name = "FastAPI", InterestId = 3 },
            new TechInterests { Id = 61, Name = "Gin Gonic", InterestId = 3 },

            // Mobile Development
            new TechInterests { Id = 62, Name = "Flutter", InterestId = 5 },
            new TechInterests { Id = 63, Name = "React Native", InterestId = 5 },
            new TechInterests { Id = 64, Name = "Swift", InterestId = 5 },
            new TechInterests { Id = 65, Name = "Kotlin", InterestId = 5 },
            new TechInterests { Id = 66, Name = "Xamarin", InterestId = 5 },
            new TechInterests { Id = 67, Name = "Ionic", InterestId = 5 },
            new TechInterests { Id = 68, Name = "Apache Cordova", InterestId = 5 },
            new TechInterests { Id = 69, Name = "Unity", InterestId = 5 },
            new TechInterests { Id = 70, Name = "Android SDK", InterestId = 5 },
            new TechInterests { Id = 71, Name = "iOS SDK", InterestId = 5 },

            // Databases
            new TechInterests { Id = 72, Name = "MySQL", InterestId = 6 },
            new TechInterests { Id = 73, Name = "PostgreSQL", InterestId = 6 },
            new TechInterests { Id = 74, Name = "MongoDB", InterestId = 6 },
            new TechInterests { Id = 75, Name = "SQLite", InterestId = 6 },
            new TechInterests { Id = 76, Name = "Oracle DB", InterestId = 6 },
            new TechInterests { Id = 77, Name = "Microsoft SQL Server", InterestId = 6 },
            new TechInterests { Id = 78, Name = "Redis", InterestId = 6 },
            new TechInterests { Id = 79, Name = "Elasticsearch", InterestId = 6 },
            new TechInterests { Id = 80, Name = "Cassandra", InterestId = 6 },
            new TechInterests { Id = 81, Name = "Firebase Realtime Database", InterestId = 6 },
            new TechInterests { Id = 82, Name = "DynamoDB", InterestId = 6 },
            new TechInterests { Id = 83, Name = "MariaDB", InterestId = 6 },
            new TechInterests { Id = 84, Name = "CouchDB", InterestId = 6 },
            new TechInterests { Id = 85, Name = "Neo4j", InterestId = 6 },
            new TechInterests { Id = 86, Name = "Supabase", InterestId = 6 },

            // DevOps Tools
            new TechInterests { Id = 87, Name = "Docker", InterestId = 7 },
            new TechInterests { Id = 88, Name = "Kubernetes", InterestId = 7 },
            new TechInterests { Id = 89, Name = "Jenkins", InterestId = 7 },
            new TechInterests { Id = 90, Name = "GitLab CI/CD", InterestId = 7 },
            new TechInterests { Id = 91, Name = "Travis CI", InterestId = 7 },
            new TechInterests { Id = 92, Name = "CircleCI", InterestId = 7 },
            new TechInterests { Id = 93, Name = "Terraform", InterestId = 7 },
            new TechInterests { Id = 94, Name = "Ansible", InterestId = 7 },
            new TechInterests { Id = 95, Name = "Puppet", InterestId = 7 },
            new TechInterests { Id = 96, Name = "Chef", InterestId = 7 },
            new TechInterests { Id = 97, Name = "Vagrant", InterestId = 7 },
            new TechInterests { Id = 98, Name = "AWS", InterestId = 7 },
            new TechInterests { Id = 99, Name = "Azure", InterestId = 7 },
            new TechInterests { Id = 100, Name = "Google Cloud Platform", InterestId = 7 },
            new TechInterests { Id = 101, Name = "Heroku", InterestId = 7 },
            new TechInterests { Id = 102, Name = "Netlify", InterestId = 7 },
            new TechInterests { Id = 103, Name = "DigitalOcean", InterestId = 7 },
            new TechInterests { Id = 104, Name = "GitHub Actions", InterestId = 7 },
            new TechInterests { Id = 105, Name = "Bamboo", InterestId = 7 },

            // Version Control
            new TechInterests { Id = 106, Name = "Git", InterestId = 8 },
            new TechInterests { Id = 107, Name = "Subversion (SVN)", InterestId = 8 },
            new TechInterests { Id = 108, Name = "Mercurial", InterestId = 8 },
            new TechInterests { Id = 109, Name = "Bitbucket", InterestId = 8 },
            new TechInterests { Id = 110, Name = "GitHub", InterestId = 8 },
            new TechInterests { Id = 111, Name = "GitLab", InterestId = 8 },
            new TechInterests { Id = 112, Name = "Perforce", InterestId = 8 },

            // Containerization/Virtualization
            new TechInterests { Id = 113, Name = "Docker", InterestId = 9 },
            new TechInterests { Id = 114, Name = "Kubernetes", InterestId = 9 },
            new TechInterests { Id = 115, Name = "OpenShift", InterestId = 9 },
            new TechInterests { Id = 116, Name = "Vagrant", InterestId = 9 },
            new TechInterests { Id = 117, Name = "VMware", InterestId = 9 },
            new TechInterests { Id = 118, Name = "Hyper-V", InterestId = 9 },
            new TechInterests { Id = 119, Name = "Parallels", InterestId = 9 },
            new TechInterests { Id = 120, Name = "VirtualBox", InterestId = 9 },

            // Testing Frameworks
            new TechInterests { Id = 121, Name = "JUnit", InterestId = 10 },
            new TechInterests { Id = 122, Name = "Mocha", InterestId = 10 },
            new TechInterests { Id = 123, Name = "Jest", InterestId = 10 },
            new TechInterests { Id = 124, Name = "RSpec", InterestId = 10 },
            new TechInterests { Id = 125, Name = "Selenium", InterestId = 10 },
            new TechInterests { Id = 126, Name = "Cypress", InterestId = 10 },
            new TechInterests { Id = 127, Name = "Puppeteer", InterestId = 10 },
            new TechInterests { Id = 128, Name = "Postman", InterestId = 10 },
            new TechInterests { Id = 129, Name = "PyTest", InterestId = 10 },
            new TechInterests { Id = 130, Name = "TestNG", InterestId = 10 },
            new TechInterests { Id = 131, Name = "xUnit", InterestId = 10 },
            new TechInterests { Id = 132, Name = "Robot Framework", InterestId = 10 },
            new TechInterests { Id = 133, Name = "Chai", InterestId = 10 },
            new TechInterests { Id = 134, Name = "Cucumber", InterestId = 10 },

            // Data Processing Tools
            new TechInterests { Id = 135, Name = "Apache Hadoop", InterestId = 11 },
            new TechInterests { Id = 136, Name = "Apache Spark", InterestId = 11 },
            new TechInterests { Id = 137, Name = "Kafka", InterestId = 11 },
            new TechInterests { Id = 138, Name = "Flink", InterestId = 11 },
            new TechInterests { Id = 139, Name = "Pandas", InterestId = 11 },
            new TechInterests { Id = 140, Name = "NumPy", InterestId = 11 },
            new TechInterests { Id = 141, Name = "Apache Beam", InterestId = 11 },
            new TechInterests { Id = 142, Name = "TensorFlow", InterestId = 11 },
            new TechInterests { Id = 143, Name = "PyTorch", InterestId = 11 },
            new TechInterests { Id = 144, Name = "Dask", InterestId = 11 },
            new TechInterests { Id = 145, Name = "HBase", InterestId = 11 },
            new TechInterests { Id = 146, Name = "Hive", InterestId = 11 },
            new TechInterests { Id = 147, Name = "Pig", InterestId = 11 },
            new TechInterests { Id = 148, Name = "Storm", InterestId = 11 },

            // Machine Learning Frameworks
            new TechInterests { Id = 149, Name = "TensorFlow", InterestId = 12 },
            new TechInterests { Id = 150, Name = "PyTorch", InterestId = 12 },
            new TechInterests { Id = 151, Name = "scikit-learn", InterestId = 12 },
            new TechInterests { Id = 152, Name = "Keras", InterestId = 12 },
            new TechInterests { Id = 153, Name = "XGBoost", InterestId = 12 },
            new TechInterests { Id = 154, Name = "LightGBM", InterestId = 12 },
            new TechInterests { Id = 155, Name = "MXNet", InterestId = 12 },
            new TechInterests { Id = 156, Name = "Caffe", InterestId = 12 },
            new TechInterests { Id = 157, Name = "Theano", InterestId = 12 },
            new TechInterests { Id = 158, Name = "Torch", InterestId = 12 },
            new TechInterests { Id = 159, Name = "CNTK", InterestId = 12 },
            new TechInterests { Id = 160, Name = "Chainer", InterestId = 12 }
        );
    }
}


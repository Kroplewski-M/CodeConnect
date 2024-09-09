using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InfrastructureLayer.Migrations
{
    /// <inheritdoc />
    public partial class seedingTechInterestsAndUserInterests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TechInterests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InterestId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechInterests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TechInterests_Interests_InterestId",
                        column: x => x.InterestId,
                        principalTable: "Interests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserInterests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TechInterestId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInterests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserInterests_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserInterests_TechInterests_TechInterestId",
                        column: x => x.TechInterestId,
                        principalTable: "TechInterests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "TechInterests",
                columns: new[] { "Id", "InterestId", "Name" },
                values: new object[,]
                {
                    { 1, 1, "Frontend Dev" },
                    { 2, 1, "Backend Dev" },
                    { 3, 1, "Devops" },
                    { 4, 1, "Mobile Dev" },
                    { 5, 1, "Cloud" },
                    { 6, 4, "Python" },
                    { 7, 4, "JavaScript" },
                    { 8, 4, "Java" },
                    { 9, 4, "C#" },
                    { 10, 4, "C++" },
                    { 11, 4, "Ruby" },
                    { 12, 4, "PHP" },
                    { 13, 4, "Go" },
                    { 14, 4, "Rust" },
                    { 15, 4, "Swift" },
                    { 16, 4, "Kotlin" },
                    { 17, 4, "TypeScript" },
                    { 18, 4, "Dart" },
                    { 19, 4, "R" },
                    { 20, 4, "Objective-C" },
                    { 21, 4, "Scala" },
                    { 22, 4, "Perl" },
                    { 23, 4, "Haskell" },
                    { 24, 4, "Lua" },
                    { 25, 4, "Elixir" },
                    { 26, 4, "Clojure" },
                    { 27, 4, "F#" },
                    { 28, 4, "Visual Basic .NET" },
                    { 29, 4, "OCaml" },
                    { 30, 2, "React" },
                    { 31, 2, "Angular" },
                    { 32, 2, "Vue.js" },
                    { 33, 2, "Svelte" },
                    { 34, 2, "Ember.js" },
                    { 35, 2, "Backbone.js" },
                    { 36, 2, "Next.js" },
                    { 37, 2, "Nuxt.js" },
                    { 38, 2, "jQuery" },
                    { 39, 2, "Bootstrap" },
                    { 40, 2, "Tailwind CSS" },
                    { 41, 2, "Foundation" },
                    { 42, 2, "Semantic UI" },
                    { 43, 2, "Bulma" },
                    { 44, 2, "Blazor" },
                    { 45, 2, "Solid" },
                    { 46, 2, "Astro" },
                    { 47, 3, "Django" },
                    { 48, 3, "Flask" },
                    { 49, 3, "Ruby on Rails" },
                    { 50, 3, "Spring" },
                    { 51, 3, "Express" },
                    { 52, 3, "NestJS" },
                    { 53, 3, "Laravel" },
                    { 54, 3, "ASP.NET Core" },
                    { 55, 3, "Koa" },
                    { 56, 3, "Phoenix" },
                    { 57, 3, "Play Framework" },
                    { 58, 3, "Symfony" },
                    { 59, 3, "CakePHP" },
                    { 60, 3, "FastAPI" },
                    { 61, 3, "Gin Gonic" },
                    { 62, 5, "Flutter" },
                    { 63, 5, "React Native" },
                    { 64, 5, "Swift" },
                    { 65, 5, "Kotlin" },
                    { 66, 5, "Xamarin" },
                    { 67, 5, "Ionic" },
                    { 68, 5, "Apache Cordova" },
                    { 69, 5, "Unity" },
                    { 70, 5, "Android SDK" },
                    { 71, 5, "iOS SDK" },
                    { 72, 6, "MySQL" },
                    { 73, 6, "PostgreSQL" },
                    { 74, 6, "MongoDB" },
                    { 75, 6, "SQLite" },
                    { 76, 6, "Oracle DB" },
                    { 77, 6, "Microsoft SQL Server" },
                    { 78, 6, "Redis" },
                    { 79, 6, "Elasticsearch" },
                    { 80, 6, "Cassandra" },
                    { 81, 6, "Firebase Realtime Database" },
                    { 82, 6, "DynamoDB" },
                    { 83, 6, "MariaDB" },
                    { 84, 6, "CouchDB" },
                    { 85, 6, "Neo4j" },
                    { 86, 6, "Supabase" },
                    { 87, 7, "Docker" },
                    { 88, 7, "Kubernetes" },
                    { 89, 7, "Jenkins" },
                    { 90, 7, "GitLab CI/CD" },
                    { 91, 7, "Travis CI" },
                    { 92, 7, "CircleCI" },
                    { 93, 7, "Terraform" },
                    { 94, 7, "Ansible" },
                    { 95, 7, "Puppet" },
                    { 96, 7, "Chef" },
                    { 97, 7, "Vagrant" },
                    { 98, 7, "AWS" },
                    { 99, 7, "Azure" },
                    { 100, 7, "Google Cloud Platform" },
                    { 101, 7, "Heroku" },
                    { 102, 7, "Netlify" },
                    { 103, 7, "DigitalOcean" },
                    { 104, 7, "GitHub Actions" },
                    { 105, 7, "Bamboo" },
                    { 106, 8, "Git" },
                    { 107, 8, "Subversion (SVN)" },
                    { 108, 8, "Mercurial" },
                    { 109, 8, "Bitbucket" },
                    { 110, 8, "GitHub" },
                    { 111, 8, "GitLab" },
                    { 112, 8, "Perforce" },
                    { 113, 9, "Docker" },
                    { 114, 9, "Kubernetes" },
                    { 115, 9, "OpenShift" },
                    { 116, 9, "Vagrant" },
                    { 117, 9, "VMware" },
                    { 118, 9, "Hyper-V" },
                    { 119, 9, "Parallels" },
                    { 120, 9, "VirtualBox" },
                    { 121, 10, "JUnit" },
                    { 122, 10, "Mocha" },
                    { 123, 10, "Jest" },
                    { 124, 10, "RSpec" },
                    { 125, 10, "Selenium" },
                    { 126, 10, "Cypress" },
                    { 127, 10, "Puppeteer" },
                    { 128, 10, "Postman" },
                    { 129, 10, "PyTest" },
                    { 130, 10, "TestNG" },
                    { 131, 10, "xUnit" },
                    { 132, 10, "Robot Framework" },
                    { 133, 10, "Chai" },
                    { 134, 10, "Cucumber" },
                    { 135, 11, "Apache Hadoop" },
                    { 136, 11, "Apache Spark" },
                    { 137, 11, "Kafka" },
                    { 138, 11, "Flink" },
                    { 139, 11, "Pandas" },
                    { 140, 11, "NumPy" },
                    { 141, 11, "Apache Beam" },
                    { 142, 11, "TensorFlow" },
                    { 143, 11, "PyTorch" },
                    { 144, 11, "Dask" },
                    { 145, 11, "HBase" },
                    { 146, 11, "Hive" },
                    { 147, 11, "Pig" },
                    { 148, 11, "Storm" },
                    { 149, 12, "TensorFlow" },
                    { 150, 12, "PyTorch" },
                    { 151, 12, "scikit-learn" },
                    { 152, 12, "Keras" },
                    { 153, 12, "XGBoost" },
                    { 154, 12, "LightGBM" },
                    { 155, 12, "MXNet" },
                    { 156, 12, "Caffe" },
                    { 157, 12, "Theano" },
                    { 158, 12, "Torch" },
                    { 159, 12, "CNTK" },
                    { 160, 12, "Chainer" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TechInterests_InterestId",
                table: "TechInterests",
                column: "InterestId");

            migrationBuilder.CreateIndex(
                name: "IX_UserInterests_TechInterestId",
                table: "UserInterests",
                column: "TechInterestId");

            migrationBuilder.CreateIndex(
                name: "IX_UserInterests_UserId",
                table: "UserInterests",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserInterests");

            migrationBuilder.DropTable(
                name: "TechInterests");
        }
    }
}

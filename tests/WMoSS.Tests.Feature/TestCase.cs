using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using System.IO;
using WMoSS.Entities;
using Bogus;
using System.Linq;
using WMoSS.Tests.TestUtils;

namespace WMoSS.Tests.Feature
{

    // In razor page integration testing, tests will return a 500 response error
    // without the following lines of code in .csproj file in the WMoSS.Tests.Feature project
    //
    //<Target Name="CopyDepsFiles" AfterTargets="Build" Condition="'$(TargetFramework)'!=''">
    //  <ItemGroup>
    //      <DepsFilePaths Include="$([System.IO.Path]::ChangeExtension('%(_ResolvedProjectReferencePaths.FullPath)', '.deps.json'))" />
    //  </ItemGroup>
    //  <Copy SourceFiles="%(DepsFilePaths.FullPath)" DestinationFolder="$(OutputPath)" Condition="Exists('%(DepsFilePaths.FullPath)')" />
    //</Target>


    /// <summary>
    /// Superclass for all feature tests
    /// </summary>
    public abstract class TestCase : IDisposable
    {
        protected readonly TestServer server;
        protected readonly HttpClient client;

        private IEntityFactory factory;

        protected TestCase()
        {
            var webfolder = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\..\src", @"WMoSS.Web");
            var webhostBuilder = new WebHostBuilder()
                .UseEnvironment("Testing")
                .UseContentRoot(webfolder)
                .UseStartup<WMoSS.Web.Startup>();

            server = new TestServer(webhostBuilder);
            client = server.CreateClient();

            factory = new ExcelEntityFactory();

            DefineEntities();
        }

        protected void Define<T>(Func<T> definition) where T : class
        {
            factory.Define<T>(definition);
        }

        public void Dispose()
        {
            client.Dispose();
            server.Dispose();
        }

        protected T Make<T>() where T : class
        {
            return Make<T>(1).FirstOrDefault();
        }

        protected IEnumerable<T> Make<T>(int numberOfEntities = 1) where T : class
        {
            return factory.Make<T>(numberOfEntities);
        }

        [Fact]
        public void SmokeTest_EntityGeneration()
        {
            var id = 2;
            factory.Define<Movie>(() =>
            {
                return new Faker<Movie>()
                .RuleFor(m => m.Id, f => id++)
                .RuleFor(m => m.Title, f => f.Lorem.Text())
                .RuleFor(m => m.Genre, f => f.Lorem.Word())
                .RuleFor(m => m.ReleaseDate, f => DateTime.UtcNow.AddDays(f.Random.Int(1, 365)))
                .RuleFor(m => m.RuntimeMinutes, f => f.Random.Number(100, 200))
                .RuleFor(m => m.Description, f => f.Lorem.Paragraph())
                .RuleFor(m => m.PosterFileName, f => f.Image.Image())
                .RuleFor(m => m.Classification, f => f.PickRandom("G", "PG", "MA", "MA15+", "R18+", "X18+"))
                .Generate();
            });

            var movie = factory.Make<Movie>();
            Assert.NotNull(movie);
        }

        [Fact]
        public void SmokeTest_MultipleEntityGeneration()
        {
            var id = 2;
            factory.Define<Movie>(() =>
            {
                return new Faker<Movie>()
                .RuleFor(m => m.Id, f => id++)
                .RuleFor(m => m.Title, f => f.Lorem.Text())
                .RuleFor(m => m.Genre, f => f.Lorem.Word())
                .RuleFor(m => m.ReleaseDate, f => DateTime.UtcNow.AddDays(f.Random.Int(1, 365)))
                .RuleFor(m => m.RuntimeMinutes, f => f.Random.Number(100, 200))
                .RuleFor(m => m.Description, f => f.Lorem.Paragraph())
                .RuleFor(m => m.PosterFileName, f => f.Image.Image())
                .RuleFor(m => m.Classification, f => f.PickRandom("G", "PG", "MA", "MA15+", "R18+", "X18+"))
                .Generate();
            });

            var movies = factory.Make<Movie>(2);
            Assert.Equal(movies.Count(), 2);
        }

        private void DefineEntities()
        {
            int theaterId = 2;
            Define<Theater>(() =>
            {
                return new Faker<Theater>()
                .RuleFor(t => t.Id, f => theaterId++)
                .RuleFor(t => t.Name, f => $"Theater #{theaterId - 2}")
                .RuleFor(t => t.Capacity, f => 50)
                .RuleFor(t => t.Address, f => "123 Lygon Street, Melbourne")
                .Generate();
            });

            int movieId = 2;
            Define<Movie>(() =>
            {
                return new Faker<Movie>()
                .RuleFor(m => m.Id, f => movieId++)
                .RuleFor(m => m.Title, f => f.Lorem.Text())
                .RuleFor(m => m.Genre, f => f.Lorem.Word())
                .RuleFor(m => m.ReleaseDate, f => DateTime.UtcNow.AddDays(f.Random.Int(1, 365)))
                .RuleFor(m => m.RuntimeMinutes, f => f.Random.Number(100, 200))
                .RuleFor(m => m.Description, f => f.Lorem.Paragraph())
                .RuleFor(m => m.PosterFileName, f => f.Image.Image())
                .RuleFor(m => m.Classification, f => f.PickRandom("G", "PG", "MA", "MA15+", "R18+", "X18+"))
                .Generate();
            });

            int movieSessionId = 2;
            Define<MovieSession>(() =>
            {
                Random random = new Random();
                int days = random.Next(1, 4);
                int hour = random.Next(1, 4);
                DateTime schedule = DateTime.Today.AddDays(days);
                schedule.AddHours(hour * 4.5);
                string scheduleStr = schedule.ToString("s", System.Globalization.CultureInfo.InvariantCulture);

                return new Faker<MovieSession>()
                .RuleFor(ms => ms.Id, f => movieSessionId++)
                .RuleFor(ms => ms.TicketPrice, f => 20.00)
                .RuleFor(ms => ms.ScheduledAt, f => DateTime.UtcNow.AddDays(f.Random.Int(1, 365)))
                .RuleFor(ms => ms.TheaterId, f => f.Random.Int(2, 6))
                .Generate();
            });
        }
    }
    
}

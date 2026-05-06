using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Pendlerapp.Controllers;
using Pendlerapp.Data;
using Pendlerapp.Models;
using Pendlerapp.Services;
using System.Security.Claims;

namespace Pendlerapp.Tests
{
    public class FavorittControllerTests
    {
        private ApplicationDbContext LagDbContext(string databaseNavn)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseNavn)
                .Options;
            return new ApplicationDbContext(options);
        }

        private FavorittController LagController(ApplicationDbContext context, string brukerId = "test-bruker-id")
        {
            var userManagerMock = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);

            userManagerMock.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(brukerId);

            var enturServiceMock = new Mock<EnturService>(Mock.Of<HttpClient>());

            var controller = new FavorittController(context, enturServiceMock.Object, userManagerMock.Object);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, brukerId)
                    }, "test"))
                }
            };

            return controller;
        }

        /// <summary>
        /// Tester at Index kun returnerer favoritter for innlogget bruker.
        /// </summary>
        [Fact]
        public async Task Index_ReturnererKunBrukersFavoritter()
        {
            var context = LagDbContext("Index_Test");
            context.Favoritter.AddRange(
                new Favoritt { Navn = "Min rute", BrukerId = "test-bruker-id", FraStoppested = "Oslo S", FraStoppestedId = "NSR:StopPlace:1", TilStoppested = "Drammen", TilStoppestedId = "NSR:StopPlace:2" },
                new Favoritt { Navn = "Andres rute", BrukerId = "annen-bruker-id", FraStoppested = "Bergen", FraStoppestedId = "NSR:StopPlace:3", TilStoppested = "Voss", TilStoppestedId = "NSR:StopPlace:4" }
            );
            await context.SaveChangesAsync();

            var controller = LagController(context);
            var result = await controller.Index() as ViewResult;
            var model = result?.Model as IEnumerable<Favoritt>;

            Assert.NotNull(model);
            Assert.Single(model);
            Assert.Equal("Min rute", model.First().Navn);
        }

        /// <summary>
        /// Tester at Create legger til en ny favoritt i databasen.
        /// </summary>
        [Fact]
        public async Task Create_LeggerTilFavorittIDatabasen()
        {
            var context = LagDbContext("Create_Test");
            var controller = LagController(context);

            var favoritt = new Favoritt
            {
                Navn = "Jobb",
                FraStoppested = "Oslo S",
                FraStoppestedId = "NSR:StopPlace:1",
                TilStoppested = "Drammen",
                TilStoppestedId = "NSR:StopPlace:2"
            };

            await controller.Create(favoritt);

            Assert.Equal(1, context.Favoritter.Count());
            Assert.Equal("Jobb", context.Favoritter.First().Navn);
            Assert.Equal("test-bruker-id", context.Favoritter.First().BrukerId);
        }

        /// <summary>
        /// Tester at Delete fjerner en favoritt fra databasen.
        /// </summary>
        [Fact]
        public async Task Delete_FjernerFavorittFraDatabasen()
        {
            var context = LagDbContext("Delete_Test");
            var favoritt = new Favoritt
            {
                Navn = "Slett meg",
                BrukerId = "test-bruker-id",
                FraStoppested = "Oslo S",
                FraStoppestedId = "NSR:StopPlace:1",
                TilStoppested = "Drammen",
                TilStoppestedId = "NSR:StopPlace:2"
            };
            context.Favoritter.Add(favoritt);
            await context.SaveChangesAsync();

            var controller = LagController(context);
            await controller.DeleteConfirmed(favoritt.Id);

            Assert.Equal(0, context.Favoritter.Count());
        }

        /// <summary>
        /// Tester at Delete ikke fjerner favoritter som tilhører andre brukere.
        /// </summary>
        [Fact]
        public async Task Delete_FjernerIkkeAndreBrukersFavoritter()
        {
            var context = LagDbContext("Delete_AnnenBruker_Test");
            var favoritt = new Favoritt
            {
                Navn = "Andres rute",
                BrukerId = "annen-bruker-id",
                FraStoppested = "Bergen",
                FraStoppestedId = "NSR:StopPlace:3",
                TilStoppested = "Voss",
                TilStoppestedId = "NSR:StopPlace:4"
            };
            context.Favoritter.Add(favoritt);
            await context.SaveChangesAsync();

            var controller = LagController(context, "test-bruker-id");
            await controller.DeleteConfirmed(favoritt.Id);

            Assert.Equal(1, context.Favoritter.Count());
        }

        /// <summary>
        /// Tester at Edit oppdaterer en eksisterende favoritt.
        /// </summary>
        [Fact]
        public async Task Edit_OppdatererFavoritt()
        {
            var context = LagDbContext("Edit_Test");
            var favoritt = new Favoritt
            {
                Navn = "Gammelt navn",
                BrukerId = "test-bruker-id",
                FraStoppested = "Oslo S",
                FraStoppestedId = "NSR:StopPlace:1",
                TilStoppested = "Drammen",
                TilStoppestedId = "NSR:StopPlace:2",
                Opprettet = DateTime.Now
            };
            context.Favoritter.Add(favoritt);
            await context.SaveChangesAsync();

            favoritt.Navn = "Nytt navn";
            var controller = LagController(context);
            await controller.Edit(favoritt.Id, favoritt);

            var oppdatert = context.Favoritter.First();
            Assert.Equal("Nytt navn", oppdatert.Navn);
        }
    }
}
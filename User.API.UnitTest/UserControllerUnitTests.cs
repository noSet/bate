using System;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using User.API.Controllers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using Microsoft.AspNetCore.JsonPatch;
using User.API.Models;
using User.API.Data;
using System.Collections.Generic;
using System.Linq;
using DotNetCore.CAP;

namespace User.API.UnitTest
{
    public class UserControllerUnitTests
    {
        private (UserController controller, UserContext context) GetUserController()
        {
            var context = GetUserContext();
            var loggerMoq = new Mock<ILogger<API.Controllers.UserController>>();
            var logger = loggerMoq.Object;
            ICapPublisher capPublisher = null;
            return (new UserController(context, capPublisher, logger), context);

            // local method
            UserContext GetUserContext()
            {
                var options = new DbContextOptionsBuilder<Data.UserContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options;

                var userContext = new UserContext(options);

                userContext.Users.Add(new AppUser
                {
                    Id = 1,
                    Name = "cbb"
                });

                userContext.SaveChanges();

                return userContext;
            }
        }

        [Fact]
        public async Task Get_ReturnRigthUser_WithExpectedParametes()
        {
            var controller = GetUserController().controller;
            var response = await controller.Get();

            var result = response.Should().BeOfType<JsonResult>().Subject;
            var appUser = result.Value.Should().BeAssignableTo<Models.AppUser>().Subject;
            appUser.Id.Should().Be(1);
            appUser.Name.Should().Be("cbb");
        }

        [Fact]
        public async Task Patch_ReturnNewName_WithExpectedNewNameParameter()
        {
            var (controller, context) = GetUserController();
            var document = new JsonPatchDocument<AppUser>();
            document.Replace(user => user.Name, "xhy");

            var response = await controller.Patch(document);
            var result = response.Should().BeOfType<JsonResult>().Subject;

            // assert response
            var appUser = result.Value.Should().BeAssignableTo<Models.AppUser>().Subject;
            appUser.Name.Should().Be("xhy");

            // assert name value in ef context
            var userModel = await context.Users.SingleOrDefaultAsync(u => u.Id == 1);
            userModel.Should().NotBeNull();
            userModel.Name.Should().Be("xhy");
        }

        [Fact]
        public async Task Patch_ReturnNewProperties_WithAddNewProperty()
        {
            var (controller, context) = GetUserController();
            var document = new JsonPatchDocument<AppUser>();
            document.Replace(user => user.Properties, new List<UserProperty> {
                new UserProperty{ Key = "fin_industry",Value = "»¥ÁªÍø", Text = "»¥ÁªÍø" }
            });

            var response = await controller.Patch(document);
            var result = response.Should().BeOfType<JsonResult>().Subject;

            // assert response
            var appUser = result.Value.Should().BeAssignableTo<Models.AppUser>().Subject;
            appUser.Properties.Count.Should().Be(1);
            appUser.Properties.First().Value.Should().Be("»¥ÁªÍø");
            appUser.Properties.First().Key.Should().Be("fin_industry");

            // assert name value in ef context
            var userModel = await context.Users.SingleOrDefaultAsync(u => u.Id == 1);
            appUser.Properties.Count.Should().Be(1);
            appUser.Properties.First().Value.Should().Be("»¥ÁªÍø");
            appUser.Properties.First().Key.Should().Be("fin_industry");
        }

        [Fact]
        public async Task Patch_ReturnNewProperties_WithRemoverProperty()
        {
            var (controller, context) = GetUserController();
            var document = new JsonPatchDocument<AppUser>();
            document.Replace(user => user.Properties, new List<UserProperty>());

            var response = await controller.Patch(document);
            var result = response.Should().BeOfType<JsonResult>().Subject;

            // assert response
            var appUser = result.Value.Should().BeAssignableTo<Models.AppUser>().Subject;
            appUser.Properties.Should().BeEmpty();

            // assert name value in ef context
            var userModel = await context.Users.SingleOrDefaultAsync(u => u.Id == 1);
            appUser.Properties.Should().BeEmpty();

        }
    }
}

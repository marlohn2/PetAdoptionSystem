﻿using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PetAdoptionSystem.Api.Controllers;
using PetAdoptionSystem.Application.Dtos;
using PetAdoptionSystem.Application.Interfaces;
using PetAdoptionSystem.Tests.Faker;

namespace PetAdoptionSystem.Tests
{
    public class UsersControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly UsersController _usersController;

        public UsersControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _usersController = new UsersController(_userServiceMock.Object);
        }

        [Fact]
        public async Task Register_ShouldReturnCreatedAtAction()
        {
            // Arrange
            var userDto = FakeDataGenerator.UserRequestDto.Generate();
            var createdUserDto = FakeDataGenerator.UserResponseDto.Generate();
            _userServiceMock.Setup(service => service.Register(It.IsAny<UserRequestDto>())).ReturnsAsync(createdUserDto);

            // Act
            var result = await _usersController.Register(userDto) as CreatedAtActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nameof(UsersController.GetUserById), result.ActionName);
            Assert.NotNull(result.RouteValues);
            Assert.True(result.RouteValues.ContainsKey("id"));
            Assert.Equal(createdUserDto.Id, result.RouteValues["id"]);
            Assert.Equal(createdUserDto, result.Value);
        }

        [Fact]
        public async Task Login_ShouldReturnOkWithToken()
        {
            // Arrange
            var loginRequest = FakeDataGenerator.UserRequestDto.Generate();
            var token = "fake-jwt-token";
            _userServiceMock.Setup(service => service.Login(loginRequest.Username, loginRequest.Password)).ReturnsAsync(token);

            // Act
            var result = await _usersController.Login(loginRequest) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(token, result.Value);
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenTokenIsNull()
        {
            // Arrange
            var loginRequest = FakeDataGenerator.UserRequestDto.Generate();
            _userServiceMock.Setup(service => service.Login(loginRequest.Username, loginRequest.Password)).ReturnsAsync(string.Empty);

            // Act
            var result = await _usersController.Login(loginRequest) as UnauthorizedResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
        }

        [Fact]
        public async Task GetUserById_ShouldReturnOkWithUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userDto = FakeDataGenerator.UserResponseDto.Generate();
            _userServiceMock.Setup(service => service.GetUserById(It.IsAny<Guid>())).ReturnsAsync(userDto);

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }, "mock"));

            _usersController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userClaims }
            };

            // Act
            var result = await _usersController.GetUserById() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(userDto, result.Value);
        }

        [Fact]
        public async Task GetUserById_ShouldReturnNotFound_WhenUserIsNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userServiceMock.Setup(service => service.GetUserById(It.IsAny<Guid>())).ReturnsAsync((UserResponseDto?)null);

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }, "mock"));

            _usersController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userClaims }
            };

            // Act
            var result = await _usersController.GetUserById() as NotFoundResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }
    }
}

using Moq;
using MovieMate.BLL.Interfaces;
using MovieMate.BLL.Services;
using MovieMate.DAL.Interfaces;
using MovieMate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieMate.BLL.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IAuditLogService> _mockAuditLogService;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockAuditLogService = new Mock<IAuditLogService>();
            _userService = new UserService(_mockUserRepository.Object, _mockAuditLogService.Object);
        }

        [Fact]
        public async Task RegisterUserAsync_WithValidData_ShouldReturnSuccessAndUser()
        {
            // Arrange
            var username = "newuser";
            var email = "new@example.com";
            var password = "Password123!";
            var expectedNewUserId = 1;

            _mockUserRepository.Setup(repo => repo.GetByUsernameAsync(username))
                               .ReturnsAsync((User?)null);

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(email))
                               .ReturnsAsync((User?)null);

            _mockUserRepository.Setup(repo => repo.AddAsync(It.IsAny<User>()))
                               .ReturnsAsync(expectedNewUserId);

            _mockAuditLogService.Setup(service => service.LogActionAsync(
                                        It.IsAny<int?>(),
                                        It.IsAny<string>(),
                                        It.IsAny<string?>()))
                                .Returns(Task.CompletedTask);

            // Act
            var result = await _userService.RegisterUserAsync(username, email, password);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(expectedNewUserId, result.Data.UserId);
            Assert.Equal(username, result.Data.Username);
            Assert.Equal(email.ToLower(), result.Data.Email);
            Assert.Null(result.ErrorMessage);
        }
    }
}

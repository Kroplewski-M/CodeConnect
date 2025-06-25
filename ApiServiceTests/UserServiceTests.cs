using ApplicationLayer.APIServices;
using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.User;
using DomainLayer.Constants;
using DomainLayer.DbEnts;
using DomainLayer.Entities;
using DomainLayer.Entities.Auth;
using InfrastructureLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace ApiServiceTests;
[Collection("DatabaseCollection")]
public class UserServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly ApplicationDbContext _context;
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly UserService _userService;
    public UserServiceTests(DatabaseFixture databaseFixture)
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        _context = databaseFixture.Context;
        _cacheMock = new Mock<IMemoryCache>();

        _userService = new UserService(_userManagerMock.Object, _context, _cacheMock.Object);
    }
    [Fact]
    public async Task UpdateUserDetails_ReturnsSuccess_WhenValid()
    {
        // Arrange
        var form = new EditProfileForm { UserId = "test", FirstName = "John",LastName = "John", Dob = DateOnly.Parse("2012-09-19") };
        var user = new ApplicationUser { UserName = "test" };
    
        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _userService.UpdateUserDetails(form);

        // Assert
        Assert.True(result.Flag);
        Assert.Equal("Updated User Successfully", result.Message);
    }
    [Fact]
    public async Task UpdateUserDetails_NoFirstName_ReturnsBadService()
    {
        // Arrange
        var form = new EditProfileForm { UserId = "test", FirstName = "",LastName = "John", Dob = DateOnly.Parse("2012-09-19") };
        var user = new ApplicationUser { UserName = "test" };
    
        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _userService.UpdateUserDetails(form);

        // Assert
        Assert.False(result.Flag);
        _userManagerMock.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Never);
        _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }
    [Fact]
    public async Task UpdateUserDetails_NoLastName_ReturnsBadService()
    {
        // Arrange
        var form = new EditProfileForm { UserId = "test", FirstName = "Mat",LastName = "", Dob = DateOnly.Parse("2012-09-19") };
        var user = new ApplicationUser { UserName = "test" };
    
        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _userService.UpdateUserDetails(form);

        // Assert
        Assert.False(result.Flag);
        _userManagerMock.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Never);
        _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }
    [Fact]
    public async Task UpdateUserDetails_NoDob_ReturnsBadService()
    {
        // Arrange
        var form = new EditProfileForm { UserId = "test", FirstName = "Mat",LastName = "Kroplewski", Dob = null };
        var user = new ApplicationUser { UserName = "test" };
    
        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _userService.UpdateUserDetails(form);

        // Assert
        Assert.False(result.Flag);
        _userManagerMock.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Never);
        _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }
    [Fact]
    public async Task UpdateUserDetails_DobInTheFuture_ReturnsBadService()
    {
        // Arrange
        var form = new EditProfileForm { UserId = "test", FirstName = "Mat",LastName = "Kroplewski", Dob = DateOnly.Parse(DateTime.UtcNow.AddDays(10).Date.ToString("yyyy/MM/dd")) };
        var user = new ApplicationUser { UserName = "test" };
    
        _userManagerMock.Setup(x => x.FindByIdAsync(form.UserId)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _userService.UpdateUserDetails(form);

        // Assert
        Assert.False(result.Flag);
        _userManagerMock.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Never);
        _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }
    [Fact]
    public async Task GetUserDetails_ReturnsNull_WhenUserNotFound()
    {
        //Arrange
        _userManagerMock.Setup(x => x.FindByIdAsync("unknown")).ReturnsAsync((ApplicationUser)null!);
        //Act
        var result = await _userService.GetUserDetails("unknown");
        //Assert
        Assert.Null(result);
    }
    [Fact]
    public async Task GetUserDetails_EmptyUsername_ReturnsNull()
    {
        //Arrange
        
        //Act
        var result = await _userService.GetUserDetails("");
        //Assert
        Assert.Null(result);
    }
    [Fact]
    public async Task GetUserDetails_UserExists_ReturnsUserDetails()
    {
        //Arrange
        var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), UserName = "test", FirstName = "Mateusz", LastName = "Kroplewski", DOB = DateOnly.Parse("2012-09-19") };
        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id)).ReturnsAsync(user);
        //Act
        var result = await _userService.GetUserDetails(user.Id);
        //Assert
        Assert.NotNull(result);
        Assert.Equal(user.UserName, result.UserName);
        Assert.Equal(user.FirstName, result.FirstName);
        Assert.Equal(user.LastName, result.LastName);
        _userManagerMock.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Once);
    }
    [Fact]
    public async Task GetUserInterests_userNotFound_ReturnsBadService()
    {
        //Arrange
        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser)null!);
        //Act
        var result = await _userService.GetUserInterests("unknown");
        //Assert
        Assert.NotNull(result);
        Assert.False(result.Flag);
    }
    [Fact]
    public async Task GetUserInterests_EmptyUsername_ReturnsBadService()
    {
        //Arrange
 
        //Act
        var result = await _userService.GetUserInterests("");
        //Assert
        Assert.NotNull(result);
        Assert.False(result.Flag);
    }
    [Fact]
    public async Task GetUserInterests_UserExists_ReturnsGoodService()
    {
        //Arrange
        var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), UserName = "test", FirstName = "Mateusz", LastName = "Kroplewski", DOB = DateOnly.Parse("2012-09-19") };
        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id)).ReturnsAsync(user);
        //Act
        var result = await _userService.GetUserInterests(user.Id);
        //Assert
        Assert.NotNull(result);
        Assert.True(result.Flag);
        _userManagerMock.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Once);
    }
    [Fact]
    public async Task UpdateUserInterests_ValidUserAndInterests_ReturnsSuccess()
    {
        // Arrange
        var username = "test";
        var id = Guid.NewGuid().ToString(); 
        var user = new ApplicationUser { Id = id, UserName = username };
        var interestsDto = new List<TechInterestsDto>
        {
            new(1, 1, "AI", "Machine Learning")
        };

        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id)).ReturnsAsync(user);

        _context.UserInterests.Add(new UserInterests { UserId = user.Id, TechInterestId = 1 });
        await _context.SaveChangesAsync();

        // Act
        var result = await _userService.UpdateUserInterests(new UpdateTechInterestsDto(id,interestsDto));

        // Assert
        Assert.True(result.Flag);
        Assert.Equal("Interests updated successfully", result.Message);
        Assert.Empty(_context.UserInterests.Where(x => x.UserId == user.Id && x.TechInterestId != 1));
    }
    [Fact]
    public async Task UpdateUserInterests_UserNotFound_ReturnsFailure()
    {
        // Arrange
        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser)null!);

        // Act
        var result = await _userService.UpdateUserInterests(new UpdateTechInterestsDto("", new List<TechInterestsDto>()));

        // Assert
        Assert.False(result.Flag);
        Assert.Equal("user not found", result.Message);
    }
    [Fact]
    public async Task UpdateUserInterests_EmptyUsername_ReturnsFailure()
    {
        // Act
        var result = await _userService.UpdateUserInterests(new UpdateTechInterestsDto("",new List<TechInterestsDto>()));

        // Assert
        Assert.False(result.Flag);
        Assert.Equal("user not found", result.Message);
    }

    [Fact]
    public async Task UpdateUserInterests_ValidUserEmptyInterests_ClearsInterests()
    {
        // Arrange
        var username = "test";
        var user = new ApplicationUser { Id = "user1", UserName = username };
        _userManagerMock.Setup(x => x.FindByIdAsync(username)).ReturnsAsync(user);

        _context.UserInterests.Add(new UserInterests { UserId = user.Id, TechInterestId = 1 });
        _context.Interests.Add(new Interest() { Id = 1, Name = "interest" });
        _context.TechInterests.Add(new TechInterests() { Id = 1, InterestId = 1, Name = "Name" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _userService.UpdateUserInterests(new UpdateTechInterestsDto(username,new List<TechInterestsDto>()));

        // Assert
        Assert.True(result.Flag);
        Assert.Empty(_context.UserInterests.Where(x => x.UserId == user.Id).ToList());
    }
    [Fact]
    public async Task GetAllInterests_ReturnsCachedList_WhenCacheExists()
    {
        // Arrange
        var expected = new List<TechInterestsDto> { new(1, 1, "AI", "Machine Learning") };
        object cachedValue = expected;

        _cacheMock.Setup(x => x.TryGetValue(Consts.CacheKeys.AllInterests, out cachedValue!)).Returns(true);

        // Act
        var result = await _userService.GetAllInterests();

        // Assert
        Assert.Equal(expected.Count, result.Count);
        Assert.Equal(expected[0].TechName, result[0].TechName);
        _cacheMock.Verify(x => x.TryGetValue(Consts.CacheKeys.AllInterests, out cachedValue!), Times.Once);
    }
}
using Coink.Application;
using Coink.Application.Common;
using Coink.Application.Users;
using Microsoft.Extensions.DependencyInjection;

namespace Coink.UnitTests.Users;

public sealed class UserServiceTests
{
    private static readonly DateTime StoredAt = new(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);

    [Fact]
    public async Task CreateAsyncInvalidInputReturnsFieldErrorsWithoutCallingRepository()
    {
        var repository = new StubUserRepository();
        IUserService service = CreateService(repository);

        ApplicationResult<UserReference> result = await service.CreateAsync(
            new UserInput(" Name ", "12ab", 0, 0, 0, ""),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorType.Validation, result.Error?.Type);
        Assert.Equal(
            ["address", "countryId", "departmentId", "municipalityId", "name", "phone"],
            result.Error?.ValidationErrors?.Keys.Order(StringComparer.Ordinal));
        Assert.Equal(0, repository.CreateCalls);
    }

    [Theory]
    [InlineData("+573001234567")]
    [InlineData("3001234")]
    [InlineData("123456789012345")]
    public async Task CreateAsyncValidPhoneBoundariesReturnsStoredUser(string phone)
    {
        var repository = new StubUserRepository
        {
            CreateResult = new UserPersistenceResult(
                UserPersistenceStatus.Success,
                StoredUser(phone: phone)),
        };
        IUserService service = CreateService(repository);

        ApplicationResult<UserReference> result = await service.CreateAsync(
            ValidInput(phone),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(phone, result.Value?.Phone);
        Assert.Equal(1, repository.CreateCalls);
    }

    [Fact]
    public async Task CreateAsyncInvalidHierarchyReturnsSafeApplicationError()
    {
        var repository = new StubUserRepository
        {
            CreateResult = new UserPersistenceResult(UserPersistenceStatus.InvalidGeography),
        };
        IUserService service = CreateService(repository);

        ApplicationResult<UserReference> result = await service.CreateAsync(
            ValidInput(),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorType.InvalidGeography, result.Error?.Type);
        Assert.Equal("geography.invalid_hierarchy", result.Error?.Code);
    }

    [Fact]
    public async Task ListAsyncInvalidPaginationDoesNotCallRepository()
    {
        var repository = new StubUserRepository();
        IUserService service = CreateService(repository);

        ApplicationResult<UserPage> result = await service.ListAsync(
            new UserListQuery(0, 101, new string('a', 101)),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(["page", "pageSize", "search"], result.Error?.ValidationErrors?.Keys);
        Assert.Equal(0, repository.ListCalls);
    }

    [Fact]
    public async Task MissingTargetsMapToNotFoundForReadUpdateAndDelete()
    {
        var repository = new StubUserRepository
        {
            UpdateResult = new UserPersistenceResult(UserPersistenceStatus.NotFound),
        };
        IUserService service = CreateService(repository);

        ApplicationResult<UserReference> read = await service.GetAsync(42, CancellationToken.None);
        ApplicationResult<UserReference> update = await service.UpdateAsync(
            42,
            ValidInput(),
            CancellationToken.None);
        ApplicationResult<long> delete = await service.DeleteAsync(42, CancellationToken.None);

        Assert.All(
            [read.Error, update.Error, delete.Error],
            error =>
            {
                Assert.Equal(ApplicationErrorType.NotFound, error?.Type);
                Assert.Equal("users.not_found", error?.Code);
            });
    }

    private static IUserService CreateService(IUserRepository repository)
    {
        var services = new ServiceCollection();
        _ = services.AddSingleton(repository);
        _ = services.AddApplication();
        ServiceProvider provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IUserService>();
    }

    private static UserInput ValidInput(string phone = "+573001234567")
    {
        return new UserInput("Ada Lovelace", phone, 1, 5, 5001, "Calle 1 # 2-3");
    }

    private static UserReference StoredUser(string phone = "+573001234567")
    {
        return new UserReference(
            1,
            "Ada Lovelace",
            phone,
            1,
            5,
            5001,
            "Calle 1 # 2-3",
            StoredAt,
            StoredAt);
    }

    private sealed class StubUserRepository : IUserRepository
    {
        public int CreateCalls { get; private set; }

        public int ListCalls { get; private set; }

        public UserPersistenceResult CreateResult { get; init; } =
            new(UserPersistenceStatus.Success, StoredUser());

        public UserPersistenceResult UpdateResult { get; init; } =
            new(UserPersistenceStatus.Success, StoredUser());

        public Task<UserPersistenceResult> CreateAsync(
            UserInput input,
            CancellationToken cancellationToken)
        {
            CreateCalls++;
            return Task.FromResult(CreateResult);
        }

        public Task<UserReference?> GetAsync(long userId, CancellationToken cancellationToken)
        {
            return Task.FromResult<UserReference?>(null);
        }

        public Task<UserPage> ListAsync(
            UserListQuery query,
            CancellationToken cancellationToken)
        {
            ListCalls++;
            return Task.FromResult(new UserPage([], query.Page, query.PageSize, 0));
        }

        public Task<UserPersistenceResult> UpdateAsync(
            long userId,
            UserInput input,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(UpdateResult);
        }

        public Task<long?> DeleteAsync(long userId, CancellationToken cancellationToken)
        {
            return Task.FromResult<long?>(null);
        }
    }
}

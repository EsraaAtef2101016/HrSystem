using Xunit;
using FluentAssertions;
using HrSystem.Domain.Entities;
using HrSystem.Domain.Enums;
using HrSystem.Infrastructure.Service;
using HrSystem.Infrastructure.IRepository;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;

namespace HrSystem.UnitTests.Service
{
    public class EmployeeParticipationServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IEmployeeParticipationRepository> _participationRepoMock;
    private readonly Mock<IGlobalPolicyRepository> _policyRepoMock;
    private readonly Mock<ILeaveRequestRepository> _leaveRequestRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;          // <-- add this
    private readonly EmployeeParticipationService _service;

    private readonly Guid _testUserId = Guid.NewGuid();

    public EmployeeParticipationServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _participationRepoMock = new Mock<IEmployeeParticipationRepository>();
        _policyRepoMock = new Mock<IGlobalPolicyRepository>();
        _leaveRequestRepoMock = new Mock<ILeaveRequestRepository>();
        _userRepoMock = new Mock<IUserRepository>();                // <-- add this

        _unitOfWorkMock.Setup(u => u.EmployeeParticipations).Returns(_participationRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.GlobalPolicies).Returns(_policyRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.LeaveRequests).Returns(_leaveRequestRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepoMock.Object);   // <-- add this

        _service = new EmployeeParticipationService(_unitOfWorkMock.Object, _httpContextAccessorMock.Object);

        SetupUserContext(_testUserId);
        SetupDefaultPolicy();
        SetupTestUser();   // <-- add this
    }

    private void SetupTestUser()
    {
        var user = User.Create("Test User", "test@example.com", UserRole.Employee);
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(user, _testUserId);
        // or better: give User a way to set Id in tests / use reflection as above
        _userRepoMock.Setup(r => r.GetByIdAsync(_testUserId)).ReturnsAsync(user);
    }
        private void SetupUserContext(Guid userId)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext { User = claimsPrincipal };
            _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(httpContext);
        }

        private void SetupDefaultPolicy(bool isSelfOptOutAllowed = true, int cooldownDays = 0)
        {
            var policy = new GlobalPolicy(isSelfOptOutAllowed, cooldownDays);
            _policyRepoMock.Setup(p => p.GetAllAsync()).ReturnsAsync(new List<GlobalPolicy> { policy });
        }

        [Fact]
        public async Task OptOutAsync_SuccessfulTransition_WhenEligible()
        {
            // Arrange
            var participation = new EmployeeParticipation(_testUserId, defaultOptIn: true);
            _participationRepoMock.Setup(p => p.GetByEmployeeIdAsync(_testUserId)).ReturnsAsync(participation);
            _leaveRequestRepoMock.Setup(l => l.GetAllAsync()).ReturnsAsync(new List<LeaveRequest>());

            // Act
            var result = await _service.OptOutAsync();

            // Assert
            result.IsSuccess.Should().BeTrue();
            participation.IsOptedIn.Should().BeFalse();
            participation.LastOptOutDate.Should().NotBeNull();
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task OptInAsync_SuccessfulTransition_WhenOptedOut()
        {
        // Arrange
        var participation = new EmployeeParticipation(_testUserId, defaultOptIn: false);
        participation.OptOut(0); // cooldown already ended
        _participationRepoMock.Setup(p => p.GetByEmployeeIdAsync(_testUserId)).ReturnsAsync(participation);
        _leaveRequestRepoMock.Setup(l => l.GetAllAsync()).ReturnsAsync(new List<LeaveRequest>());

        // Act
        var result = await _service.OptInAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        participation.IsOptedIn.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task OptOutAsync_Blocked_WhenSelfOptOutDisabledByPolicy()
        {
            // Arrange
            SetupDefaultPolicy(isSelfOptOutAllowed: false);
            var participation = new EmployeeParticipation(_testUserId, defaultOptIn: true);
            _participationRepoMock.Setup(p => p.GetByEmployeeIdAsync(_testUserId)).ReturnsAsync(participation);

            // Act
            var result = await _service.OptOutAsync();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.First().Message.Should().Contain("Self opt-out is disabled");
            participation.IsOptedIn.Should().BeTrue();
        }

        [Fact]
        public async Task OptOutAsync_Blocked_WhenAlreadyOptedOut()
        {
            // Arrange
            var participation = new EmployeeParticipation(_testUserId, defaultOptIn: false);
            _participationRepoMock.Setup(p => p.GetByEmployeeIdAsync(_testUserId)).ReturnsAsync(participation);

            // Act
            var result = await _service.OptOutAsync();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.First().Message.Should().Contain("already opted out");
        }

        [Fact]
        public async Task OptOutAsync_Blocked_WhenActivePendingLeaveRequestsExist()
        {
            // Arrange
            var participation = new EmployeeParticipation(_testUserId, defaultOptIn: true);
            _participationRepoMock.Setup(p => p.GetByEmployeeIdAsync(_testUserId)).ReturnsAsync(participation);

            var pendingLeave = new LeaveRequest
            {
                Id = Guid.NewGuid(),
                EmployeeId = _testUserId,
                Status = LeaveStatus.Pending,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5))
            };
            _leaveRequestRepoMock.Setup(l => l.GetAllAsync()).ReturnsAsync(new List<LeaveRequest> { pendingLeave });

            // Act
            var result = await _service.OptOutAsync();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.First().Message.Should().Contain("pending requests");
            participation.IsOptedIn.Should().BeTrue();
        }

        [Fact]
        public async Task OptOutAsync_Blocked_WhenFutureApprovedLeaveRequestsExist()
        {
            // Arrange
            var participation = new EmployeeParticipation(_testUserId, defaultOptIn: true);
            _participationRepoMock.Setup(p => p.GetByEmployeeIdAsync(_testUserId)).ReturnsAsync(participation);

            var approvedFutureLeave = new LeaveRequest
            {
                Id = Guid.NewGuid(),
                EmployeeId = _testUserId,
                Status = LeaveStatus.Approved,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3))
            };
            _leaveRequestRepoMock.Setup(l => l.GetAllAsync()).ReturnsAsync(new List<LeaveRequest> { approvedFutureLeave });

            // Act
            var result = await _service.OptOutAsync();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.First().Message.Should().Contain("approved requests with a future start date");
            participation.IsOptedIn.Should().BeTrue();
        }
    }
}
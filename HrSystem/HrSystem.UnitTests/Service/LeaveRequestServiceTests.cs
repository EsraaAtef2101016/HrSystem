using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using FluentValidation;
using HrSystem.Infrastructure.Service;
using HrSystem.Domain.Entities;
using HrSystem.Domain.Enums;
using HrSystem.Infrastructure.IRepository;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using HrSystem.Application.Features.LeaveRequestFeature.Validator;

using HrSystem.Application.Features.LeaveRequestFeature.DTO.RequestDto;
namespace HrSystem.UnitTests.Service
{
    public class LeaveRequestServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IValidator<CreateLeaveRequestRequest>> _createValidatorMock;
        private readonly Mock<IValidator<UpdateLeaveRequestRequest>> _updateValidatorMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<ILeaveRequestRepository> _leaveRequestRepoMock;
        private readonly Mock<ILeaveBalanceRepository> _leaveBalanceRepoMock;
        private readonly Mock<ILeavePolicyRepository> _leavePolicyRepoMock;
        private readonly Mock<IPublicHolidayRepository> _publicHolidayRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;

        private readonly LeaveRequestService _service;
        private readonly Guid _testUserId;

        public LeaveRequestServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _createValidatorMock = new Mock<IValidator<CreateLeaveRequestRequest>>();
            _updateValidatorMock = new Mock<IValidator<UpdateLeaveRequestRequest>>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            _leaveRequestRepoMock = new Mock<ILeaveRequestRepository>();
            _leaveBalanceRepoMock = new Mock<ILeaveBalanceRepository>();
            _leavePolicyRepoMock = new Mock<ILeavePolicyRepository>();
            _publicHolidayRepoMock = new Mock<IPublicHolidayRepository>();
            _userRepoMock = new Mock<IUserRepository>();

            _unitOfWorkMock.Setup(u => u.LeaveRequests).Returns(_leaveRequestRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.LeaveBalances).Returns(_leaveBalanceRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.LeavePolicies).Returns(_leavePolicyRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.PublicHolidays).Returns(_publicHolidayRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepoMock.Object);

            var testUser = CreateTestUser(UserRole.Employee);
            _testUserId = testUser.Id;

            SetupUserContext(testUser);

            _service = new LeaveRequestService(
                _unitOfWorkMock.Object,
                _createValidatorMock.Object,
                _updateValidatorMock.Object,
                _httpContextAccessorMock.Object
            );
        }

        private static User CreateTestUser(UserRole role, bool isActive = true)
        {
            var user = User.Create("Test User", "test.user@example.com", role);
            user.IsActive = isActive;
            return user;
        }

        private void SetupUserContext(User user)
        {
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext { User = claimsPrincipal };
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            _userRepoMock.Setup(u => u.GetByIdAsync(user.Id)).ReturnsAsync(user);
        }

        private void SetupDefaultMocks(LeavePolicy policy, LeaveBalance balance, List<PublicHoliday> holidays = null, List<LeaveRequest> existingRequests = null)
        {
            _leavePolicyRepoMock.Setup(p => p.GetActiveByTypeAsync(It.IsAny<LeaveType>())).ReturnsAsync(policy);
            _publicHolidayRepoMock.Setup(p => p.GetFutureHolidaysAsync(It.IsAny<DateOnly>())).ReturnsAsync(holidays ?? new List<PublicHoliday>());
            _leaveBalanceRepoMock.Setup(b => b.GetByEmployeeAndTypeAndYearAsync(_testUserId, It.IsAny<LeaveType>(), It.IsAny<int>())).ReturnsAsync(balance);
            _leaveRequestRepoMock.Setup(r => r.GetByEmployeeIdAsync(_testUserId)).ReturnsAsync(existingRequests ?? new List<LeaveRequest>());

            // Supporting both direct validation signatures for safety
            _createValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateLeaveRequestRequest>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            _createValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<CreateLeaveRequestRequest>>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        }

        [Fact]
        public async Task CalculateChargedBusinessDays_ExcludesWeekendsAndPublicHolidays()
        {
            var startDate = new DateOnly(2026, 11, 1);
            var endDate = new DateOnly(2026, 11, 7);
            var holidays = new List<PublicHoliday> { PublicHoliday.Create(new DateOnly(2026, 11, 3), "Test Holiday") };
            var policy = new LeavePolicy { Id = Guid.NewGuid(), AnnualAllowance = 10, MaxConsecutiveDays = 14, Version = 1 };
             var balance = new LeaveBalance(
                employeeId: Guid.NewGuid(),
                leaveType: LeaveType.Vacation,
                year: 2026,
                initialAllowance: 15
            );
          //   var balance = new LeaveBalance { InitialAllowance = 15, UsedDays = 0, ReservedDays = 0, Year = 2026 };

            SetupDefaultMocks(policy, balance, holidays);

            var createDto = new CreateLeaveRequestRequest
            {
                LeaveType = LeaveType.Vacation,
                StartDate = startDate,
                EndDate = endDate
            };

            var result = await _service.CreateAsync(createDto);

            result.IsSuccess.Should().BeTrue();
            result.Value.ChargedDays.Should().Be(4);
        }

        [Fact]
        public async Task CreateAsync_FailsWhenValidationFails()
        {
            var policy = new LeavePolicy { Id = Guid.NewGuid(), AnnualAllowance = 10, MaxConsecutiveDays = 14, Version = 1 };
             var balance = new LeaveBalance(
                employeeId: Guid.NewGuid(),
                leaveType: LeaveType.Vacation,
                year: 2026,
                initialAllowance: 15
            );
            SetupDefaultMocks(policy, balance);

            var validationFailures = new List<FluentValidation.Results.ValidationFailure>
            {
                new("StartDate", "Start date is required.")
            };
            _createValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateLeaveRequestRequest>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult(validationFailures));
            _createValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<CreateLeaveRequestRequest>>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult(validationFailures));

            var createDto = new CreateLeaveRequestRequest
            {
                LeaveType = LeaveType.Vacation,
                StartDate = default,
                EndDate = default
            };

            var result = await _service.CreateAsync(createDto);

            result.IsFailed.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Message.Contains("Start date is required."));
        }

        [Fact]
        public async Task CreateAsync_FailsWhenRequestOverlapsExistingRequest()
        {
            var startDate = new DateOnly(2026, 11, 10);
            var endDate = new DateOnly(2026, 11, 12);
            var existingRequests = new List<LeaveRequest>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = _testUserId,
                    StartDate = new DateOnly(2026, 11, 11),
                    EndDate = new DateOnly(2026, 11, 15),
                    Status = LeaveStatus.Approved,
                    LeaveType = LeaveType.Vacation
                }
            };
            var policy = new LeavePolicy { Id = Guid.NewGuid(), AnnualAllowance = 10, MaxConsecutiveDays = 14, Version = 1 };
            var balance = new LeaveBalance(
                employeeId: Guid.NewGuid(),
                leaveType: LeaveType.Vacation,
                year: 2026,
                initialAllowance: 10
            );
            SetupDefaultMocks(policy, balance, existingRequests: existingRequests);

            var createDto = new CreateLeaveRequestRequest
            {
                LeaveType = LeaveType.Vacation,
                StartDate = startDate,
                EndDate = endDate
            };

            var result = await _service.CreateAsync(createDto);

            result.IsFailed.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Message.Contains("overlapping"));
        }

       [Fact]
        public async Task CreateAsync_ReservesBalanceSuccessfully()
        {
            var startDate = new DateOnly(2026, 9, 7); // Monday
            var endDate = new DateOnly(2026, 9, 8);   // Tuesday (2 business days)

            var testUser = CreateTestUser(UserRole.Employee);
            var employeeId = testUser.Id;

            var policy = new LeavePolicy { Id = Guid.NewGuid(), AnnualAllowance = 10, MaxConsecutiveDays = 14, Version = 1, MinNoticeDays = 0 };

            var balance = new LeaveBalance(
                employeeId: employeeId,
                leaveType: LeaveType.Vacation,
                year: 2026,
                initialAllowance: 10
            );

            SetupDefaultMocks(policy, balance); // <-- fixed

            _unitOfWorkMock.Setup(u => u.PublicHolidays.GetFutureHolidaysAsync(It.IsAny<DateOnly>()))
                .ReturnsAsync(new List<PublicHoliday>());

            var createDto = new CreateLeaveRequestRequest
            {
                LeaveType = LeaveType.Vacation,
                StartDate = startDate,
                EndDate = endDate
            };

            var result = await _service.CreateAsync(createDto);

            result.IsSuccess.Should().BeTrue();
            balance.ReservedDays.Should().Be(2);
            _leaveBalanceRepoMock.Verify(b => b.Update(balance), Times.Once);
        }

       [Fact]
        public async Task CancelAsync_ReleasesReservedAndUsedBalance()
        {
            var requestId = Guid.NewGuid();
            var leaveRequest = new LeaveRequest
            {
                Id = requestId,
                EmployeeId = _testUserId,
                LeaveType = LeaveType.Vacation,
                StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
                EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(6)),
                ChargedDays = 2,
                Status = LeaveStatus.Pending
            };

            _leaveRequestRepoMock.Setup(r => r.GetByIdAsync(requestId)).ReturnsAsync(leaveRequest);

            var balance = new LeaveBalance(
                employeeId: Guid.NewGuid(),
                leaveType: LeaveType.Vacation,
                year: 2026,
                initialAllowance: 10
            );
            balance.ReserveDays(2); // Pending request only ever reserves, never "uses"

            _leaveBalanceRepoMock.Setup(b => b.GetByEmployeeAndTypeAndYearAsync(_testUserId, LeaveType.Vacation, leaveRequest.StartDate.Year))
                .ReturnsAsync(balance);

            var result = await _service.CancelAsync(requestId);

            result.IsSuccess.Should().BeTrue();
            balance.ReservedDays.Should().Be(0);
            balance.UsedDays.Should().Be(0); // never set, so stays 0
            leaveRequest.Status.Should().Be(LeaveStatus.Cancelled);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_FailsWhenExceedsMaxConsecutiveDaysWithMergedRequests()
        {
            var startDate = new DateOnly(2026, 11, 3);
            var endDate = new DateOnly(2026, 11, 4);
            var policy = new LeavePolicy { Id = Guid.NewGuid(), AnnualAllowance = 20, MaxConsecutiveDays = 3, Version = 1 };
             var balance = new LeaveBalance(
                employeeId: Guid.NewGuid(),
                leaveType: LeaveType.Vacation,
                year: 2026,
                initialAllowance: 10
            );
            var existingRequests = new List<LeaveRequest>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = _testUserId,
                    StartDate = new DateOnly(2026, 11, 1),
                    EndDate = new DateOnly(2026, 11, 2),
                    Status = LeaveStatus.Approved,
                    LeaveType = LeaveType.Vacation
                }
            };

            SetupDefaultMocks(policy, balance, existingRequests: existingRequests);

            var createDto = new CreateLeaveRequestRequest
            {
                LeaveType = LeaveType.Vacation,
                StartDate = startDate,
                EndDate = endDate
            };

            var result = await _service.CreateAsync(createDto);

            result.IsFailed.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Message.Contains("consecutive leave days"));
        }

        [Fact]
        public async Task CancelAsync_ReleasesUsedBalance_WhenApprovedAndFuture()
        {
            var requestId = Guid.NewGuid();
            var leaveRequest = new LeaveRequest
            {
                Id = requestId,
                EmployeeId = _testUserId,
                LeaveType = LeaveType.Vacation,
                StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
                EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(6)),
                ChargedDays = 2,
                Status = LeaveStatus.Approved
            };

            _leaveRequestRepoMock.Setup(r => r.GetByIdAsync(requestId)).ReturnsAsync(leaveRequest);

            var balance = new LeaveBalance(
                employeeId: Guid.NewGuid(),
                leaveType: LeaveType.Vacation,
                year: 2026,
                initialAllowance: 10
            );
            balance.UseDays(2); // Approved request has consumed days

            _leaveBalanceRepoMock.Setup(b => b.GetByEmployeeAndTypeAndYearAsync(_testUserId, LeaveType.Vacation, leaveRequest.StartDate.Year))
                .ReturnsAsync(balance);

            var result = await _service.CancelAsync(requestId);

            result.IsSuccess.Should().BeTrue();
            balance.UsedDays.Should().Be(0);
            leaveRequest.Status.Should().Be(LeaveStatus.Cancelled);
        }
        [Fact]
        public async Task CreateAsync_FailsWhenMinNoticeDaysViolationOccurs()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var startDate = today.AddDays(1); // Only 1 day notice
            var endDate = today.AddDays(2);
            // Policy requires at least 5 days notice
            var policy = new LeavePolicy
            {
                Id = Guid.NewGuid(),
                LeaveType = LeaveType.Vacation,
                AnnualAllowance = 20,
                MaxConsecutiveDays = 14,
                MinNoticeDays = 5,
                Version = 1
            };
             var balance = new LeaveBalance(
                employeeId: Guid.NewGuid(),
                leaveType: LeaveType.Vacation,
                year: 2026,
                initialAllowance: 10
            );
            SetupDefaultMocks(policy, balance);

            var createDto = new CreateLeaveRequestRequest
            {
                LeaveType = LeaveType.Vacation,
                StartDate = startDate,
                EndDate = endDate
            };

            var result = await _service.CreateAsync(createDto);

            result.IsFailed.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Message.Contains("notice") || e.Message.Contains("MinNoticeDays"));
        }

        [Fact]
        public async Task CreateAsync_FailsWhenDayOffExceedsSingleBusinessDayLimit()
        {
            var startDate = new DateOnly(2026, 11, 2); // Monday
            var endDate = new DateOnly(2026, 11, 3);   // Tuesday (2 days instead of 1)
            var policy = new LeavePolicy
            {
                Id = Guid.NewGuid(),
                LeaveType = LeaveType.DayOff,
                AnnualAllowance = 5,
                MaxConsecutiveDays = 1,
                Version = 1
            };
             var balance = new LeaveBalance(
                employeeId: Guid.NewGuid(),
                leaveType: LeaveType.Vacation,
                year: 2026,
                initialAllowance: 5
            );
            SetupDefaultMocks(policy, balance);

            var createDto = new CreateLeaveRequestRequest
            {
                LeaveType = LeaveType.DayOff,
                StartDate = startDate,
                EndDate = endDate
            };

            var result = await _service.CreateAsync(createDto);

            result.IsFailed.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Message.Contains("DayOff") || e.Message.Contains("one business day") || e.Message.Contains("consecutive"));
        }



      [Fact]
        public async Task CreateAsync_SickLeaveAllowsBackdatingWithinPolicyLimit()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var startDate = GetPreviousBusinessDay(today, 2);
            var endDate = GetPreviousBusinessDay(today, 1);

            var policy = new LeavePolicy { Id = Guid.NewGuid(), AnnualAllowance = 10, MaxConsecutiveDays = 5, BackdateDays = 5, Version = 1 };
            var balance = new LeaveBalance(
                employeeId: Guid.NewGuid(),
                leaveType: LeaveType.Vacation,
                year: 2026,
                initialAllowance: 10
            );
            SetupDefaultMocks(policy, balance);

            var createDto = new CreateLeaveRequestRequest
            {
                LeaveType = LeaveType.SickLeave,
                StartDate = startDate,
                EndDate = endDate
            };

            var result = await _service.CreateAsync(createDto);

            result.IsSuccess.Should().BeTrue();
        }

        private static DateOnly GetPreviousBusinessDay(DateOnly today, int daysBack)
        {
            var date = today.AddDays(-daysBack);
            while (date.DayOfWeek == DayOfWeek.Friday || date.DayOfWeek == DayOfWeek.Saturday)
            {
                date = date.AddDays(-1);
            }
            return date;
        }
        [Fact]
        public async Task CreateAsync_SickLeaveFailsWhenBackdatingExceedsPolicyLimit()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var startDate = today.AddDays(-10);
            var endDate = today.AddDays(-8);
            var policy = new LeavePolicy { Id = Guid.NewGuid(), AnnualAllowance = 10, MaxConsecutiveDays = 5, BackdateDays = 3, Version = 1 };
             var balance = new LeaveBalance(
                employeeId: Guid.NewGuid(),
                leaveType: LeaveType.Vacation,
                year: 2026,
                initialAllowance: 10
            );
            SetupDefaultMocks(policy, balance);

            var createDto = new CreateLeaveRequestRequest
            {
                LeaveType = LeaveType.SickLeave,
                StartDate = startDate,
                EndDate = endDate
            };

            var result = await _service.CreateAsync(createDto);

            result.IsFailed.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Message.Contains("backdated more than"));
        }

        [Fact]
        public async Task CancelAsync_FailsOnStartedOrHistoricalApprovedRequest()
        {
            var requestId = Guid.NewGuid();
            var today = DateOnly.FromDateTime(DateTime.Today);
            var leaveRequest = new LeaveRequest
            {
                Id = requestId,
                EmployeeId = _testUserId,
                LeaveType = LeaveType.Vacation,
                StartDate = today.AddDays(-1),
                EndDate = today.AddDays(2),
                ChargedDays = 3,
                Status = LeaveStatus.Approved
            };

            _leaveRequestRepoMock.Setup(r => r.GetByIdAsync(requestId)).ReturnsAsync(leaveRequest);

            var result = await _service.CancelAsync(requestId);

            result.IsFailed.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Message.Contains("historical approved request cannot be cancelled"));
        }

        [Fact]
        public async Task UpdateAsync_FailsWhenRequestIsNotPending()
        {
            var requestId = Guid.NewGuid();
            var leaveRequest = new LeaveRequest
            {
                Id = requestId,
                EmployeeId = _testUserId,
                LeaveType = LeaveType.Vacation,
                Status = LeaveStatus.Approved
            };

            _leaveRequestRepoMock.Setup(r => r.GetByIdAsync(requestId)).ReturnsAsync(leaveRequest);

            _updateValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateLeaveRequestRequest>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            _updateValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<UpdateLeaveRequestRequest>>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            var updateDto = new UpdateLeaveRequestRequest
            {
                LeaveType = LeaveType.Vacation,
                StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(10)),
                EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(11))
            };

            var result = await _service.UpdateAsync(requestId, updateDto);

            result.IsFailed.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Message.Contains("Only pending leave requests can be updated"));
        }

     [Fact]
        public async Task CreateAsync_SnapshotsPolicyVersionAndAllowance()
        {
            // Pick a business day at least 1 day out (MinNoticeDays=1) that isn't
            // Friday/Saturday (this app's weekend), so the test isn't flaky
            // depending on which day it happens to run.
            var startDate = GetNextBusinessDay(DateOnly.FromDateTime(DateTime.Today), 5);
            var endDate = startDate;

            var policy = new LeavePolicy { Id = Guid.NewGuid(), AnnualAllowance = 21, MaxConsecutiveDays = 5, MinNoticeDays = 1, Version = 3 };
            var balance = new LeaveBalance(
                employeeId: Guid.NewGuid(),
                leaveType: LeaveType.Vacation,
                year: 2026,
                initialAllowance: 21
            );
            SetupDefaultMocks(policy, balance);

            var createDto = new CreateLeaveRequestRequest
            {
                LeaveType = LeaveType.Vacation,
                StartDate = startDate,
                EndDate = endDate
            };

            LeaveRequest capturedRequest = null;
            _leaveRequestRepoMock.Setup(r => r.AddAsync(It.IsAny<LeaveRequest>()))
                .Callback<LeaveRequest>(req => capturedRequest = req)
                .Returns(Task.CompletedTask);

            var result = await _service.CreateAsync(createDto);

            result.IsSuccess.Should().BeTrue();
            capturedRequest.Should().NotBeNull();
            capturedRequest.PolicyVersionSnapshot.Should().Be(3);
            capturedRequest.PolicyAllowanceSnapshot.Should().Be(21);
            capturedRequest.LeavePolicyId.Should().Be(policy.Id);
        }

        // Walks forward from "today" by roughly `daysForward` calendar days, then
        // nudges later if it lands on a Friday/Saturday, so the resulting date is
        // always a chargeable business day per this app's weekend definition.
        private static DateOnly GetNextBusinessDay(DateOnly today, int daysForward)
        {
            var date = today.AddDays(daysForward);
            while (date.DayOfWeek == DayOfWeek.Friday || date.DayOfWeek == DayOfWeek.Saturday)
            {
                date = date.AddDays(1);
            }
            return date;
        }
    }
}
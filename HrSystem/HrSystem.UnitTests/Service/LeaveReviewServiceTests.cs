using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using HrSystem.Application.Common.DTO.ErrorDto;
using HrSystem.Infrastructure.Service;
using HrSystem.Domain.Entities;
using HrSystem.Domain.Enums;
using HrSystem.Infrastructure.IRepository;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace HrSystem.UnitTests.Service
{
    public class LeaveReviewServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<ILeaveRequestRepository> _leaveRequestRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<ILeaveBalanceRepository> _leaveBalanceRepoMock;
        private readonly LeaveReviewService _service;
        private readonly Guid _managerId;

        public LeaveReviewServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _leaveRequestRepoMock = new Mock<ILeaveRequestRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _leaveBalanceRepoMock = new Mock<ILeaveBalanceRepository>();

            _unitOfWorkMock.Setup(u => u.LeaveRequests).Returns(_leaveRequestRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.LeaveBalances).Returns(_leaveBalanceRepoMock.Object);

            _managerId = Guid.NewGuid();
            var manager = User.Create("Manager User", "manager@example.com", UserRole.Manager);
            typeof(User).GetProperty(nameof(User.Id))?.SetValue(manager, _managerId);

            SetupUserContext(manager);

            _service = new LeaveReviewService(_unitOfWorkMock.Object, _httpContextAccessorMock.Object);
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

        [Fact]
        public async Task AcceptAsync_ShouldApproveRequest_WhenValidManagerAndPendingStatus()
        {
            var employeeId = Guid.NewGuid();
            var leaveRequestId = Guid.NewGuid();

            var employee = User.Create("Employee User", "employee@example.com", UserRole.Employee, _managerId);
            typeof(User).GetProperty(nameof(User.Id))?.SetValue(employee, employeeId);

            var leaveRequest = new LeaveRequest
            {
                Id = leaveRequestId,
                EmployeeId = employeeId,
                Employee = employee,
                Status = LeaveStatus.Pending,
                StartDate = new DateOnly(2026, 11, 1),
                EndDate = new DateOnly(2026, 11, 3),
                ChargedDays = 2,
                LeaveType = LeaveType.Vacation
            };

            _leaveRequestRepoMock.Setup(r => r.GetByIdAsync(leaveRequestId)).ReturnsAsync(leaveRequest);
            _leaveBalanceRepoMock.Setup(b => b.GetAllAsync()).ReturnsAsync(new List<LeaveBalance>
            {
                new() { EmployeeId = employeeId, Year = 2026, ReservedDays = 2, UsedDays = 0, LeaveType = LeaveType.Vacation }
            });

            var result = await _service.AcceptAsync(leaveRequestId);

            result.IsSuccess.Should().BeTrue();
            result.Value.Status.Should().Be("Approved");
            leaveRequest.Status.Should().Be(LeaveStatus.Approved);
            _leaveRequestRepoMock.Verify(r => r.Update(leaveRequest), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AcceptAsync_Fails_WhenRequestIsNotPending()
        {
            var leaveRequestId = Guid.NewGuid();
            var leaveRequest = new LeaveRequest
            {
                Id = leaveRequestId,
                Status = LeaveStatus.Approved
            };

            _leaveRequestRepoMock.Setup(r => r.GetByIdAsync(leaveRequestId)).ReturnsAsync(leaveRequest);

            var result = await _service.AcceptAsync(leaveRequestId);

            result.IsFailed.Should().BeTrue();
            result.Errors.Should().Contain(e => e is ConflictError);
        }

        [Fact]
        public async Task RejectAsync_ShouldRejectRequestAndReleaseReservedDays_WhenValid()
        {
            var employeeId = Guid.NewGuid();
            var leaveRequestId = Guid.NewGuid();

            var employee = User.Create("Employee User", "employee@example.com", UserRole.Employee, _managerId);
            typeof(User).GetProperty(nameof(User.Id))?.SetValue(employee, employeeId);

            var leaveRequest = new LeaveRequest
            {
                Id = leaveRequestId,
                EmployeeId = employeeId,
                Employee = employee,
                Status = LeaveStatus.Pending,
                StartDate = new DateOnly(2026, 11, 1),
                EndDate = new DateOnly(2026, 11, 3),
                ChargedDays = 2,
                LeaveType = LeaveType.Vacation
            };

            _leaveRequestRepoMock.Setup(r => r.GetByIdAsync(leaveRequestId)).ReturnsAsync(leaveRequest);
            var balance = new LeaveBalance { EmployeeId = employeeId, Year = 2026, ReservedDays = 2, UsedDays = 0, LeaveType = LeaveType.Vacation };
            _leaveBalanceRepoMock.Setup(b => b.GetAllAsync()).ReturnsAsync(new List<LeaveBalance> { balance });

            var result = await _service.RejectAsync(leaveRequestId, "Operational constraints");

            result.IsSuccess.Should().BeTrue();
            result.Value.Status.Should().Be("Rejected");
            leaveRequest.Status.Should().Be(LeaveStatus.Rejected);
            leaveRequest.RejectionReason.Should().Be("Operational constraints");
            balance.ReservedDays.Should().Be(0);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AcceptAsync_Fails_WhenManagerTriesToApproveOwnRequest()
        {
            var leaveRequestId = Guid.NewGuid();
            var managerAsEmployee = User.Create("Manager User", "manager@example.com", UserRole.Manager, null);
            typeof(User).GetProperty(nameof(User.Id))?.SetValue(managerAsEmployee, _managerId);

            var leaveRequest = new LeaveRequest
            {
                Id = leaveRequestId,
                EmployeeId = _managerId,
                Employee = managerAsEmployee,
                Status = LeaveStatus.Pending
            };

            _leaveRequestRepoMock.Setup(r => r.GetByIdAsync(leaveRequestId)).ReturnsAsync(leaveRequest);

            var result = await _service.AcceptAsync(leaveRequestId);

            result.IsFailed.Should().BeTrue();
            result.Errors.Should().Contain(e => e is UnauthorizedError);
        }
    }
}
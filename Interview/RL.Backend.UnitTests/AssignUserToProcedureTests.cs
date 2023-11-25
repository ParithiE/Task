using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using RL.Backend.Commands;
using RL.Backend.Commands.Handlers.Plans;
using RL.Backend.Exceptions;
using RL.Data;
using RL.Data.DataModels;

namespace RL.Backend.UnitTests
{
    [TestClass]
    public class AssignUserToProcedureTests
    {
        [TestMethod]
        [DataRow(-1)]
        [DataRow(0)]
        [DataRow(int.MinValue)]
        public async Task AssignUserToProcedureTests_InvalidPlanId_ReturnsBadRequest(int planId)
        {
            // Given
            var context = new Mock<RLContext>();
            var sut = new AssignUserToProcedureCommandHandler(context.Object);
            var request = new AssignUserToProcedureCommand
            {
                PlanId = planId,
                ProcedureId = 1,
                UserIds = new List<int> { 1, 2, 3 }
            };

            // When
            var result = await sut.Handle(request, new CancellationToken());

            // Then
            result.Exception.Should().BeOfType(typeof(BadRequestException));
            result.Succeeded.Should().BeFalse();
        }

        [TestMethod]
        [DataRow(-1)]
        [DataRow(0)]
        [DataRow(int.MinValue)]
        public async Task AssignUserToProcedureTests_InvalidProcedureId_ReturnsBadRequest(int procedureId)
        {
            //Given
            var context = new Mock<RLContext>();
            var sut = new AssignUserToProcedureCommandHandler(context.Object);
            var request = new AssignUserToProcedureCommand
            {
                PlanId = 1,
                ProcedureId = procedureId
            };
            //When
            var result = await sut.Handle(request, new CancellationToken());

            //Then
            result.Exception.Should().BeOfType(typeof(BadRequestException));
            result.Succeeded.Should().BeFalse();
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(19)]
        [DataRow(35)]
        public async Task AssignUserToProcedureTests_PlanIdNotFound_ReturnsNotFound(int planId)
        {
            //Given
            var context = DbContextHelper.CreateContext();
            var sut = new AssignUserToProcedureCommandHandler(context);
            var request = new AssignUserToProcedureCommand()
            {
                PlanId = planId,
                ProcedureId = 1
            };

            context.Plans.Add(new Data.DataModels.Plan
            {
                PlanId = planId + 1
            });
            await context.SaveChangesAsync();

            //When
            var result = await sut.Handle(request, new CancellationToken());

            //Then
            result.Exception.Should().BeOfType(typeof(NotFoundException));
            result.Succeeded.Should().BeFalse();
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(19)]
        [DataRow(35)]
        public async Task AssignUserToProcedureTests_ProcedureIdNotFound_ReturnsNotFound(int procedureId)
        {
            //Given
            var context = DbContextHelper.CreateContext();
            var sut = new AssignUserToProcedureCommandHandler(context);
            var request = new AssignUserToProcedureCommand()
            {
                PlanId = 1,
                ProcedureId = procedureId
            };

            context.Plans.Add(new Data.DataModels.Plan
            {
                PlanId = procedureId + 1
            });
            context.Procedures.Add(new Data.DataModels.Procedure
            {
                ProcedureId = procedureId + 1,
                ProcedureTitle = "Test Procedure"
            });
            await context.SaveChangesAsync();

            //When
            var result = await sut.Handle(request, new CancellationToken());

            //Then
            result.Exception.Should().BeOfType(typeof(NotFoundException));
            result.Succeeded.Should().BeFalse();
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(19)]
        [DataRow(35)]
        public async Task AssignUserToProcedureTests_UserNotFound_ReturnsNotFound(int userId)
        {
            // Given
            var context = DbContextHelper.CreateContext();
            var sut = new AssignUserToProcedureCommandHandler(context);
            var request = new AssignUserToProcedureCommand
            {
                PlanId = 1,
                ProcedureId = 1,
                UserIds = new List<int> { userId }
            };

            context.Plans.Add(new Data.DataModels.Plan
            {
                PlanId = request.PlanId
            });
            context.Procedures.Add(new Data.DataModels.Procedure
            {
                ProcedureId = request.ProcedureId,
                ProcedureTitle = "Test Procedure"
            });


            await context.SaveChangesAsync();

            // When
            var result = await sut.Handle(request, new CancellationToken());

            // Then
            result.Exception.Should().BeOfType(typeof(NotFoundException));
            result.Succeeded.Should().BeFalse();
        }

        [TestMethod]
        [DataRow(1,2,3)]
        [DataRow(4,5,6)]
        public async Task AssignUserToProcedureTests_Success_ReturnsSuccess(params int[] userIds)
        {
            // Given
            var context = DbContextHelper.CreateContext();
            var sut = new AssignUserToProcedureCommandHandler(context);
            var request = new AssignUserToProcedureCommand
            {
                PlanId = 1,
                ProcedureId = 1,
                UserIds = userIds.ToList()
            };

            context.Plans.Add(new Data.DataModels.Plan
            {
                PlanId = request.PlanId
            });
            context.Procedures.Add(new Data.DataModels.Procedure
            {
                ProcedureId = request.ProcedureId,
                ProcedureTitle = "Test Procedure"
            });

         
            foreach (var userId in request.UserIds)
            {
                context.Users.Add(new Data.DataModels.User
                {
                    UserId = userId,
                });
            }

            await context.SaveChangesAsync();

            // When
            var result = await sut.Handle(request, new CancellationToken());

            result.Value.Should().BeOfType(typeof(Unit));
            result.Succeeded.Should().BeTrue();
        }

        [TestMethod]
        [DataRow(1, 2, 3)]
        [DataRow(4, 5, 6)]
        public async Task AssignUserToProcedureTests_AlreadyAssigned_ReturnsSuccess(params int[] userIds)
        {
            // Given
            var context = DbContextHelper.CreateContext();
            var sut = new AssignUserToProcedureCommandHandler(context);
            var request = new AssignUserToProcedureCommand
            {
                PlanId = 1,
                ProcedureId = 1,
                UserIds = userIds.ToList()
            };

            context.Plans.Add(new Data.DataModels.Plan
            {
                PlanId = request.PlanId
            });
            context.Procedures.Add(new Data.DataModels.Procedure
            {
                ProcedureId = request.ProcedureId,
                ProcedureTitle = "Test Procedure"
            });

            foreach (var userId in request.UserIds)
            {
                context.Users.Add(new Data.DataModels.User
                {
                    UserId = userId,
                });
            }

            // Add existing assignments
            foreach (var userId in userIds)
            {
                context.ProcedureUserAssignments.Add(new Data.DataModels.ProcedureUserAssignment
                {
                    PlanId = request.PlanId,
                    ProcedureId = request.ProcedureId,
                    UserId = userId
                });
            }

            await context.SaveChangesAsync();

            // When
            var result = await sut.Handle(request, new CancellationToken());

            // Then
            var dbProcedureUserAssignments = await context.ProcedureUserAssignments
                .Where(pua => pua.PlanId == request.PlanId && pua.ProcedureId == request.ProcedureId)
                .ToListAsync();

            dbProcedureUserAssignments.Should().HaveCount(userIds.Length); // Ensure existing assignments are not duplicated

            result.Value.Should().BeOfType(typeof(Unit));
            result.Succeeded.Should().BeTrue();
        }

    }
}


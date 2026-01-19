using AutoMapper;
using FluentAssertions;
using Insurance.Api.Mapping;
using Insurance.Application.Common.DTOs.ClientDTOs;
using Insurance.Application.Common.Exceptions;
using Insurance.Application.Services.Implementations;
using Insurance.Domain.Entities;
using Insurance.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Insurance.Tests.Unit.Services;

    [TestFixture]
    public sealed class ClientServiceIdentifierTests
    {
        private static IMapper CreateMapper()
        {
            var cfg = new MapperConfiguration(
                mc => mc.AddProfile(new ApiMappingProfile()),
                NullLoggerFactory.Instance);

            cfg.AssertConfigurationIsValid();
            return cfg.CreateMapper();
        }

        [Test]
        public async Task ChangeIdentifier_ClientMissing_ThrowsNotFound()
        {
            var clients = new Mock<IClientRepository>();
            clients.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), true, It.IsAny<CancellationToken>()))
                   .ReturnsAsync((Client?)null);

            var buildings = new Mock<IBuildingRepository>(MockBehavior.Loose);

            var svc = new ClientService(clients.Object, buildings.Object, CreateMapper(), NullLogger<ClientService>.Instance);

            Func<Task> act = async () => await svc.ChangeIdentifierAsync(Guid.NewGuid(), new ChangeClientIdentifierRequestDTO { NewIdentificationNumber = "NEW" });

            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Client not found.");
        }

        [Test]
        public async Task ChangeIdentifier_Duplicate_ThrowsConflict()
        {
            var clientId = Guid.NewGuid();
            var clients = new Mock<IClientRepository>();
            clients.Setup(x => x.GetByIdAsync(clientId, true, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new Client { Id = clientId, Name = "A", IdentificationNumber = "OLD" });

            clients.Setup(x => x.ExistsByIdentifierAsync("NEW", It.IsAny<CancellationToken>()))
                   .ReturnsAsync(true);

            var buildings = new Mock<IBuildingRepository>(MockBehavior.Loose);
            var svc = new ClientService(clients.Object, buildings.Object, CreateMapper(), NullLogger<ClientService>.Instance);

            Func<Task> act = async () => await svc.ChangeIdentifierAsync(clientId, new ChangeClientIdentifierRequestDTO { NewIdentificationNumber = "NEW" });

            await act.Should().ThrowAsync<ConflictException>();
        }

        [Test]
        public async Task ChangeIdentifier_Valid_AddsHistory_AndUpdatesClient()
        {
            var clientId = Guid.NewGuid();
            var trackedClient = new Client { Id = clientId, Name = "A", IdentificationNumber = "OLD" };

            var clients = new Mock<IClientRepository>();
            clients.Setup(x => x.GetByIdAsync(clientId, true, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(trackedClient);

            clients.Setup(x => x.ExistsByIdentifierAsync("NEW", It.IsAny<CancellationToken>()))
                   .ReturnsAsync(false);

            clients.Setup(x => x.AddIdentifierChangeAsync(It.IsAny<ClientIdentifierChange>(), It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);

            clients.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);

            var buildings = new Mock<IBuildingRepository>(MockBehavior.Loose);

            var svc = new ClientService(clients.Object, buildings.Object, CreateMapper(), NullLogger<ClientService>.Instance);

            await svc.ChangeIdentifierAsync(clientId, new ChangeClientIdentifierRequestDTO { NewIdentificationNumber = "NEW", Reason = "Fix typo" });

            trackedClient.IdentificationNumber.Should().Be("NEW");
            clients.Verify(x => x.AddIdentifierChangeAsync(
                It.Is<ClientIdentifierChange>(c =>
                    c.ClientId == clientId &&
                    c.OldIdentificationNumber == "OLD" &&
                    c.NewIdentificationNumber == "NEW" &&
                    c.Reason == "Fix typo"),
                It.IsAny<CancellationToken>()), Times.Once);

            clients.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }

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
using NUnit.Framework;

namespace Insurance.Tests.Unit.Services;

    [TestFixture]
    public sealed class ClientServiceTests
    {
        private static IMapper CreateMapper()
        {
            var cfg = new MapperConfiguration(c =>
            {
                c.AddProfile(new ApiMappingProfile());
            }, NullLoggerFactory.Instance);

            cfg.AssertConfigurationIsValid();
            return cfg.CreateMapper();
        }

        private static ClientService CreateSut(
            Mock<IClientRepository> clients,
            Mock<IBuildingRepository>? buildings = null,
            IMapper? mapper = null)
        {
            buildings ??= new Mock<IBuildingRepository>(MockBehavior.Loose);
            mapper ??= CreateMapper();

            return new ClientService(
                clients.Object,
                buildings.Object,
                mapper,
                NullLogger<ClientService>.Instance);
        }

        [Test]
        public void CreateAsync_MissingName_ThrowsValidation()
        {
            var clients = new Mock<IClientRepository>(MockBehavior.Strict);
            var buildings = new Mock<IBuildingRepository>(MockBehavior.Strict);

            var svc = new ClientService(clients.Object, buildings.Object, CreateMapper(), NullLogger<ClientService>.Instance);

            Func<Task> act = async () => await svc.CreateAsync(new CreateClientRequestDTO
            {
                Name = "",
                IdentificationNumber = "CNP1"
            });

            act.Should().ThrowAsync<ValidationException>();
        }

        [Test]
        public void CreateAsync_InvalidEmail_ThrowsValidation()
        {
            var clients = new Mock<IClientRepository>(MockBehavior.Loose);
            clients.Setup(x => x.ExistsByIdentifierAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(false);

            var svc = CreateSut(clients);

            Func<Task> act = async () => await svc.CreateAsync(new CreateClientRequestDTO
            {
                Name = "John",
                IdentificationNumber = "CNP1",
                Email = "not-an-email"
            });

            act.Should().ThrowAsync<ValidationException>();
        }

        [Test]
        public async Task CreateAsync_DuplicateIdentifier_ThrowsConflict()
        {
            var clients = new Mock<IClientRepository>();
            clients.Setup(x => x.ExistsByIdentifierAsync("CNP1", It.IsAny<CancellationToken>()))
                   .ReturnsAsync(true);

            var svc = CreateSut(clients);

            Func<Task> act = async () => await svc.CreateAsync(new CreateClientRequestDTO
            {
                Name = "John",
                IdentificationNumber = "CNP1"
            });

            await act.Should().ThrowAsync<ConflictException>();
        }

        [Test]
        public async Task UpdateAsync_ClientNotFound_ThrowsNotFound()
        {
            var clients = new Mock<IClientRepository>();
            clients.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), true, It.IsAny<CancellationToken>()))
                   .ReturnsAsync((Client?)null);

            var svc = CreateSut(clients);

            Func<Task> act = async () =>
                await svc.UpdateAsync(Guid.NewGuid(), new UpdateClientRequestDTO { Name = "X" });

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task SearchAsync_ReturnsPagedResult()
        {
            var clients = new Mock<IClientRepository>();
            clients.Setup(x => x.SearchAsync(null, null, 1, 20, It.IsAny<CancellationToken>()))
                   .ReturnsAsync((new List<Client>(), 0));

            var svc = CreateSut(clients);

            var result = await svc.SearchAsync(null, null, 1, 20);

            result.TotalCount.Should().Be(0);
            result.Items.Should().BeEmpty();
            result.Page.Should().Be(1);
            result.PageSize.Should().Be(20);
        }

        [Test]
        public async Task SearchAsync_ClampsPageAndPageSize()
        {
            var clients = new Mock<IClientRepository>();
            clients.Setup(x => x.SearchAsync(null, null, 1, 20, It.IsAny<CancellationToken>()))
                   .ReturnsAsync((new List<Client>(), 0));

            var svc = CreateSut(clients);

            var result = await svc.SearchAsync(null, null, page: 0, pageSize: 999);

            result.Page.Should().Be(1);
            result.PageSize.Should().Be(20);

            clients.Verify(x => x.SearchAsync(null, null, 1, 20, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SearchAsync_ByName_PassesNameThrough()
        {
            var clients = new Mock<IClientRepository>();
            clients.Setup(x => x.SearchAsync("Ion", null, 1, 20, It.IsAny<CancellationToken>()))
                   .ReturnsAsync((new List<Client>(), 0));

            var svc = CreateSut(clients);

            await svc.SearchAsync(name: "Ion", identifier: null, page: 1, pageSize: 20);

            clients.Verify(x => x.SearchAsync("Ion", null, 1, 20, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SearchAsync_NameWithSpaces_ForwardsRawValue()
        {
            var clients = new Mock<IClientRepository>();
            clients.Setup(x => x.SearchAsync("  Ion  ", null, 1, 20, It.IsAny<CancellationToken>()))
                   .ReturnsAsync((new List<Client>(), 0));

            var svc = CreateSut(clients);

            await svc.SearchAsync(name: "  Ion  ", identifier: null, page: 1, pageSize: 20);

            clients.Verify(x => x.SearchAsync("  Ion  ", null, 1, 20, It.IsAny<CancellationToken>()), Times.Once);
        }
    }

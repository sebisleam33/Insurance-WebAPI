using AutoMapper;
using FluentAssertions;
using Insurance.Application.Common.DTOs.AdminDTOs.ReportDTOs;
using Insurance.Application.Common.Exceptions;
using Insurance.Application.Services.Implementations;
using Insurance.Domain.Entities.Reports;
using Insurance.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace Insurance.Tests.Unit;

[TestFixture]
public sealed class PolicyReportsServiceTests
    {
        private Mock<IPolicyReportsRepository> _repo = null!;
        private Mock<IMapper> _mapper = null!;
        private Mock<ILogger<PolicyReportsService>> _logger = null!;
        private PolicyReportsService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _repo = new Mock<IPolicyReportsRepository>(MockBehavior.Strict);
            _mapper = new Mock<IMapper>(MockBehavior.Strict);
            _logger = new Mock<ILogger<PolicyReportsService>>();
            _sut = new PolicyReportsService(_repo.Object, _mapper.Object, _logger.Object);
        }

        [Test]
        public async Task PoliciesByCountryAsync_WhenToBeforeFrom_ThrowsValidation()
        {
            var q = InvalidQuery();

            var act = async () => await _sut.PoliciesByCountryAsync(q);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("ToUtc must be after FromUtc.");
        }

        [Test]
        public async Task PoliciesByCountyAsync_WhenToBeforeFrom_ThrowsValidation()
        {
            var q = InvalidQuery();

            var act = async () => await _sut.PoliciesByCountyAsync(q);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("ToUtc must be after FromUtc.");
        }

        [Test]
        public async Task PoliciesByCityAsync_WhenToBeforeFrom_ThrowsValidation()
        {
            var q = InvalidQuery();

            var act = async () => await _sut.PoliciesByCityAsync(q);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("ToUtc must be after FromUtc.");
        }

        [Test]
        public async Task PoliciesByBrokerAsync_WhenToBeforeFrom_ThrowsValidation()
        {
            var q = InvalidQuery();

            var act = async () => await _sut.PoliciesByBrokerAsync(q);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("ToUtc must be after FromUtc.");
        }

        [Test]
        public async Task PoliciesByCountryAsync_MapsQuery_CallsRepo_MapsRows_ReturnsDtos()
        {
            var q = ValidQuery();
            var domainQuery = MakeDomainQuery(q);

            var rows = new[]
            {
                new PoliciesByCountryReportRow
                {
                    CountryId = 1,
                    CountryName = "Romania",
                    CurrencyId = 2,
                    CurrencyCode = "EUR",
                    PolicyCount = 2,
                    TotalFinalPremium = 150m,
                    TotalFinalPremiumInBaseCurrency = 750m
                }
            };

            var dto = new PoliciesByCountryRowDTO
            {
                CountryId = 1,
                CountryName = "Romania",
                CurrencyId = 2,
                CurrencyCode = "EUR",
                PolicyCount = 2,
                TotalFinalPremium = 150m,
                TotalFinalPremiumInBaseCurrency = 750m
            };

            SetupCommonMapQuery(q, domainQuery);

            _repo.Setup(r => r.PoliciesByCountryAsync(
                    It.Is<PolicyReportQuery>(x => SameQuery(x, q)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(rows);

            _mapper.Setup(m => m.Map<PoliciesByCountryRowDTO>(rows[0])).Returns(dto);

            var result = await _sut.PoliciesByCountryAsync(q);

            result.Should().HaveCount(1);
            result[0].Should().BeEquivalentTo(dto);

            _repo.VerifyAll();
            _mapper.VerifyAll();
        }

        [Test]
        public async Task PoliciesByCountyAsync_MapsQuery_CallsRepo_MapsRows_ReturnsDtos()
        {
            var q = ValidQuery();
            var domainQuery = MakeDomainQuery(q);

            var rows = new[]
            {
                new PoliciesByCountyReportRow
                {
                    CountryId = 1,
                    CountryName = "Romania",
                    CountyId = 10,
                    CountyName = "Cluj",
                    CurrencyId = 2,
                    CurrencyCode = "EUR",
                    PolicyCount = 3,
                    TotalFinalPremium = 300m,
                    TotalFinalPremiumInBaseCurrency = 1500m
                }
            };

            var dto = new PoliciesByCountyRowDTO
            {
                CountryId = 1,
                CountryName = "Romania",
                CountyId = 10,
                CountyName = "Cluj",
                CurrencyId = 2,
                CurrencyCode = "EUR",
                PolicyCount = 3,
                TotalFinalPremium = 300m,
                TotalFinalPremiumInBaseCurrency = 1500m
            };

            SetupCommonMapQuery(q, domainQuery);

            _repo.Setup(r => r.PoliciesByCountyAsync(
                    It.Is<PolicyReportQuery>(x => SameQuery(x, q)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(rows);

            _mapper.Setup(m => m.Map<PoliciesByCountyRowDTO>(rows[0])).Returns(dto);

            var result = await _sut.PoliciesByCountyAsync(q);

            result.Should().HaveCount(1);
            result[0].Should().BeEquivalentTo(dto);

            _repo.VerifyAll();
            _mapper.VerifyAll();
        }

        [Test]
        public async Task PoliciesByCityAsync_MapsQuery_CallsRepo_MapsRows_ReturnsDtos()
        {
            var q = ValidQuery();
            var domainQuery = MakeDomainQuery(q);

            var rows = new[]
            {
                new PoliciesByCityReportRow
                {
                    CountryId = 1,
                    CountryName = "Romania",
                    CountyId = 10,
                    CountyName = "Cluj",
                    CityId = 100,
                    CityName = "Cluj-Napoca",
                    CurrencyId = 2,
                    CurrencyCode = "EUR",
                    PolicyCount = 1,
                    TotalFinalPremium = 99m,
                    TotalFinalPremiumInBaseCurrency = 495m
                }
            };

            var dto = new PoliciesByCityRowDTO
            {
                CountryId = 1,
                CountryName = "Romania",
                CountyId = 10,
                CountyName = "Cluj",
                CityId = 100,
                CityName = "Cluj-Napoca",
                CurrencyId = 2,
                CurrencyCode = "EUR",
                PolicyCount = 1,
                TotalFinalPremium = 99m,
                TotalFinalPremiumInBaseCurrency = 495m
            };

            SetupCommonMapQuery(q, domainQuery);

            _repo.Setup(r => r.PoliciesByCityAsync(
                    It.Is<PolicyReportQuery>(x => SameQuery(x, q)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(rows);

            _mapper.Setup(m => m.Map<PoliciesByCityRowDTO>(rows[0])).Returns(dto);

            var result = await _sut.PoliciesByCityAsync(q);

            result.Should().HaveCount(1);
            result[0].Should().BeEquivalentTo(dto);

            _repo.VerifyAll();
            _mapper.VerifyAll();
        }

        [Test]
        public async Task PoliciesByBrokerAsync_MapsQuery_CallsRepo_MapsRows_ReturnsDtos()
        {
            var q = ValidQuery();
            var domainQuery = MakeDomainQuery(q);

            var brokerId = Guid.NewGuid();

            var rows = new[]
            {
                new PoliciesByBrokerReportRow
                {
                    BrokerId = brokerId,
                    BrokerCode = "BRK-A",
                    BrokerName = "Broker A",
                    CurrencyId = 2,
                    CurrencyCode = "EUR",
                    PolicyCount = 2,
                    TotalFinalPremium = 150m,
                    TotalFinalPremiumInBaseCurrency = 750m
                }
            };

            var dto = new PoliciesByBrokerRowDTO
            {
                BrokerId = brokerId,
                BrokerCode = "BRK-A",
                BrokerName = "Broker A",
                CurrencyId = 2,
                CurrencyCode = "EUR",
                PolicyCount = 2,
                TotalFinalPremium = 150m,
                TotalFinalPremiumInBaseCurrency = 750m
            };

            SetupCommonMapQuery(q, domainQuery);

            _repo.Setup(r => r.PoliciesByBrokerAsync(
                    It.Is<PolicyReportQuery>(x => SameQuery(x, q)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(rows);

            _mapper.Setup(m => m.Map<PoliciesByBrokerRowDTO>(rows[0])).Returns(dto);

            var result = await _sut.PoliciesByBrokerAsync(q);

            result.Should().HaveCount(1);
            result[0].Should().BeEquivalentTo(dto);

            _repo.VerifyAll();
            _mapper.VerifyAll();
        }

        private static PolicyReportQueryDTO InvalidQuery() => new()
        {
            FromUtc = new DateTime(2026, 1, 10),
            ToUtc = new DateTime(2026, 1, 1)
        };

        private static PolicyReportQueryDTO ValidQuery() => new()
        {
            FromUtc = new DateTime(2026, 1, 1),
            ToUtc = new DateTime(2026, 1, 31)
        };

        private static PolicyReportQuery MakeDomainQuery(PolicyReportQueryDTO q) => new()
        {
            FromUtc = q.FromUtc,
            ToUtc = q.ToUtc,
            Status = q.Status,
            CurrencyId = q.CurrencyId,
            BuildingType = q.BuildingType
        };

        private void SetupCommonMapQuery(PolicyReportQueryDTO dto, PolicyReportQuery domain)
        {
            _mapper.Setup(m => m.Map<PolicyReportQuery>(dto)).Returns(domain);
        }

        private static bool SameQuery(PolicyReportQuery x, PolicyReportQueryDTO q) =>
            x.FromUtc == q.FromUtc &&
            x.ToUtc == q.ToUtc &&
            x.Status == q.Status &&
            x.CurrencyId == q.CurrencyId &&
            x.BuildingType == q.BuildingType;
    }

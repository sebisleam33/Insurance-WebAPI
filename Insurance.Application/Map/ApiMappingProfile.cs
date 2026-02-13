using AutoMapper;
using Insurance.Application.Common.DTOs.AdminDTOs.BrokerDTOs;
using Insurance.Application.Common.DTOs.AdminDTOs.CurrencyDTOs;
using Insurance.Application.Common.DTOs.AdminDTOs.FeeDTOs;
using Insurance.Application.Common.DTOs.AdminDTOs.ReportDTOs;
using Insurance.Application.Common.DTOs.AdminDTOs.RiskFactorConfigurationDTOs;
using Insurance.Application.Common.DTOs.BuildingDTOs;
using Insurance.Application.Common.DTOs.ClientDTOs;
using Insurance.Application.Common.DTOs.GeographyDTOs;
using Insurance.Application.Common.DTOs.PolicyDTOs;
using Insurance.Domain.Entities;
using Insurance.Domain.Entities.Geography;
using Insurance.Domain.Entities.Metadata;
using Insurance.Domain.Entities.Reports;

namespace Insurance.Api.Mapping;

public sealed class ApiMappingProfile : Profile
    {
        public ApiMappingProfile()
        {

            CreateMap<CreateClientRequestDTO, Client>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.Buildings, opt => opt.Ignore())
                .ForMember(d => d.IdentifierChanges, opt => opt.Ignore())
                .ForMember(d => d.Policies, opt => opt.Ignore())
                .ForMember(d => d.Name, opt => opt.MapFrom(s => Trim(s.Name)))
                .ForMember(d => d.IdentificationNumber, opt => opt.MapFrom(s => Trim(s.IdentificationNumber)))
                .ForMember(d => d.Email, opt => opt.MapFrom(s => TrimOrNull(s.Email)))
                .ForMember(d => d.Phone, opt => opt.MapFrom(s => TrimOrNull(s.Phone)))
                .ForMember(d => d.PrimaryAddress, opt => opt.MapFrom(s => TrimOrNull(s.PrimaryAddress)));

            CreateMap<UpdateClientRequestDTO, Client>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.IdentificationNumber, opt => opt.Ignore())
                .ForMember(d => d.Type, opt => opt.Ignore())
                .ForMember(d => d.Buildings, opt => opt.Ignore())
                .ForMember(d => d.IdentifierChanges, opt => opt.Ignore())
                .ForMember(d => d.Policies, opt => opt.Ignore())
                .ForMember(d => d.Name, opt => opt.MapFrom(s => Trim(s.Name)))
                .ForMember(d => d.Email, opt => opt.MapFrom(s => TrimOrNull(s.Email)))
                .ForMember(d => d.Phone, opt => opt.MapFrom(s => TrimOrNull(s.Phone)))
                .ForMember(d => d.PrimaryAddress, opt => opt.MapFrom(s => TrimOrNull(s.PrimaryAddress)));

            CreateMap<Client, ClientListItemResponseDTO>();

            CreateMap<CreateBuildingRequestDTO, Building>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.ClientId, opt => opt.Ignore())
                .ForMember(d => d.Client, opt => opt.Ignore())
                .ForMember(d => d.City, opt => opt.Ignore())
                .ForMember(d => d.Policies, opt => opt.Ignore())
                .ForMember(d => d.Street, opt => opt.MapFrom(s => Trim(s.Street)))
                .ForMember(d => d.Number, opt => opt.MapFrom(s => Trim(s.Number)));

            CreateMap<UpdateBuildingRequestDTO, Building>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.ClientId, opt => opt.Ignore())
                .ForMember(d => d.Client, opt => opt.Ignore())
                .ForMember(d => d.City, opt => opt.Ignore())
                .ForMember(d => d.Policies, opt => opt.Ignore())
                .ForMember(d => d.Street, opt => opt.MapFrom(s => Trim(s.Street)))
                .ForMember(d => d.Number, opt => opt.MapFrom(s => Trim(s.Number)));

            CreateMap<Building, BuildingListItemResponseDTO>();

            CreateMap<Building, BuildingDetailsResponseDTO>()
                .ForMember(d => d.CityName, opt => opt.MapFrom(s => s.City.Name))
                .ForMember(d => d.CountyId, opt => opt.MapFrom(s => s.City.CountyId))
                .ForMember(d => d.CountyName, opt => opt.MapFrom(s => s.City.County.Name))
                .ForMember(d => d.CountryId, opt => opt.MapFrom(s => s.City.County.CountryId))
                .ForMember(d => d.CountryName, opt => opt.MapFrom(s => s.City.County.Country.Name));

            CreateMap<Country, CountryResponseDTO>();
            CreateMap<County, CountyResponseDTO>();
            CreateMap<City, CityResponseDTO>();

            CreateMap<CreateBrokerRequestDTO, Broker>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.Policies, opt => opt.Ignore())
                .ForMember(d => d.BrokerCode, opt => opt.MapFrom(s => Trim(s.BrokerCode)))
                .ForMember(d => d.Name, opt => opt.MapFrom(s => Trim(s.Name)))
                .ForMember(d => d.Email, opt => opt.MapFrom(s => TrimOrNull(s.Email)))
                .ForMember(d => d.Phone, opt => opt.MapFrom(s => TrimOrNull(s.Phone)));

            CreateMap<UpdateBrokerRequestDTO, Broker>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.BrokerCode, opt => opt.Ignore())
                .ForMember(d => d.Status, opt => opt.Ignore())
                .ForMember(d => d.Policies, opt => opt.Ignore())
                .ForMember(d => d.Name, opt => opt.MapFrom(s => Trim(s.Name)))
                .ForMember(d => d.Email, opt => opt.MapFrom(s => TrimOrNull(s.Email)))
                .ForMember(d => d.Phone, opt => opt.MapFrom(s => TrimOrNull(s.Phone)));

            CreateMap<Broker, BrokerResponseDTO>();

            CreateMap<Policy, PolicyListItemResponseDTO>()
                .ForMember(d => d.ClientName, opt => opt.MapFrom(s => s.Client.Name))
                .ForMember(d => d.BrokerName, opt => opt.MapFrom(s => s.Broker.Name))
                .ForMember(d => d.CurrencyCode, opt => opt.MapFrom(s => s.Currency.Code));

            CreateMap<Policy, PolicyDetailsResponseDTO>()
                .ForMember(d => d.ClientName, opt => opt.MapFrom(s => s.Client.Name))
                .ForMember(d => d.BrokerName, opt => opt.MapFrom(s => s.Broker.Name))
                .ForMember(d => d.BuildingStreet, opt => opt.MapFrom(s => s.Building.Street))
                .ForMember(d => d.BuildingNumber, opt => opt.MapFrom(s => s.Building.Number))
                .ForMember(d => d.CityId, opt => opt.MapFrom(s => s.Building.CityId))
                .ForMember(d => d.CityName, opt => opt.MapFrom(s => s.Building.City.Name))
                .ForMember(d => d.CountyId, opt => opt.MapFrom(s => s.Building.City.CountyId))
                .ForMember(d => d.CountyName, opt => opt.MapFrom(s => s.Building.City.County.Name))
                .ForMember(d => d.CountryId, opt => opt.MapFrom(s => s.Building.City.County.CountryId))
                .ForMember(d => d.CountryName, opt => opt.MapFrom(s => s.Building.City.County.Country.Name))
                .ForMember(d => d.CurrencyCode, opt => opt.MapFrom(s => s.Currency.Code));

            CreateMap<CreateFeeConfigurationRequestDTO, FeeConfiguration>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.Name, opt => opt.MapFrom(s => Trim(s.Name)));

            CreateMap<FeeConfiguration, FeeConfigurationResponseDTO>();

            CreateMap<CreateRiskFactorConfigurationRequestDTO, RiskFactorConfiguration>()
                .ForMember(d => d.Id, opt => opt.Ignore());

            CreateMap<RiskFactorConfiguration, RiskFactorConfigurationResponseDTO>();

            CreateMap<Currency, CurrencyResponseDTO>();

            CreateMap<PolicyReportQueryDTO, PolicyReportQuery>();

            CreateMap<PoliciesByCountryReportRow, PoliciesByCountryRowDTO>();
            CreateMap<PoliciesByCountyReportRow, PoliciesByCountyRowDTO>();
            CreateMap<PoliciesByCityReportRow, PoliciesByCityRowDTO>();
            CreateMap<PoliciesByBrokerReportRow, PoliciesByBrokerRowDTO>();
        }

        private static string Trim(string value) => value.Trim();

        private static string? TrimOrNull(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

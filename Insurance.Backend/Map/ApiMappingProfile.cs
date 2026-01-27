using AutoMapper;
using Insurance.Application.Common.DTOs.BuildingDTOs;
using Insurance.Application.Common.DTOs.ClientDTOs;
using Insurance.Application.Common.DTOs.GeographyDTOs;
using Insurance.Domain.Entities;
using Insurance.Domain.Entities.Geography;

namespace Insurance.Api.Mapping;

public sealed class ApiMappingProfile : Profile
{
    public ApiMappingProfile()
    {
        CreateMap<CreateClientRequestDTO, Client>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.Buildings, opt => opt.Ignore())
            .ForMember(d => d.IdentifierChanges, opt => opt.Ignore())
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name.Trim()))
            .ForMember(d => d.IdentificationNumber, opt => opt.MapFrom(s => s.IdentificationNumber.Trim()))
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email == null ? null : s.Email.Trim()))
            .ForMember(d => d.Phone, opt => opt.MapFrom(s => s.Phone == null ? null : s.Phone.Trim()))
            .ForMember(d => d.PrimaryAddress, opt => opt.MapFrom(s => s.PrimaryAddress == null ? null : s.PrimaryAddress.Trim()));

        CreateMap<UpdateClientRequestDTO, Client>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.IdentificationNumber, opt => opt.Ignore())
            .ForMember(d => d.Type, opt => opt.Ignore())
            .ForMember(d => d.Buildings, opt => opt.Ignore())
            .ForMember(d => d.IdentifierChanges, opt => opt.Ignore())
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name.Trim()))
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email == null ? null : s.Email.Trim()))
            .ForMember(d => d.Phone, opt => opt.MapFrom(s => s.Phone == null ? null : s.Phone.Trim()))
            .ForMember(d => d.PrimaryAddress, opt => opt.MapFrom(s => s.PrimaryAddress == null ? null : s.PrimaryAddress.Trim()));


        CreateMap<CreateBuildingRequestDTO, Building>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.ClientId, opt => opt.Ignore())
            .ForMember(d => d.Client, opt => opt.Ignore())
            .ForMember(d => d.City, opt => opt.Ignore())
            .ForMember(d => d.Street, opt => opt.MapFrom(s => s.Street.Trim()))
            .ForMember(d => d.Number, opt => opt.MapFrom(s => s.Number.Trim()));

        CreateMap<UpdateBuildingRequestDTO, Building>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.ClientId, opt => opt.Ignore())
            .ForMember(d => d.Client, opt => opt.Ignore())
            .ForMember(d => d.City, opt => opt.Ignore())
            .ForMember(d => d.Street, opt => opt.MapFrom(s => s.Street.Trim()))
            .ForMember(d => d.Number, opt => opt.MapFrom(s => s.Number.Trim()));

        CreateMap<Client, ClientListItemResponseDTO>();
        CreateMap<Building, BuildingListItemResponseDTO>();

        CreateMap<Country, CountryResponseDTO>();
        CreateMap<County, CountyResponseDTO>();
        CreateMap<City, CityResponseDTO>();

        CreateMap<Building, BuildingDetailsResponseDTO>()
            .ForMember(d => d.CityName, opt => opt.MapFrom(s => s.City.Name))
            .ForMember(d => d.CountyId, opt => opt.MapFrom(s => s.City.CountyId))
            .ForMember(d => d.CountyName, opt => opt.MapFrom(s => s.City.County.Name))
            .ForMember(d => d.CountryId, opt => opt.MapFrom(s => s.City.County.CountryId))
            .ForMember(d => d.CountryName, opt => opt.MapFrom(s => s.City.County.Country.Name));

            //CreateMap<Policy, PolicyListItemResponseDTO>()
            //    .ForMember(d => d.ClientName, opt => opt.MapFrom(s => s.Client.Name))
            //    .ForMember(d => d.BrokerName, opt => opt.MapFrom(s => s.Broker.Name))
            //    .ForMember(d => d.CurrencyCode, opt => opt.MapFrom(s => s.Currency.Code));

            //CreateMap<Policy, PolicyDetailsResponseDTO>()
            //    .ForMember(d => d.ClientName, opt => opt.MapFrom(s => s.Client.Name))
            //    .ForMember(d => d.BrokerName, opt => opt.MapFrom(s => s.Broker.Name))
            //    .ForMember(d => d.BuildingStreet, opt => opt.MapFrom(s => s.Building.Street))
            //    .ForMember(d => d.BuildingNumber, opt => opt.MapFrom(s => s.Building.Number))
            //    .ForMember(d => d.CityId, opt => opt.MapFrom(s => s.Building.CityId))
            //    .ForMember(d => d.CityName, opt => opt.MapFrom(s => s.Building.City.Name))
            //    .ForMember(d => d.CountyId, opt => opt.MapFrom(s => s.Building.City.CountyId))
            //    .ForMember(d => d.CountyName, opt => opt.MapFrom(s => s.Building.City.County.Name))
            //    .ForMember(d => d.CountryId, opt => opt.MapFrom(s => s.Building.City.County.CountryId))
            //    .ForMember(d => d.CountryName, opt => opt.MapFrom(s => s.Building.City.County.Country.Name))
            //    .ForMember(d => d.CurrencyCode, opt => opt.MapFrom(s => s.Currency.Code));
        }
    }

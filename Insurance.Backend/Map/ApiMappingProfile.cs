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
        // Requests -> Entities
        CreateMap<CreateClientRequestDTO, Client>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.Buildings, opt => opt.Ignore())
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name.Trim()))
            .ForMember(d => d.IdentificationNumber, opt => opt.MapFrom(s => s.IdentificationNumber.Trim()))
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email == null ? null : s.Email.Trim()))
            .ForMember(d => d.Phone, opt => opt.MapFrom(s => s.Phone == null ? null : s.Phone.Trim()))
            .ForMember(d => d.PrimaryAddress, opt => opt.MapFrom(s => s.PrimaryAddress == null ? null : s.PrimaryAddress.Trim()));

        CreateMap<UpdateClientRequestDTO, Client>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.IdentificationNumber, opt => opt.Ignore()) // immutable this sprint
            .ForMember(d => d.Type, opt => opt.Ignore())
            .ForMember(d => d.Buildings, opt => opt.Ignore())
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
            .ForMember(d => d.ClientId, opt => opt.Ignore()) // client cannot change
            .ForMember(d => d.Client, opt => opt.Ignore())
            .ForMember(d => d.City, opt => opt.Ignore())
            .ForMember(d => d.Street, opt => opt.MapFrom(s => s.Street.Trim()))
            .ForMember(d => d.Number, opt => opt.MapFrom(s => s.Number.Trim()));

        // Entities -> Responses
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
    }
}

using AutoMapper;

namespace Harbour.Application.Mapping;

/// <summary>
/// Configuración de perfiles de AutoMapper para mapear entre entidades y DTOs
/// </summary>
public class MappingProfile : Profile
{
	public MappingProfile()
	{
		// Box mappings
		CreateMap<Box, BoxDto>();

		CreateMap<CreateBoxDto, Box>();

		// Pallet mappings
		CreateMap<Pallet, PalletDetailDto>()
			.ForMember(dest => dest.SelfWeightKg, opt => opt.MapFrom(src => src.SelfWeight))
			.ForMember(dest => dest.TotalWeightKg, opt => opt.MapFrom(src => src.GetTotalWeight()))
			.ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.GetItemCount()))
			.ForMember(dest => dest.PalletTypeName, opt => opt.MapFrom(src => src.PalletTypeSpec!.Name))
			.ForMember(dest => dest.ContainerId, opt => opt.MapFrom(src => src.ParentId));

		CreateMap<PalletTypeSpec, PalletTypeSpecDto>();

		// Container mappings
		CreateMap<Container, ContainerDetailDto>()
			.ForMember(dest => dest.SelfWeightKg, opt => opt.MapFrom(src => src.SelfWeight))
			.ForMember(dest => dest.TotalWeightKg, opt => opt.MapFrom(src => src.GetTotalWeight()))
			.ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.GetItemCount()))
			.ForMember(dest => dest.ContainerTypeName, opt => opt.MapFrom(src => src.ContainerTypeSpec!.Name))
			.ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.ParentId))
			.ForMember(dest => dest.SizeInFeet, opt => opt.MapFrom(src => src.GetContainerSizeInFeet()))
			.ForMember(dest => dest.IsRefrigerated, opt => opt.MapFrom(src => src.IsRefrigerated));

		CreateMap<ContainerTypeSpec, ContainerTypeSpecDto>();

		// Ship mappings
		CreateMap<Ship, ShipDetailDto>()
			.ForMember(dest => dest.CargoItemCount, opt => opt.MapFrom(src => src.GetCargoCount()))
			.ForMember(dest => dest.TotalCargoWeightKg, opt => opt.MapFrom(src => src.GetTotalCargoWeight()))
			.ForMember(dest => dest.AvailableCapacityKg, opt => opt.MapFrom(src => src.AvailableCapacity))
			.ForMember(dest => dest.CapacityUtilization, opt => opt.MapFrom(src => src.GetCapacityUtilization()))
			.ForMember(dest => dest.CanSail, opt => opt.MapFrom(src => src.GetTotalCargoWeight() >= src.MinCapacity))
			.ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
	}
}

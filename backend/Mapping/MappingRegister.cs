using Mapster;
using VirtualShot.Entities;
using VirtualShot.Features.Identity;
using VirtualShot.Features.Party;
using VirtualShot.Features.Rooms;
using VirtualShot.Services;

namespace VirtualShot.Mapping;

/// <summary>
/// Central Mapster registration scanned at startup. Feature folders may add
/// their own IRegister classes later without touching this file.
/// </summary>
public sealed class MappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, IdentityResponse>()
            .Map(dest => dest.UserId, src => src.Id);

        config.NewConfig<Room, RoomResponse>();
        config.NewConfig<Room, RoomDetailResponse>();
        config.NewConfig<Participant, ParticipantDto>();
    }
}

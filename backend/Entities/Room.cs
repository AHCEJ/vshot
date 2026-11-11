namespace VirtualShot.Entities;

public class Room
{
    public int Id { get; set; }

    public string Code { get; set; } = "";

    public string Name { get; set; } = "";

    public int HostId { get; set; }

    public User? Host { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<RoomMember> Members { get; set; } = [];

    public List<ShotEntry> Shots { get; set; } = [];
}

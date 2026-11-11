namespace VirtualShot.Entities;

public class RoomMember
{
    public int Id { get; set; }

    public int RoomId { get; set; }

    public Room? Room { get; set; }

    public int UserId { get; set; }

    public User? User { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

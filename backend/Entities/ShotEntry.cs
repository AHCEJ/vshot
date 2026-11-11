namespace VirtualShot.Entities;

public class ShotEntry
{
    public int Id { get; set; }

    public int RoomId { get; set; }

    public Room? Room { get; set; }

    public int UserId { get; set; }

    public User? User { get; set; }

    public string Drink { get; set; } = "";

    public DateTime TakenAt { get; set; } = DateTime.UtcNow;
}

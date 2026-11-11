namespace VirtualShot.Entities;

public class User
{
    public int Id { get; set; }

    public string Nickname { get; set; } = "";

    public string Emoji { get; set; } = "😎";

    public long LifetimeShots { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

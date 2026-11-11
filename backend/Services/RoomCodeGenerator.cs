namespace VirtualShot.Services;

public static class RoomCodeGenerator
{
    // No 0/O/1/I so lobby numbers are easy to read out and type.
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    private static readonly Random Random = new();

    public static string Generate(int length = 6)
    {
        var chars = new char[length];
        lock (Random)
        {
            for (var i = 0; i < length; i++)
                chars[i] = Alphabet[Random.Next(Alphabet.Length)];
        }
        return new string(chars);
    }
}

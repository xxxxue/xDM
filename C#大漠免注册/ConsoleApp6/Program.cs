namespace Program;

class Program
{
    public static void Main(string[] args)
    {
        var dm3 = new DmSoft("dm3.dll");
        Console.WriteLine(dm3.VER);

        var dm7 = new DmSoft("dm7.dll");
        Console.WriteLine(dm7.VER);

    }
}
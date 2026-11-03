namespace CommanderD.Shell;

public class ConsoleOutputWriter : IOutputWriter
{
    public void Write(string output)
    {
        Console.Write(output);
    }

    public void WriteLn(string output)
    {
        Console.WriteLine(output);
    }
}

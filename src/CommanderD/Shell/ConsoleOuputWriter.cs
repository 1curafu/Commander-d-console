namespace CommanderD.Shell;

public class ConsoleOuputWriter : IOutputWriter
{
    public string output { get; set; }
    public void Write(string output)
    {
        Console.Write(output);
    }
    
    public void WriteLn(string output)
    {
        Console.WriteLine(output);
    }
}
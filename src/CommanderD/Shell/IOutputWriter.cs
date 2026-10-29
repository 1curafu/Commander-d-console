namespace CommanderD.Shell;

public interface IOutputWriter
{
    public string output { get; set; }
    public void Write(string output);
    public void WriteLn(string output);
}
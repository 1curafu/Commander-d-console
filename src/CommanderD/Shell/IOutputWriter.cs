namespace CommanderD.Shell;

public interface IOutputWriter
{
    void Write(string output);
    void WriteLn(string output);
}

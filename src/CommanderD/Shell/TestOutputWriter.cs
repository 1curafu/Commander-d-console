namespace CommanderD.Shell;

public class TestOutputWriter : IOutputWriter
{
    public string output{get;set;}
    
    public void Write(string output)
    {
        this.output = output;
    }

    public void WriteLn(string output)
    {
        this.output = output;
    }
}
using System.Text;

namespace CommanderD.Shell;

public class TestOutputWriter : IOutputWriter
{
    private readonly StringBuilder _buffer = new();

    public string OutputBuffer => _buffer.ToString();

    public void Write(string output)
    {
        _buffer.Append(output);
    }

    public void WriteLn(string output)
    {
        _buffer.Append(output).Append(Environment.NewLine);
    }

    public void ClearOutputBuffer()
    {
        _buffer.Clear();
    }
}

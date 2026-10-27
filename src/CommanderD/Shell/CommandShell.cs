namespace CommanderD.Shell;

public class CommandShell
{
    public void Run()
    {
        while (true)
        {
            Console.Write(">");

            var input = Console.ReadLine();

            if (input == null)
            {
                break;
            }
        }
    }
}

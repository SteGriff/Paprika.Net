using System;

namespace Paprika.Net.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var cli = new PaprikaCli();
                cli.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Oops - there was a fatal exception and Paprika has to close:");
                Console.WriteLine(ex.ToString());
                Console.ReadLine();
            }
        }
    }
}

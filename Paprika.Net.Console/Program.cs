using System;
using Con = System.Console;

namespace Paprika.Net.Console
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
                Con.WriteLine("Oops - there was a fatal exception and Paprika has to close:");
                Con.WriteLine(ex.ToString());
            }
        }

    }
}

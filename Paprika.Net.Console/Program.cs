using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Con = System.Console;

namespace Paprika.Net.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var engine = new Core();

            while (true)
            {
                Con.Write("> ");
                string input = Con.ReadLine();

                //Skip back to input if nothing was entered
                if (String.IsNullOrWhiteSpace(input))
                {
                    continue;
                }

                //Check for commands starting '//'
                if (input.Length > 1 && input.Substring(0, 2) == "//")
                {
                    string command = input.Replace("//", "");
                    switch (command)
                    {
                        case "reload":
                            Con.WriteLine("Reloading...");
                            engine = new Core();
                            Con.WriteLine("Done");
                            continue;
                    }
                }

                //Do the actual parsing
                try
                {
                    string output = engine.Parse(input);
                    Con.WriteLine(output);
                }
                catch (BracketResolutionException ex)
                {
                    Con.WriteLine("{0} in [{1}]", ex.Message, ex.Category);
                }
                catch (FormatException ex)
                {
                    Con.WriteLine(ex.Message);
                }
                catch (InputException ex)
                {
                    Con.WriteLine(ex.Message);
                }

            }

        }

        static void TestGrammar(Core engine)
        {
            var grammar = engine.Grammar;
            foreach (var g in grammar)
            {
                Con.WriteLine(g.Key);
                foreach (var x in g.Value)
                {
                    Con.WriteLine("\t{0}", x);
                }
            }
            Con.ReadLine();
        }
    }
}

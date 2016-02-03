using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Con = System.Console;

namespace Paprika.Net.Console
{
    class Program
    {
        static Core engine = new Core();
        static RunModes mode = RunModes.ConfiguredManifest;
        static string grammarSource = "";
        static string prompt = "";

        static void Main(string[] args)
        {
            LoadConfigured();

            while (true)
            {
                //Display prompt and get input
                Con.Write( prompt );
                string input = Con.ReadLine();

                //Skip back to input if nothing was entered
                if (String.IsNullOrWhiteSpace(input))
                {
                    continue;
                }

                //Check for commands starting '//'
                if (input.Length > 1 && input.Substring(0, 2) == "//")
                {
                    //Parameterise the commands after the slashes
                    var commands = input
                        .Replace("//", "")
                        .Split(new[] { ' ' });

                    switch (commands[0])
                    {
                        case "test":
                            TestGrammar(engine);
                            continue;

                        case "reload":
                            Con.WriteLine("Reloading...");
                            Reload();
                            Con.WriteLine("Done");
                            continue;

                        case "manifest":
                            string target = commands[1];
                            LoadSpecific(target);
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

        static void LoadConfigured()
        {
            mode = RunModes.ConfiguredManifest;
            prompt = ">";

            Reload();
        }

        static void LoadSpecific(string grammar)
        {
            mode = RunModes.SpecifiedManifest;
            grammarSource = grammar;
            prompt = grammar.Split(Path.DirectorySeparatorChar).Last() + " >";

            Reload();
        }

        static void Reload()
        {
            engine = new Core();

            try
            {
                switch (mode)
                {
                    case RunModes.ConfiguredManifest:
                        engine.LoadConfiguredManifest();
                        break;

                    case RunModes.SpecifiedManifest:
                        engine.LoadManifest(grammarSource);
                        break;
                }
            }
            catch (GrammarLoadingException ex)
            {
                Con.WriteLine(ex.Message);
            }

        }
    }
}

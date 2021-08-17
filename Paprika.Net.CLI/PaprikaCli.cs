using Paprika.Net.Exceptions;
using System;
using System.IO;
using System.Linq;

namespace Paprika.Net.CLI
{
    public class PaprikaCli
    {
        private PaprikaEngine engine = new PaprikaEngine();
        private RunModes mode = RunModes.WorkingDirectory;
        private string grammarSource = "";
        private string prompt = "";

        public void Run()
        {
            LoadConfigured();

            Console.WriteLine("Welcome to Paprika. Type a phrase to resolve it, or type //? for extra commands.");

            while (true)
            {
                // Display prompt and get input
                Console.Write(prompt);
                string input = Console.ReadLine();

                // Skip back to input if nothing was entered
                if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }

                // Check for commands starting '//'
                if (input.Length > 1 && input.Substring(0, 2) == "//")
                {
                    // Parameterise the commands after the slashes
                    var commands = input
                        .Replace("//", "")
                        .Split(new[] { ' ' });

                    switch (commands[0])
                    {
                        case "?":
                            HelpText();
                            continue;

                        case "test":
                            TestGrammar(engine);
                            continue;

                        case "reload":
                            Console.WriteLine("Reloading...");
                            Reload();
                            Console.WriteLine("Done");
                            continue;

                        case "manifest":
                            LoadFromCommand(commands);
                            continue;

                        case "validate":
                            Console.WriteLine("Reloading...");
                            Reload();
                            Console.WriteLine("Validating...");
                            var exs = engine.ValidateGrammar();
                            if (exs.Any())
                            {
                                foreach (var ex in exs)
                                {
                                    Console.WriteLine(ex.Message);
                                }
                            }
                            else
                            {
                                Console.WriteLine("No errors - nice");
                            }
                            continue;

                        case "login":
                            //Console.Write("Username: ");
                            //var username = Console.ReadLine();
                            //Console.Write("Password (key-presses hidden): ");
                            //var password = new PasswordHarvest().Harvest();
                            continue;

                        case "upload":
                            continue;
                    }
                }

                // Do the actual parsing
                try
                {
                    string output = engine.Parse(input);
                    Console.WriteLine(output);
                }
                catch (BracketResolutionException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (FormatException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (InputException ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
        }

        private void TestGrammar(PaprikaEngine engine)
        {
            var grammar = engine.Grammar;
            foreach (var g in grammar)
            {
                Console.WriteLine(g.Key);
                foreach (var x in g.Value)
                {
                    Console.WriteLine("\t{0}", x);
                }
            }
            Console.WriteLine("Press Return");
            Console.ReadLine();
        }

        private void LoadConfigured()
        {
            mode = RunModes.WorkingDirectory;
            prompt = ">";

            Reload();
        }

        private void LoadFromCommand(string[] commands)
        {
            if (commands.Length > 1)
            {
                string target = commands[1];
                LoadSpecific(target);
            }
            else
            {
                Console.WriteLine("You didn't specify a file after 'manifest' command, so I'll reload the configured grammar.");
                LoadConfigured();
            }
        }

        private void LoadSpecific(string grammar)
        {
            mode = RunModes.SpecifiedManifest;
            grammarSource = grammar;
            prompt = grammar.Split(Path.DirectorySeparatorChar).Last() + " >";

            Reload();
        }

        private void Reload()
        {
            engine = new PaprikaEngine();

            try
            {
                switch (mode)
                {
                    case RunModes.WorkingDirectory:
                        engine.LoadManifest(Environment.CurrentDirectory);
                        break;

                    case RunModes.SpecifiedManifest:
                        engine.LoadManifest(grammarSource);
                        break;
                }
            }
            catch (GrammarLoadingException ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private void HelpText()
        {
            Console.WriteLine("//? - This message");
            Console.WriteLine("//reload - Reloads grammars from file");
            Console.WriteLine("//validate - Reloads grammar and then validates it to find any errors");
            Console.WriteLine("//test - Writes all loaded grammar definitions to screen");
            Console.WriteLine("//count - Toggles counting on/off");
            Console.WriteLine("//manifest [file] - Load a manifest file (a list of grammars)");
            Console.WriteLine("//login - Log in to a Paprika server (allows you to upload grammars)");
            Console.WriteLine("//upload - Upload the currently loaded grammar to a Paprika server (must be logged in first)");
        }
    }
}

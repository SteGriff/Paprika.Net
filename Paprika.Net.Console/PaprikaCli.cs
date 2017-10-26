﻿using System;
using System.IO;
using System.Linq;
using Con = System.Console;

namespace Paprika.Net.Console
{
    public class PaprikaCli
    {
        private Core engine = new Core();
        private RunModes mode = RunModes.ConfiguredManifest;
        private string grammarSource = "";
        private string prompt = "";
        private bool count = false;

        public void Run()
        {
            LoadConfigured();

            Con.WriteLine("Welcome to Paprika. Type a phrase to resolve it, or type //? for extra commands.");

            while (true)
            {
                //Display prompt and get input
                Con.Write(prompt);
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
                        case "?":
                            HelpText();
                            continue;

                        case "test":
                            TestGrammar(engine);
                            continue;

                        case "reload":
                            Con.WriteLine("Reloading...");
                            Reload();
                            Con.WriteLine("Done");
                            continue;

                        case "manifest":
                            LoadFromCommand(commands);
                            continue;

                        case "count":
                            count = !count;
                            engine.CountingIsImportant = count;
                            Con.WriteLine("Counting is now: {0}", count ? "on" : "off");
                            continue;

                        case "validate":
                            Con.WriteLine("Reloading...");
                            Reload();
                            Con.WriteLine("Validating...");
                            var exs = engine.ValidateGrammar();
                            if (exs.Any())
                            {
                                foreach (var ex in exs)
                                {
                                    Con.WriteLine(ex.Message);
                                }
                            }
                            else
                            {
                                Con.WriteLine("No errors - nice");
                            }
                            continue;
                    }
                }

                //Do the actual parsing
                try
                {
                    string output = engine.Parse(input);
                    Con.WriteLine(output);

                    if (count)
                    {
                        var n = engine.NumberOfOptions();
                        Con.WriteLine("Number of options considered was between {0} and {1}", n.LowerBound, n.UpperBound);
                    }

                }
                catch (BracketResolutionException ex)
                {
                    Con.WriteLine(ex.Message);
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

        private void TestGrammar(Core engine)
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
            Con.WriteLine("Press Return");
            Con.ReadLine();
        }

        private void LoadConfigured()
        {
            mode = RunModes.ConfiguredManifest;
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
                Con.WriteLine("You didn't specify a file after 'manifest' command, so I'll reload the configured grammar.");
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

        private void HelpText()
        {
            Con.WriteLine("//? - This message");
            Con.WriteLine("//reload - Reloads grammars from file");
            Con.WriteLine("//validate - Reloads grammar and then validates it to find any errors");
            Con.WriteLine("//test - Writes all grammar definitions to screen");
            Con.WriteLine("//count - Toggles counting on/off");
            Con.WriteLine("//manifest [file] - Load a manifest file (a list of grammars)");
        }
    }
}

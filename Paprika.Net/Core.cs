using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Paprika.Net
{
    public class Core
    {
        private const string GRAMMAR_MANIFEST = "index.grammar";

        //the directory with the index.grammar file in it
        private string _rootDirectory = "../../../paprika-grammar/";
        
        private Random randomiser;

        private Dictionary<string, List<string>> _grammar;

        public Dictionary<string, List<string>> Grammar
        {
            get
            {
                if (_grammar == null)
                {
                    _grammar = new Dictionary<string, List<string>>();
                }
                return _grammar;
            }
        }

        public Core()
        {
            if (Debugger.IsAttached) { Console.WriteLine("Loading grammar..."); }

            try
            {
                PopulateGrammarFromManifest();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            

            if (Debugger.IsAttached) { Console.WriteLine("... Done"); }

            randomiser = new Random();
        }

        private string FileName(string fileName)
        {
            return _rootDirectory + fileName;
        }

        private void PopulateGrammarFromManifest()
        {
            if (!File.Exists(FileName(GRAMMAR_MANIFEST)))
            {
                throw new ApplicationException("Grammar manifest index.grammar not found");
            }

            var manifest = File.ReadAllLines(FileName(GRAMMAR_MANIFEST));

            foreach (var line in manifest)
            {
                string thisFile = line.Trim();
                if (String.IsNullOrWhiteSpace(thisFile))
                {
                    continue;
                }

                if (!File.Exists(FileName(thisFile)))
                {
                    throw new ApplicationException("Can't find linked grammar file, " + thisFile);
                }

                LoadGrammar(thisFile);
            }
        }

        private void LoadGrammar(string fileName)
        {
            string category = "";
            var categoryGrammar = new List<string>();

            Debug.WriteLine("Loading " + fileName);

            var gLines = File.ReadAllLines(FileName(fileName));

            foreach (var line in gLines)
            {
                if (IgnoreLine(line))
                {
                    continue;
                }

                if (LineIsCategory(line))
                {
                    // We've come across a new category
                    // If we already have a cat in memory, commit it

                    if (!String.IsNullOrWhiteSpace(category))
                    {
                        if (category.Contains("/"))
                        {
                            Console.WriteLine("Warning [in {0}]: {1} contains slashes. Consider deleting this category.", fileName, category);
                        }

                        try
                        {
                            CommitCategory(category, categoryGrammar);
                        }
                        catch (Exception ex)
                        {
                            throw new ApplicationException(String.Format("Error [in {0}] ({1}): {2}", fileName, category, ex.Message));
                        }

                    }

                    // Now, regardless, read the category under the cursor
                    // Omit the * character at the start
                    category = line.Substring(1);
                    Debug.WriteLine("New category: " + category);

                    // Clear the cat grammar
                    categoryGrammar = new List<string>();
                }
                else
                {
                    // It's a grammar member
                    categoryGrammar.Add(line.Trim());
                }
            }

            // Commit the final cat grammar
            CommitCategory(category, categoryGrammar);

            Debug.WriteLine("   Done\r\n");
        }

        private bool IgnoreLine(string line)
        {
            //Ignore if comment or empty
            string trimmed = line.Trim();
            return String.IsNullOrWhiteSpace(trimmed) || trimmed[0] == '#';
        }

        private bool LineIsCategory(string line)
        {
            return line[0] == '*';
        }

        private void CommitCategory(string category, List<string> categoryGrammar)
        {
            category = category.Trim();
            Debug.WriteLine(" - Set cat " + category);

            Grammar.Add(category, categoryGrammar);
        }

        public string Parse(string query)
        {
            int open = query.IndexOf('[');

            // If we do a loop with no useful operation, nops increases
            // If nops is 2, we exit with error.
            string oldQuery = "";
            int nops = 0;

            while (open > -1)
            {
                //Check for infinite loop
                if (query == oldQuery)
                {
                    nops += 1;
                    if (nops == 2)
                    {
                        throw new InputException("Found an infinite loop and quit");
                    }
                }

                oldQuery = query;

                // Here we go
                // Setup the blanking flag
                bool blank = false;

                Debug.WriteLine(query);

                int close = query.IndexOf(']');
                if (close < 0)
                {
                    throw new InputException("No closing bracket for the opening bracket at col." + open);
                }

                // Get the bracketed expression, like [sport] or [!sport]
                // $target will be replaced by $resolution
                string target = query.Substring(open, (close + 1 - open));

                //$expression will be stripped of commands, to leave a grammar key.
                string expression = target;

                string resolution = "";

                if (expression[1] == '!')
                {
                    // Hide this tag but retarget to 
                    // replace other instances (for early calls)
                    query = query.Replace(expression, "");
                    expression = expression.Replace("!", "");

                    // Update target after the '!' is removed because
                    // we want to replace all [sport] not [!sport]
                    target = expression;

                }
                else if (expression[1] == '?')
                {
                    if (randomiser.Next(2) == 0)
                    {
                        blank = true;
                    }
                    else
                    {
                        expression = expression.Replace("?", "");
                    }
                }

                if (!blank)
                {
                    resolution = ResolveBracket(expression);
                }

                // Clean up double spaces (won't catch triple spaces)
                query = query.Replace("  ", " ");

                // Do the replacement
                query = query.Replace(target, resolution);
                Debug.WriteLine("Replace all '{0}' with '{1}'", target, resolution);

                // Get next open for the while loop
                open = query.IndexOf('[');

            }

            // All [expressions] replaced

            // Replace all the a/an words which have been marked with Chr(254)-Chr(255)
            open = query.IndexOf('þ');
            while (open > -1)
            {
                int close = query.IndexOf('ÿ');
                if (close < 0)
                {
                    throw new ApplicationException("Bad a/an matching, developer messed up");
                }
                string expression = query.Substring(open, close + 1 - open);
                char nextChar = query.Substring(close + 1).TrimStart()[0];
                string resolution = IsVowel(nextChar.ToString())
                    ? "an"
                    : "a";
                query = query.Replace(expression, resolution);
                open = query.IndexOf('þ');
            }

            // Query is all fixed up; return it!
            return query;

        }

        private bool IsVowel(string c)
        {
            return "aeiou".Contains(c);
        }

        private string ResolveBracket(string expression)
        {
            // Get expression without brackets []
            string innerExpression = expression.Substring(1, expression.Length - 2);

            // Throw an error if it still contains square brackets
            int nestingPosition = innerExpression.IndexOfAny(new char[] { '[', ']' });
            if (nestingPosition > -1)
            {
                throw new FormatException("Nested brackets at col." + nestingPosition);
            }

            // Handle a label inside the tag like [sport#1] (remove the label)
            int labelPosition = innerExpression.IndexOf('#');
            if (labelPosition > -1)
            {
                innerExpression = innerExpression.Substring(0, labelPosition);
            }

            // If the brackets contain terms separated by slash, we treat those as literals
            // and randomise between them.
            if (innerExpression.Contains("/"))
            {
                var terms = innerExpression.Split('/');
                return randomFrom(terms);
            }

            if (Grammar.ContainsKey(innerExpression))
            {
                var categoryTerms = Grammar[innerExpression].ToArray();
                return randomFrom(categoryTerms);
            }
            else if (innerExpression == "a" || innerExpression == "an")
            {
                return "þ" + innerExpression + "ÿ";
            }
            else
            {
                throw new BracketResolutionException("Unknown term, can't resolve", innerExpression);
            }

        }

        private string randomFrom(string[] terms)
        {
            int randomId = randomiser.Next(terms.Length);
            return terms[randomId];
        }

    }
}

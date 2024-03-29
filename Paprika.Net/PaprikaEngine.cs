﻿using Paprika.Net.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Paprika.Net
{
    public class PaprikaEngine
    {
        private const string GRAMMAR_MANIFEST = "index.grammar";
        private const string ARTICLE_OPEN = "{{$AAN${{";
        private const string ARTICLE_CLOSE = "}}$AAN$}}";

        //the directory with the index.grammar file in it
        private string _rootDirectory;

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

        private int _totalNumberOfParseCyclesSpent;
        private readonly int _parseCyclesMax = 1024;

        /// <summary>
        /// When true, the Load routines will 'test' each line as it is read in
        /// This makes it a lot slower but catches some classes of error at 
        /// Paprika's 'compile-time' rather than at runtime
        /// </summary>
        public bool ValidateOnLoad { get; set; }

        public PaprikaEngine()
        {
            CommonInitialisation();
        }
        
        /// <summary>
        /// Load the specified dictionary as the grammar
        /// (Overwrites any existing grammar)
        /// </summary>
        public void LoadThisGrammar(Dictionary<string, List<string>> grammar)
        {
            foreach (var cat in grammar)
            {
                CommitCategory(cat.Key, cat.Value, "direct grammar");
            }
        }

        /// <summary>
        /// Load the grammars using the specified rootDirectory which contains an index.grammar file
        /// (Overwrites any existing grammar)
        /// </summary>
        /// <param name="rootDirectory">A directory containing an index.grammar file</param>
        public void LoadManifest(string rootDirectory)
        {
            _rootDirectory = rootDirectory;
            PopulateGrammarFromManifest();
        }

        private void CommonInitialisation()
        {
            randomiser = new Random();

            if (ARTICLE_OPEN.Length != ARTICLE_CLOSE.Length)
            {
                throw new CoreDeveloperException("ARTICLE_OPEN and _CLOSE constants MUST be the same length");
            }

        }

        private string FileName(string fileName)
        {
            return _rootDirectory + Path.DirectorySeparatorChar + fileName;
        }

        private void PopulateGrammarFromManifest()
        {
            string targetFile = FileName(GRAMMAR_MANIFEST);
            if (!File.Exists(targetFile))
            {
                throw new GrammarLoadingException("Grammar manifest index.grammar not found at " + targetFile);
            }

            var manifest = File.ReadAllLines(targetFile);

            foreach (var line in manifest)
            {
                string thisFile = line.Trim();
                if (string.IsNullOrWhiteSpace(thisFile))
                {
                    continue;
                }

                if (!File.Exists(FileName(thisFile)))
                {
                    throw new GrammarLoadingException("Can't find linked grammar file, " + thisFile);
                }

                LoadGrammarFromFile(thisFile);
            }
        }

        public void LoadGrammarFromString(string[] grammarString)
        {
            LoadGrammarFromString(grammarString, null);
        }

        private void LoadGrammarFromString(string[] grammarString, string source)
        {
            string category = "";
            var categoryGrammar = new List<string>();

            foreach (var line in grammarString)
            {
                if (IgnoreLine(line))
                {
                    continue;
                }

                if (LineIsCategory(line))
                {
                    // We've come across a new category
                    // If we already have a category in memory, commit it

                    if (!string.IsNullOrWhiteSpace(category))
                    {
                        CommitCategory(category, categoryGrammar, source);
                    }

                    // Now, regardless, read the category under the cursor
                    // Omit the * character at the start
                    category = line.Substring(1);

                    // Clear the cat grammar
                    categoryGrammar = new List<string>();
                }
                else
                {
                    // It's a grammar member
                    // Trim whitespace and leading escape character if any
                    var finalLine = line.Trim().TrimStart(new char[] { '\\' });
                    categoryGrammar.Add(finalLine);
                }
            }

            // Commit the final cat grammar
            CommitCategory(category, categoryGrammar, source);
        }

        public void LoadGrammarFromFile(string fileName)
        {
            Debug.WriteLine("Loading " + fileName);
            var gLines = File.ReadAllLines(FileName(fileName));
            LoadGrammarFromString(gLines);
            Debug.WriteLine("   Done\r\n");
        }

        private bool IgnoreLine(string line)
        {
            // Ignore if comment or empty. P.s. Comment character can be escaped with '\'
            string trimmed = line.Trim();
            return string.IsNullOrWhiteSpace(trimmed) || trimmed[0] == '#';
        }

        private bool LineIsCategory(string line)
        {
            return line[0] == '*';
        }

        private void CommitCategory(string category, List<string> categoryGrammar, string grammarSource)
        {
            try
            {
                category = category.Trim();
                Grammar.Add(category, categoryGrammar);
            }
            catch (Exception ex)
            {
                throw new GrammarLoadingException(string.Format("Error [in {0}] ({1}): {2}", grammarSource, category, ex.Message));
            }
        }

        public List<PaprikaException> ValidateGrammar()
        {
            var exceptionList = new List<PaprikaException>();
            foreach(var category in Grammar)
            {
                if (category.Key.Contains("/"))
                {
                    string message = string.Format("Warning: {0} contains slashes. Consider renaming or deleting this category.", category);

                    exceptionList.Add(new GrammarLoadingException(message));
                }

                foreach (var entry in category.Value)
                {
                    try
                    {
                        Parse(entry);
                    }
                    catch (PaprikaException pex)
                    {
                        exceptionList.Add(pex);
                    }
                }
            }
            return exceptionList;
        }

        public string Parse(string query)
        {
            return Parse(query, null);
        }

        public string Parse(string query, Dictionary<string, string> injectedValues)
        {
            _totalNumberOfParseCyclesSpent = 0;

            int openPos = query.IndexOf('[');

            // If we do a loop with no useful operation, nops increases
            // If nops is 2, we exit with error.
            string oldQuery = "";
            int nops = 0;

            while (openPos > -1)
            {                
                //Check for infinite loop (unchanging expression)
                if (query == oldQuery)
                {
                    nops += 1;
                    if (nops == 2)
                    {
                        throw new InputException("Found an infinite loop and quit");
                    }
                }
                oldQuery = query;

                //Check for infinite recursion
                _totalNumberOfParseCyclesSpent += 1;
                if (_totalNumberOfParseCyclesSpent >= _parseCyclesMax)
                {
                    throw new InputException("Found infinite recursion and quit");
                }

                // Here we go
                bool blank = false;

                int closePos = query.IndexOf(']');
                if (closePos < 0)
                {
                    throw new InputException(string.Format("No closing bracket for the opening bracket at col.{0} in '{1}'", openPos, query));
                }

                // Get the bracketed expression, like [sport] or [!sport]
                // target will be replaced by resolution
                string target = query.Substring(openPos, (closePos + 1 - openPos));

                // expression will be stripped of commands, to leave a grammar key.
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
                }

                if (!blank)
                {
                    expression = expression.Replace("?", "");
                    resolution = ResolveBracket(expression, injectedValues);
                    if (blank)
                    {
                        resolution = "";
                    }
                }

                // Clean up double spaces (won't catch triple spaces)
                query = query.Replace("  ", " ");

                // Do the replacement
                query = query.Replace(target, resolution);

                // Get next open for the while loop
                openPos = query.IndexOf('[');
            }

            // All [expressions] replaced

            // Replace all the a/an words which have been marked
            query = FixAAndAn(query);

            // Query is all fixed up; return it!
            return query.Trim();
        }

        private string FixAAndAn(string query)
        {
            int len = ARTICLE_OPEN.Length;

            int openPos = query.IndexOf(ARTICLE_OPEN);
            while (openPos > -1)
            {
                int closePos = query.IndexOf(ARTICLE_CLOSE);
                if (closePos < 0)
                {
                    throw new CoreDeveloperException("No matching ARTICLE_CLOSE found for an _OPEN");
                }

                char nextChar = query.Substring(closePos + len).TrimStart()[0];
                string resolution = IsVowel(nextChar.ToString())
                    ? "an"
                    : "a";

                // Slice and insert replacement
                string beforeOpen = query.Substring(0, openPos);
                string afterClose = query.Substring(closePos + len);
                query = beforeOpen + resolution + afterClose;

                // Get next
                openPos = query.IndexOf(ARTICLE_OPEN);
            }

            return query;
        }

        private bool IsVowel(string c)
        {
            return "aeiou".Contains(c);
        }

        private string ResolveBracket(string expression, Dictionary<string, string> injectedValues)
        {
            // Get expression without brackets []
            string innerExpression = expression.Substring(1, expression.Length - 2);

            // Throw an error if it still contains square brackets
            int nestingPosition = innerExpression.IndexOfAny(new char[] { '[', ']' });
            if (nestingPosition > -1)
            {
                throw new BracketResolutionException("Nested brackets at col." + nestingPosition + ": \"" + expression + "\"", expression);
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
                return RandomFrom(terms);
            }

            if (injectedValues != null && injectedValues.ContainsKey(innerExpression))
            {
                return injectedValues[innerExpression];
            }
            else if (Grammar.ContainsKey(innerExpression))
            {
                var categoryTerms = Grammar[innerExpression].ToArray();
                return RandomFrom(categoryTerms);
            }
            else if (innerExpression == "a" || innerExpression == "an")
            {
                return ARTICLE_OPEN + innerExpression + ARTICLE_CLOSE;
            }
            else
            {
                throw new BracketResolutionException("Unknown term, can't resolve, [" + innerExpression + "]", innerExpression);
            }

        }

        private string RandomFrom(string[] terms)
        {
            if (terms.Length == 0)
            {
                return "";
            }
            try
            {
                int randomId = randomiser.Next(terms.Length);
                return terms[randomId];
            }
            catch (Exception ex)
            {
                string combinedTerms = string.Join(";", terms);
                throw new BracketResolutionException("Failed to pick random entry from (" + combinedTerms + "): " + ex.ToString(), combinedTerms);
            }
        }

    }
}

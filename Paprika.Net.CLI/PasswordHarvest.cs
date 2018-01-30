using System;

namespace Paprika.Net.CLI
{
    public class PasswordHarvest
    {
        public string Harvest()
        {
            string gathered = "";
            ConsoleKey lastKey = ConsoleKey.NoName;
            while(lastKey != ConsoleKey.Enter && lastKey != ConsoleKey.Escape)
            {
                var keypress = Console.ReadKey(true);
                lastKey = keypress.Key;
                gathered += keypress.KeyChar;
            }
            return gathered;
        }

    }
}

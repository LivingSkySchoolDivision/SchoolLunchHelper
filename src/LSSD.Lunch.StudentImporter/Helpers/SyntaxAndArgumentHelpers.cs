using System;
using System.Collections.Generic;
using System.Linq;

namespace LSSD.Lunch.StudentImporter
{
    public class SyntaxAndArgumentHelpers
    {
        private static readonly Dictionary<string, string> _commands = new Dictionary<string, string>()
        {
            { "/inputfile", "Filename for the CSV to import." },
            { "/configfile", "Filename for the configuration file for this utility." },
            { "/help, /h, /?", "Display this help message." }

        };

        private static void SendSyntax()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Parameters:");

            // Find the max characters the parameter names use up
            int maxChars = _commands.Keys.Max(x => x.Length);
            int descriptionMaxLength = Console.WindowWidth - maxChars - 8;

            foreach (KeyValuePair<string, string> availableCommand in _commands.OrderBy(x => x.Key))
            {
                Console.Write(" " + availableCommand.Key);

                // Split the description into chunks of a certain number of characters
                foreach (string description in availableCommand.Value.SplitIntoLines(descriptionMaxLength))
                {
                    Console.SetCursorPosition(maxChars + 3, Console.CursorTop);
                    Console.WriteLine(description);
                }
                //Console.WriteLine("");
            }

            Console.ResetColor();
        }

        private static Dictionary<string, string> extractArguments(string[] args)
        {
            Dictionary<string, string> returnMe = new Dictionary<string, string>();
            
            for(int x = 0; x < args.Length; x++)
            {
                if (args[x].StartsWith('/'))
                {
                    if (args.Length > x+1)
                    {
                        if (!args[x+1].StartsWith('/'))
                        {
                            returnMe.Add(args[x].Trim().ToLower(), args[x + 1].Trim());
                        } else
                        {
                            returnMe.Add(args[x].Trim().ToLower(), string.Empty);
                        }
                    } else
                    {
                        returnMe.Add(args[x].Trim().ToLower(), string.Empty);
                    }
                }
            }

            return returnMe;
        }
    }
}
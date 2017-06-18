using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            FileCopierCommandLineParser parser = new FileCopierCommandLineParser();
            parser.OptionParamValueAnnouncerChar = ' '; // Use space to separate an option name from its value
            string errorText;
            if (!parser.Parse(Environment.CommandLine, out errorText))
                Console.WriteLine("Error parsing command line: " + errorText);
            else
            {
                if (parser.SourceFile != null)
                {
                    Console.WriteLine($"Source: {parser.SourceFile}");
                }
                if (parser.DestFile != null)
                {
                    Console.WriteLine($"Destination: {parser.DestFile}");
                }
                if (parser.Overwrite)
                {
                    Console.WriteLine("Overwrite: true");
                }
                if (parser.MaxSize.HasValue)
                {
                    Console.WriteLine($"Max Size: {parser.MaxSize}");
                }
            }
        }
    }
}

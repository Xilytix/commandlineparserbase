# Xilytix.CommandLineParserBase

## Overview

Xilytix.CommandLineParserBase contains a base class, CLParserBase, from which a Console Application Command Line parser class can be derived.

CLParserBase does the parsing of the Command Line.  It will call one of the following 2 virtual methods whenever it has parsed a text parameter or an option in the command line.

* bool ProcessTextParam(int textParamIdx, string value, int linePosIdx, out string errorText)
* bool ProcessOption(string name, string value, out string errorText)

The derived class simply needs to override one or both of these methods.  The example below illustrates this.

    class FileCopierCommandLineParser : CLParserBase
    {
        public bool Overwrite = false; // -o
        public int? MaxSize; // -m:
        public string SourceFile; // 1st Param 
        public string DestFile; // 2nd Param 

        protected override bool ProcessOption(string name, string value, out string errorText)
        {
            errorText = null;
            switch (name[0])
            {
                case 'o':
                    Overwrite = true;
                    return true;
                case 'm':
                    MaxSize = Int32.Parse(value);
                    return true;
                default:
                    errorText = "Unsupported Option: " + name;
                    return false;
            }
        }

        protected override bool ProcessTextParam(int textParamIdx, string value, int linePosIdx, out string errorText)
        {
            errorText = null;
            switch (textParamIdx)
            {
                case 0:
                    // Executable path 
                    return true;
                case 1:
                    SourceFile = value;
                    return true;
                case 2:
                    DestFile = value;
                    return true;
                default:
                    errorText = "Too many parameters";
                    return false;
            }
        }
    }

This parser can then be simply used as shown below.

    FileCopierCommandLineParser parser = new FileCopierCommandLineParser();
    
    if (!parser.Parse(Environment.CommandLine, out errorText))
        throw new Exception("Command Line Error: " + errorText);
    else
        SpecialCopyFile(parser.SourceFile, parser.DestFile, parser.Overwrite, parser.MaxSize);

## Capabilities

The specifications and capabilities of CLParserBase are:

* Any parameter beginning with any of the Option Parameter Announcer characters {'-'}, is an option.
* If an option parameter contains the Option Parameter Value Announcer character (':'), then the option has both a name and a value.  If the announcer character is not in the option, then the option only contains a name (ie it is a flag).
* Text parameters and Option parameter values can include spaces if they are enclosed in the quote character ('"').  Note that an Option name cannot contain spaces.
* A quoted text parameter or quoted option value can include Quote characters by doubling each included Quote character (ie 2 successive Quote characters are considered as one Quote character).
* If any of the "Parse Terminate" characters {'<', '>', '|'} are encountered in the command line, then the rest of the command line is ignored.  These characters signify that the rest of the command line is NOT to be interpretted by the application.
* Alternative "Option Parameter Announcer" characters, "Option Parameter Value Announcer" character, Quote character and "Parse Terminate" characters can be specified by CLParserBase properties.  

## Advantage

There are a lot more capable Command Line Parsers available. They typically work with a registration syntax specifying how the command line should be interpretted. With CLParserBase there is no registration syntax to remember.  Just override ProcessTextParam() and/or ProcessOption().

CLParserBase would be ideal for simple console applications and quick hacks.  Easy to remember and easy to read.  For anything more sophisticated, there are better alternatives available.
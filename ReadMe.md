# Xilytix.CommandLineParserBase

## Overview

Xilytix.CommandLineParserBase contains a base class, CLParserBase, from which a Command Line parser class can be derived.

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

    protected override bool CanOptionHaveValue(string name)
    {
        // only need to override CanOptionHaveValue() if OptionParamValueAnnouncerChar = ' '
        // (or other white space character)
        return name == "m";
    }

## Special case of using space character to separate an option from its value

The third override, **CanOptionHaveValue(string name)**, is only required if the character separating an option name from its value is specified by a **' '** (space) or any other white space character. In this case, there is an ambiguity about whether the text after the option name is the option's value or a separate text parameter. For example, is **-m 20000** an option with the value 20000 or is **m** a flag set to true and **20000** a separate text parameter?

Whenever such an ambiguity arises, CanOptionHaveValue() will be called to resolve the ambiguity.  If CanOptionHaveValue() returns true, then the text after the option name will be its value and ProcessOption() option will be called with the text as the value parameter. Otherwise the option is a flag and ProcessOption() will be called with the value parameter holding null (and subsequently, ProcessTextParam() will be called with the text as the value parameter).

Note that if the text after the option name begins with one of the OptionParamAnnouncer characters (eg. **'-'**), then it is considered as a separate option. In this case there is no ambiguity and CanOptionHaveValue() will not be called.  For example, in **-o -m 20000** CanOptionHaveValue() will not be called for the **-o** option as the subsequent text **-m** is a separate option.

If CanOptionHaveValue() is not overrided, then no options are assumed to have values. That is, they are all treated as flags. 

## Using

Once the derived parser is declared, it can then be used as shown below.

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
* The "Option Parameter Value Announcer" character can be set to space (**' '**). In this case spaces are used to separate command line parameters and separate option values from their names. In this case CanOptionHaveValue() should also be overrided as described above.

## Advantage

There are a lot more capable Command Line Parsers available. They typically work with a registration syntax specifying how the command line should be interpretted. With CLParserBase there is no registration syntax to remember.  Just override ProcessTextParam() and/or ProcessOption() (and maybe CanOptionHaveValue()).

CLParserBase would be ideal for simple console applications and quick hacks.  Easy to remember and easy to read.  For anything more sophisticated, there are better alternatives available.
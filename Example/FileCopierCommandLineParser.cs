using System;
using Xilytix.CommandLineParserBase;

namespace Example
{
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

        protected override bool CanOptionHaveValue(string name)
        {
            // only need to implement this override if OptionParamValueAnnouncerChar = ' ' (or other white space character)
            return name == "m";
        }
    }
}

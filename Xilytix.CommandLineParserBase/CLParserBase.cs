// Project: Xilytix.CommandLineParserBase
// Licence: Public Domain
// Web Home Page: https://bitbucket.org/xilytix/commandlineparserbase/overview
// Initial Developer: Paul Klink (http://paul.klink.id.au)

using System;
using System.Diagnostics;
using System.Text;

namespace Xilytix.CommandLineParserBase
{
    public abstract class CLParserBase
    {
        private enum ParseState
        {
            NotInParam,
            TextParam,
            TextParamPossibleEndQuote,
            OptionParam,
        }

        private enum ParseOptionState
        {
            Announced,
            Name,
            ValuePossible,
            ValueAnnounced,
            Value,
            ValuePossibleEndQuote
        }

        static public char DefaultQuoteChar = '"';
        static public char[] DefaultOptionParamAnnouncerChars = new char[1] {'-'};
        static public char DefaultOptionParamValueAnnouncerChar = ':';
        static public char[] DefaultParseTerminateChars = new char[3] {'<', '>', '|'};

        private char quoteChar = DefaultQuoteChar;
        private char[] optionParamAnnouncerChars = DefaultOptionParamAnnouncerChars;
        private char optionParamValueAnnouncerChar = DefaultOptionParamValueAnnouncerChar;
        private char[] parseTerminateChars = DefaultParseTerminateChars;

        private bool optionParamValueAnnouncerIsWhiteSpace = false;

        private string parseLine;
        private int textParamIdx;
        private ParseState parseState;
        private ParseOptionState parseOptionState;
        private int startIdx;
        private string optionName;
        private bool quoted;
        private bool ignoreRestOfLine;
        private bool result;
        private StringBuilder textBldr;
        private char[] optionTerminationChars;

        /// <summary>
        /// Specifies the character used to enclose all text in a parameter or a option parameter value.
        /// </summary>
        /// <remarks>
        /// Spaces are used to delimit parameters in a command line.  If a parameter or a parameter value contains spaces,
        /// place the QuoteChar at either end of the parameter or value text.  If the parameter or value already contains 
        /// one or more QuoteChar, then replace each QuoteChar with 2 QuoteChars before placing a QuoteChar at either end
        /// of the text.
        /// <para>
        /// You need to use QuoteChars around a parameter or value if the text starts with a QuoteChar.  Make sure you
        /// replace the existing QuoteChars with 2 QuoteChars before placing a QuoteChar at each end.
        /// <para>
        /// Default: "  (Double Quote character)
        /// </remarks>
        public char QuoteChar
        {
            get { return quoteChar; } set { quoteChar = value; }
        }

        /// <summary>
        /// An array of characters which can be used to signify the start of an option parameter in the command line.
        /// Any command line parameter which begins with one of the characters in this array will be parsed as a option
        /// parameter.
        /// <para>
        /// Default: -  (Dash character is the only character in the array)
        /// </summary>
        public char[] OptionParamAnnouncerChars
        {
            get { return optionParamAnnouncerChars; } set { optionParamAnnouncerChars = value; }
        }

        /// <summary>
        /// Specifies the character which separates an option parameter with its value. If an option parameter does not
        /// contain this character, then it is a switch/flag only and does not include a value. If it does contain this
        /// character, then the characters prior to this character are the option parameter name and the characters after
        /// it, are the option parameters value.
        /// <para>
        /// Default: :  (Colon character)
        /// </summary>
        public char OptionParamValueAnnouncerChar
        {
            get { return optionParamValueAnnouncerChar; }
            set
            {
                optionParamValueAnnouncerChar = value;
                optionParamValueAnnouncerIsWhiteSpace = char.IsWhiteSpace(optionParamValueAnnouncerChar);
            }
        }

        /// <summary>
        /// If the parser reads any of the characters in the ParseTerminateChars array, then it will ignore that character
        /// and all remaining characters in the command line.  This can be used to ignore standard input/output
        /// redirection and the end of a command line.
        /// <para>
        /// Default: <>|  (standard input redirection to file, standard output redirection to file, pipe standard output)
        /// </summary>
        public char[] ParseTerminateChars
        {
            get { return parseTerminateChars; } set { parseTerminateChars = value; }
        }

        private bool PrepareErrorText(CLParserError.Id errorId, string errorParam, out string errorText)
        {
            errorText = MakeErrorText(errorId, errorParam);
            return false;
        }

        /// <summary>
        /// The virtual ProcessTextParam method is called whenever the parser has finished a text parameter (ie a 
        /// parameter which is not an option).
        /// </summary>
        /// <param name="textParamIdx">Parameter index in command line</param>
        /// <param name="value">Value of text parameter.</param>
        /// <param name="linePosIdx">Position index of parameter in command line.</param>
        /// <param name="errorText">Contains error text if method returns false. Otherwise contains null</param>
        /// <returns>Returns true if parameter value is acceptable.  Returns false if value not acceptable.</returns>
        /// <remarks>Descendant classes should override this method and interpret parameter.  If parameter is not
        /// acceptable, then descendant override should return false and include error description in errorText parameter.</remarks>
        protected virtual bool ProcessTextParam(int textParamIdx, string value, int linePosIdx, out string errorText)
        {
            errorText = null;
            return true;
        }

        /// <summary>
        /// The virtual ProcessOption method is called whenever the parser has completely read an option parameter (including 
        /// name and value if present).
        /// </summary>
        /// <param name="name">Name of option parameter.</param>
        /// <param name="value">Option parameter's value. Will be null if option does not have a value.</param>
        /// <param name="errorText">Contains error text if method returns false. Otherwise contains null</param>
        /// <returns>Returns true if option name and value are acceptable.  Returns false if name or value are not acceptable.</returns>
        /// <remarks>Descendant classes should override this method and interpret option name and value.  If option name
        /// or value are not acceptable, then descendant override should return false and include error description in 
        /// errorText parameter.
        /// </remarks>
        protected virtual bool ProcessOption(string name, string value, out string errorText)
        {
            errorText = null;
            return true;
        }

        protected virtual bool CanOptionHaveValue(string name)
        {
            return false;
        }

        protected internal virtual string MakeErrorText(CLParserError.Id errorId, string errorParam)
        {
            string Result = CLParserError.IdToDefaultText(errorId);
            if (CLParserError.IdToHasParam(errorId))
            {
                Result = Result + ": " + errorParam;
            }
            return Result;
        }

        public virtual bool Parse(string line, out string errorText)
        {
            parseLine = line;
            textParamIdx = -1;
            parseState = ParseState.NotInParam;
            parseOptionState = ParseOptionState.Announced;
            startIdx = -1;
            optionName = null;
            quoted = false;
            ignoreRestOfLine = false;
            textBldr = new StringBuilder(30);
            optionTerminationChars = CreateOptionTerminationCharArray();
            errorText = null;

            for (int i = 0; i < line.Length; i++)
            {
                result = ProcessChar(i, line[i], out errorText);
                if (!result || ignoreRestOfLine)
                {
                    break;
                }
            }

            if (result && !ignoreRestOfLine)
            {
                switch (parseState)
                {
                    case ParseState.NotInParam:
                        break;

                    case ParseState.TextParam:
                        if (quoted)
                            result = PrepareErrorText(CLParserError.Id.TextMissingClosingQuoteCharacter, null, out errorText);
                        else
                            result = ProcessTextParam(textParamIdx, textBldr.ToString(), startIdx, out errorText);
                        break;

                    case ParseState.TextParamPossibleEndQuote:
                        result = ProcessTextParam(textParamIdx, textBldr.ToString(), startIdx, out errorText);
                        break;

                    case ParseState.OptionParam:
                        switch (parseOptionState)
                        {
                            case ParseOptionState.Announced:
                                result = PrepareErrorText(CLParserError.Id.OptionNotSpecifiedAtLinePosition, line.Length.ToString(), out errorText);
                                break;
                            case ParseOptionState.Name:
                                optionName = line.Substring(startIdx, line.Length - startIdx);
                                result = ProcessOption(optionName, null, out errorText);
                                break;
                            case ParseOptionState.ValuePossible:
                                result = ProcessOption(optionName, null, out errorText);
                                break;
                            case ParseOptionState.ValueAnnounced:
                                result = ProcessOption(optionName, "", out errorText);
                                break;
                            case ParseOptionState.Value:
                                if (quoted)
                                    result = PrepareErrorText(CLParserError.Id.OptionMissingClosingQuoteCharacter, optionName, out errorText);
                                else
                                    result = ProcessOption(optionName, textBldr.ToString(), out errorText);
                                break;
                            case ParseOptionState.ValuePossibleEndQuote:
                                result = ProcessOption(optionName, textBldr.ToString(), out errorText);
                                break;
                        }
                        break;

                    default:
                        Debug.Assert(false, "False");
                        break;
                }
            }

            return result;
        }

        private bool ProcessChar(int i, char lineChar, out string errorText)
        {
            result = true;
            errorText = null;

            switch (parseState)
            {
                case ParseState.NotInParam:
                    if (lineChar == quoteChar)
                    {
                        parseState = ParseState.TextParam;
                        textParamIdx++;
                        startIdx = i;
                        textBldr.Clear();
                        quoted = true;
                    }
                    else
                    {
                        if (Array.IndexOf<char>(optionParamAnnouncerChars, lineChar) >= 0)
                        {
                            parseState = ParseState.OptionParam;
                            parseOptionState = ParseOptionState.Announced;
                            startIdx = i + 1;
                        }
                        else
                        {
                            if (Array.IndexOf<char>(parseTerminateChars, lineChar) >= 0)
                                ignoreRestOfLine = true;
                            else
                            {
                                if (!char.IsWhiteSpace(lineChar))
                                {
                                    parseState = ParseState.TextParam;
                                    textParamIdx++;
                                    startIdx = i;
                                    textBldr.Clear();
                                    textBldr.Append(lineChar);
                                    quoted = false;
                                }
                            }
                        }
                    }
                    break;

                case ParseState.TextParam:
                    if (!quoted)
                    {
                        if (!char.IsWhiteSpace(lineChar))
                            textBldr.Append(lineChar);
                        else
                        {
                            result = ProcessTextParam(textParamIdx, textBldr.ToString(), startIdx, out errorText);
                            parseState = ParseState.NotInParam;
                        }
                    }
                    else
                    {
                        if (lineChar != QuoteChar)
                            textBldr.Append(lineChar);
                        else
                            parseState = ParseState.TextParamPossibleEndQuote;
                    }
                    break;

                case ParseState.TextParamPossibleEndQuote:
                    if (lineChar == QuoteChar)
                    {
                        textBldr.Append(lineChar);
                        parseState = ParseState.TextParam;
                    }
                    else
                    {
                        if (char.IsWhiteSpace(lineChar))
                        {
                            result = ProcessTextParam(textParamIdx, textBldr.ToString(), startIdx, out errorText);
                            parseState = ParseState.NotInParam;
                        }
                        else
                        {
                            result = PrepareErrorText(CLParserError.Id.InvalidQuoteCharacterInTextParameter, textBldr.ToString(), out errorText);
                        }
                    }
                    break;

                case ParseState.OptionParam:
                    switch (parseOptionState)
                    {
                        case ParseOptionState.Announced:
                            if (char.IsWhiteSpace(lineChar) || Array.IndexOf<char>(optionTerminationChars, lineChar) >= 0)
                                result = PrepareErrorText(CLParserError.Id.OptionNotSpecifiedAtLinePosition, i.ToString(), out errorText);
                            else
                                parseOptionState = ParseOptionState.Name;
                            break;
                        case ParseOptionState.Name:
                            bool optionValueAnnounced = lineChar == optionParamValueAnnouncerChar;
                            if (optionValueAnnounced || char.IsWhiteSpace(lineChar) || Array.IndexOf<char>(optionTerminationChars, lineChar) >= 0)
                            {
                                optionName = parseLine.Substring(startIdx, i - startIdx);
                                if (optionName == "")
                                    result = PrepareErrorText(CLParserError.Id.OptionNotSpecifiedAtLinePosition, i.ToString(), out errorText);
                                else
                                {
                                    if (optionValueAnnounced)
                                    {
                                        if (optionParamValueAnnouncerIsWhiteSpace)
                                            parseOptionState = ParseOptionState.ValuePossible;
                                        else
                                            parseOptionState = ParseOptionState.ValueAnnounced;
                                    }
                                    else
                                    {
                                        result = ProcessOption(optionName, null, out errorText);
                                        if (result)
                                        {
                                            parseState = ParseState.NotInParam;
                                        }
                                    }
                                }
                            }
                            break;
                        case ParseOptionState.ValuePossible:
                            if (!char.IsWhiteSpace(lineChar))
                            {
                                if (
                                    Array.IndexOf<char>(optionParamAnnouncerChars, lineChar) < 0 // not a new option
                                    &&
                                    CanOptionHaveValue(optionName) // option can have values
                                    )
                                {
                                    parseOptionState = ParseOptionState.ValueAnnounced;
                                    result = ProcessChar(i, lineChar, out errorText); // process first char of value
                                }
                                else
                                {
                                    result = ProcessOption(optionName, null, out errorText); // process current option
                                    if (result)
                                    {
                                        parseState = ParseState.NotInParam;
                                        result = ProcessChar(i, lineChar, out errorText); // will handle new option/text param
                                    }
                                }
                            }
                            break;
                        case ParseOptionState.ValueAnnounced:
                            startIdx = i;
                            textBldr.Clear();
                            if (char.IsWhiteSpace(lineChar))
                            {
                                result = ProcessOption(optionName, "", out errorText);
                                parseState = ParseState.NotInParam;
                            }
                            else
                            {
                                if (lineChar == QuoteChar)
                                    quoted = true;
                                else
                                {
                                    quoted = false;
                                    textBldr.Append(lineChar);
                                }
                                parseOptionState = ParseOptionState.Value;
                            }
                            break;
                        case ParseOptionState.Value:
                            if (!quoted)
                            {
                                if (!char.IsWhiteSpace(lineChar))
                                    textBldr.Append(lineChar);
                                else
                                {
                                    result = ProcessOption(optionName, textBldr.ToString(), out errorText);
                                    parseState = ParseState.NotInParam;
                                }
                            }
                            else
                            {
                                if (lineChar != QuoteChar)
                                    textBldr.Append(lineChar);
                                else
                                    parseOptionState = ParseOptionState.ValuePossibleEndQuote;
                            }
                            break;
                        case ParseOptionState.ValuePossibleEndQuote:
                            if (lineChar == QuoteChar)
                            {
                                textBldr.Append(lineChar);
                                parseOptionState = ParseOptionState.Value;
                            }
                            else
                            {
                                if (char.IsWhiteSpace(lineChar))
                                {
                                    result = ProcessOption(optionName, textBldr.ToString(), out errorText);
                                    parseState = ParseState.NotInParam;
                                }
                                else
                                {
                                    result = PrepareErrorText(CLParserError.Id.InvalidQuoteCharacterInOptionValue, optionName, out errorText);
                                }
                            }
                            break;
                    }
                    break;
            }

            return result;
        }

        private char[] CreateOptionTerminationCharArray()
        {
            char[] result = new char[2 + parseTerminateChars.Length];
            result[0] = quoteChar;
            result[1] = optionParamValueAnnouncerChar;
            parseTerminateChars.CopyTo(result, 2);

            return result;
        }
    }
}

using System;

namespace Xilytix.CommandLineParserBase
{
    public class CLParserError
    {
        public enum Id
        {
            InvalidQuoteCharacterInTextParameter,
            OptionNotSpecifiedAtLinePosition,
            InvalidQuoteCharacterInOptionValue,
            TextMissingClosingQuoteCharacter,
            OptionMissingClosingQuoteCharacter
        }

        public static bool IdToHasParam(Id id)
        {
            return ErrorLookup.IdToHasParam(id);
        }

        public static string IdToDefaultText(Id id)
        {
            return ErrorLookup.IdToDefaultText(id);
        }
    }
}

namespace Xilytix.CommandLineParserBase
{
    using System;

    using Id = CLParserError.Id;
    using Definition = ErrorDefinition;

    internal struct ErrorLookup
	{
        static private Definition[] definitionArray =
        {
            new Definition
            {
                Id = Id.InvalidQuoteCharacterInTextParameter,
                HasParam = true,
                DefaultText = Properties.Resources.Error_InvalidQuoteCharacterInTextParameter
            },
            new Definition
            {
                Id = Id.OptionNotSpecifiedAtLinePosition,
                HasParam = true,
                DefaultText = Properties.Resources.Error_OptionNotSpecifiedAtLinePosition
            },
            new Definition
            {
                Id = Id.InvalidQuoteCharacterInOptionValue,
                HasParam = true,
                DefaultText = Properties.Resources.Error_InvalidQuoteCharacterInOptionValue
            },
            new Definition
            {
                Id = Id.TextMissingClosingQuoteCharacter,
                HasParam = false,
                DefaultText = Properties.Resources.Error_TextMissingClosingQuoteCharacter
            },
            new Definition
            {
                Id = Id.OptionMissingClosingQuoteCharacter,
                HasParam = true,
                DefaultText = Properties.Resources.Error_OptionMissingClosingQuoteCharacter
            }
        };

		internal static bool IdToHasParam(Id Id)
		{
			return definitionArray[(int)Id].HasParam;
		}

		internal static string IdToDefaultText(Id Id)
		{
			return definitionArray[(int)Id].DefaultText;
		}

        internal static void TestStatic()
        {
            const string ArrayName = "ErrorDefinition";

            if (definitionArray.Length != Enum.GetNames(typeof(Id)).Length)
                throw new InvalidOperationException(string.Format("Incorrect length of {0} array", ArrayName));
            else
            {
                for (int i = 0; i < definitionArray.Length; i++)
                {
                    if ((int)definitionArray[i].Id != i)
                    {
                        throw new InvalidOperationException(string.Format("{0} Lookup array out of order: {1}", ArrayName, definitionArray[i].Id.ToString()));
                    }
                }
            }
        }
    }
}

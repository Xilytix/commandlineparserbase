// Project: Xilytix.CommandLineParserBase
// Licence: Public Domain
// Web Home Page: https://bitbucket.org/xilytix/commandlineparserbase/overview
// Initial Developer: Paul Klink (http://paul.klink.id.au)

namespace Xilytix.CommandLineParserBase
{
    using Id = CLParserError.Id;

    internal struct ErrorDefinition
    {
        internal Id Id;
        internal bool HasParam;
        internal string DefaultText;
    }
}

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

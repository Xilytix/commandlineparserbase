using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xilytix.CommandLineParserBase.UnitTest
{
    [TestClass]
    public class Test
    {
        [TestMethod]
        public void TestStatic()
        {
            StaticTester.Test();
        }

        [TestMethod]
        public void TestParser()
        {
            string errorText;

            Parser parser = new Parser();
            Assert.IsTrue(parser.Parse("A", out errorText));
            Assert.AreEqual<int>(1, parser.TextParams.Length);
            Assert.AreEqual<string>("A", parser.TextParams[0]);

            Assert.IsTrue(parser.Parse("-F A", out errorText));
            Assert.IsTrue(parser.Flag);
            Assert.AreEqual<int>(1, parser.TextParams.Length);
            Assert.AreEqual<string>("A", parser.TextParams[0]);

            Assert.IsTrue(parser.Parse("A -Flag B", out errorText));
            Assert.IsTrue(parser.Flag);
            Assert.AreEqual<int>(2, parser.TextParams.Length);
            Assert.AreEqual<string>("A", parser.TextParams[0]);
            Assert.AreEqual<string>("B", parser.TextParams[1]);

            Assert.IsTrue(parser.Parse("Abc -V:XX def", out errorText));
            Assert.IsFalse(parser.Flag);
            Assert.AreEqual<string>("XX", parser.OptionValue);
            Assert.AreEqual<int>(2, parser.TextParams.Length);
            Assert.AreEqual<string>("Abc", parser.TextParams[0]);
            Assert.AreEqual<string>("def", parser.TextParams[1]);

            Assert.IsTrue(parser.Parse("Abc -V:\"X X\" \"d ef\"", out errorText));
            Assert.IsFalse(parser.Flag);
            Assert.AreEqual<string>("X X", parser.OptionValue);
            Assert.AreEqual<int>(2, parser.TextParams.Length);
            Assert.AreEqual<string>("Abc", parser.TextParams[0]);
            Assert.AreEqual<string>("d ef", parser.TextParams[1]);

            // check terminator char
            Assert.IsTrue(parser.Parse("Abc -V:\"X X\" > \"d ef\"", out errorText));
            Assert.IsFalse(parser.Flag);
            Assert.AreEqual<string>("X X", parser.OptionValue);
            Assert.AreEqual<int>(1, parser.TextParams.Length);
            Assert.AreEqual<string>("Abc", parser.TextParams[0]);

            // Check Errors
            Assert.IsFalse(parser.Parse(Parser.InvalidTextParamValue, out errorText));
            Assert.AreEqual<string>(Parser.InvalidTextParamValueErrorText, errorText);

            Assert.IsFalse(parser.Parse("-X", out errorText));
            Assert.AreEqual<string>("Invalid Option Flag", errorText);

            Assert.IsFalse(parser.Parse("-Y", out errorText));
            Assert.AreEqual<string>("Unsupported Option", errorText);

            Assert.IsFalse(parser.Parse("\"A", out errorText));
            Assert.AreEqual<string>(CLParserError.IdToDefaultText(CLParserError.Id.TextMissingClosingQuoteCharacter), errorText);

            Assert.IsFalse(parser.Parse("\"A\"B", out errorText));
            Assert.AreEqual<string>(CLParserError.IdToDefaultText(CLParserError.Id.InvalidQuoteCharacterInTextParameter) + ": A", errorText);

            Assert.IsFalse(parser.Parse("-V:\"A", out errorText));
            Assert.AreEqual<string>(CLParserError.IdToDefaultText(CLParserError.Id.OptionMissingClosingQuoteCharacter) + ": V", errorText);

            Assert.IsFalse(parser.Parse("-V:\"A\"B", out errorText));
            Assert.AreEqual<string>(CLParserError.IdToDefaultText(CLParserError.Id.InvalidQuoteCharacterInOptionValue) + ": V", errorText);

            Assert.IsFalse(parser.Parse("-", out errorText));
            Assert.AreEqual<string>(CLParserError.IdToDefaultText(CLParserError.Id.OptionNotSpecifiedAtLinePosition) + ": 1", errorText);
        }
    }
}

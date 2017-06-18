// Project: Xilytix.CommandLineParserBase
// Licence: Public Domain
// Web Home Page: https://bitbucket.org/xilytix/commandlineparserbase/overview
// Initial Developer: Paul Klink (http://paul.klink.id.au)

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xilytix.CommandLineParserBase.UnitTest
{
    [TestClass]
    public class Test
    {
        private readonly char defaultOptionAnnouncer = CLParserBase.DefaultOptionParamAnnouncerChars[0];
        private readonly char defaultValueAnnouncer = CLParserBase.DefaultOptionParamValueAnnouncerChar;

        [TestMethod]
        public void TestStatic()
        {
            StaticTester.Test();
        }

        [TestMethod]
        public void TestParser()
        {
            TestParserConfig(defaultOptionAnnouncer, defaultValueAnnouncer);
            TestParserConfig(defaultOptionAnnouncer, ' '); // does not work
        }

        private void TestParserConfig(char optionAnnouncer, char valueAnnoucer)
        {
            string errorText;

            Parser parser = new Parser();
            char[] optionAnnouncers = new char[1];
            optionAnnouncers[0] = optionAnnouncer;
            parser.OptionParamAnnouncerChars = optionAnnouncers;
            parser.OptionParamValueAnnouncerChar = valueAnnoucer;

            Assert.IsTrue(parser.Parse("A", out errorText));
            Assert.AreEqual<int>(1, parser.TextParams.Length);
            Assert.AreEqual<string>("A", parser.TextParams[0]);

            Assert.IsTrue(parser.Parse($"{optionAnnouncer}F A", out errorText));
            Assert.IsTrue(parser.Flag);
            Assert.AreEqual<int>(1, parser.TextParams.Length);
            Assert.AreEqual<string>("A", parser.TextParams[0]);

            Assert.IsTrue(parser.Parse($"A {optionAnnouncer}Flag B", out errorText));
            Assert.IsTrue(parser.Flag);
            Assert.AreEqual<int>(2, parser.TextParams.Length);
            Assert.AreEqual<string>("A", parser.TextParams[0]);
            Assert.AreEqual<string>("B", parser.TextParams[1]);

            Assert.IsTrue(parser.Parse($"Abc {optionAnnouncer}V{valueAnnoucer}XX def", out errorText));
            Assert.IsFalse(parser.Flag);
            Assert.AreEqual<string>("XX", parser.OptionValue);
            Assert.AreEqual<int>(2, parser.TextParams.Length);
            Assert.AreEqual<string>("Abc", parser.TextParams[0]);
            Assert.AreEqual<string>("def", parser.TextParams[1]);

            Assert.IsTrue(parser.Parse($"Abc {optionAnnouncer}O {optionAnnouncer}F", out errorText));
            Assert.IsTrue(parser.Flag);
            Assert.AreEqual<int>(1, parser.TextParams.Length);
            Assert.AreEqual<string>("Abc", parser.TextParams[0]);
            Assert.IsNull(parser.OptionalOptionValue);

            Assert.IsTrue(parser.Parse($"Abc {optionAnnouncer}O {optionAnnouncer}F", out errorText));
            Assert.IsTrue(parser.Flag);
            Assert.AreEqual<int>(1, parser.TextParams.Length);
            Assert.AreEqual<string>("Abc", parser.TextParams[0]);
            Assert.IsNull(parser.OptionalOptionValue);

            Assert.IsTrue(parser.Parse($"Abc {optionAnnouncer}V{valueAnnoucer}\"X X\" \"d ef\"", out errorText));
            Assert.IsFalse(parser.Flag);
            Assert.AreEqual<string>("X X", parser.OptionValue);
            Assert.AreEqual<int>(2, parser.TextParams.Length);
            Assert.AreEqual<string>("Abc", parser.TextParams[0]);
            Assert.AreEqual<string>("d ef", parser.TextParams[1]);

            // check terminator char
            Assert.IsTrue(parser.Parse($"Abc {optionAnnouncer}V{valueAnnoucer}\"X X\" > \"d ef\"", out errorText));
            Assert.IsFalse(parser.Flag);
            Assert.AreEqual<string>("X X", parser.OptionValue);
            Assert.AreEqual<int>(1, parser.TextParams.Length);
            Assert.AreEqual<string>("Abc", parser.TextParams[0]);

            // Check Errors
            Assert.IsFalse(parser.Parse(Parser.InvalidTextParamValue, out errorText));
            Assert.AreEqual<string>(Parser.InvalidTextParamValueErrorText, errorText);

            Assert.IsFalse(parser.Parse($"{optionAnnouncer}X", out errorText));
            Assert.AreEqual<string>("Invalid Option Flag", errorText);

            Assert.IsFalse(parser.Parse($"{optionAnnouncer}Y", out errorText));
            Assert.AreEqual<string>("Unsupported Option", errorText);

            Assert.IsFalse(parser.Parse("\"A", out errorText));
            Assert.AreEqual<string>(CLParserError.IdToDefaultText(CLParserError.Id.TextMissingClosingQuoteCharacter), errorText);

            Assert.IsFalse(parser.Parse("\"A\"B", out errorText));
            Assert.AreEqual<string>(CLParserError.IdToDefaultText(CLParserError.Id.InvalidQuoteCharacterInTextParameter) + ": A", errorText);

            Assert.IsFalse(parser.Parse($"{optionAnnouncer}V{valueAnnoucer}\"A", out errorText));
            Assert.AreEqual<string>(CLParserError.IdToDefaultText(CLParserError.Id.OptionMissingClosingQuoteCharacter) + ": V", errorText);

            Assert.IsFalse(parser.Parse($"{optionAnnouncer}V{valueAnnoucer}\"A\"B", out errorText));
            Assert.AreEqual<string>(CLParserError.IdToDefaultText(CLParserError.Id.InvalidQuoteCharacterInOptionValue) + ": V", errorText);

            Assert.IsFalse(parser.Parse($"{optionAnnouncer}", out errorText));
            Assert.AreEqual<string>(CLParserError.IdToDefaultText(CLParserError.Id.OptionNotSpecifiedAtLinePosition) + ": 1", errorText);
        }
    }
}

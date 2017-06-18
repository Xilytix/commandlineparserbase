// Project: Xilytix.CommandLineParserBase
// Licence: Public Domain
// Web Home Page: https://bitbucket.org/xilytix/commandlineparserbase/overview
// Initial Developer: Paul Klink (http://paul.klink.id.au)

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xilytix.CommandLineParserBase.UnitTest
{
    internal class Parser : CLParserBase
    {
        public const string OptionFlagName = "F";
        public const string OptionFlagLongName = "Flag";

        public const string InvalidOptionFlagName = "X";
        public const string InvalidOptionFlagLongName = "XInvalid";

        public const string ValuedOptionName = "V";
        public const string ValuedOptionLongName = "ValuedOption";

        public const string OptionalValuedOptionName = "O";

        public const string InvalidTextParamValue = "InvalidTextParamValue";
        public const string InvalidTextParamValueErrorText = "Invalid Text Parameter Value";

        public int ParseLineLength;
        public bool Flag;
        public string OptionValue;
        public string OptionalOptionValue;

        public string[] TextParams;

        public override bool Parse(string line, out string errorText)
        {
            ParseLineLength = line.Length;
            Flag = false;
            OptionValue = null;
            TextParams = new string[0];

            return base.Parse(line, out errorText);
        }

        protected override bool ProcessTextParam(int textParamIdx, string value, int linePosIdx, out string errorText)
        {
            Assert.AreEqual<int>(TextParams.Length, textParamIdx, "Unexpected textParamIdx count: " + textParamIdx.ToString());
            Assert.IsNotNull(value, "Null TextParam value");

            if (linePosIdx < 0 || linePosIdx >= ParseLineLength)
            {
                Assert.Fail("Invalid TextParam linePosIdx" + linePosIdx.ToString());
                errorText = "Fail";
                return false;
            }
            else
            {
                if (value == InvalidTextParamValue)
                {
                    errorText = InvalidTextParamValueErrorText;
                    return false;
                }
                else
                {
                    int idx = TextParams.Length;
                    Array.Resize<string>(ref TextParams, TextParams.Length + 1);
                    TextParams[idx] = value;
                    errorText = null;
                    return true;
                }
            }
        }

        protected override bool ProcessOption(string name, string value, out string errorText)
        {
            if (name == OptionFlagName || name == OptionFlagLongName)
            {
                Flag = true;
                Assert.IsNull(value, "OptionFlag received value: " + value);
                errorText = null;
                return true;
            }
            else
            {
                if (name == InvalidOptionFlagName || name == InvalidOptionFlagLongName)
                {
                    Assert.IsNull(value, "InvalidOptionFlag received value: " + value);
                    errorText = "Invalid Option Flag";
                    return false;
                }
                else
                {
                    if (name == ValuedOptionName || name == ValuedOptionLongName)
                    {
                        Assert.IsNotNull(value, "ValuedOption received null value");
                        OptionValue = value;
                        errorText = null;
                        return true;
                    }
                    else
                    {
                        if (name == OptionalValuedOptionName)
                        {
                            OptionalOptionValue = value;
                            errorText = null;
                            return true;
                        }
                        else
                        {
                            errorText = "Unsupported Option";
                            return false;
                        }
                    }
                }
            }
        }

        protected override bool CanOptionHaveValue(string name)
        {
            return name == ValuedOptionName || name == ValuedOptionLongName || name == OptionalValuedOptionName;
        }
    }
}

using System.Globalization;
using NUnit.Framework;

namespace MiscTests.StringParsing
{
    [TestFixture]
    // ReSharper disable once InconsistentNaming
    public class ConvertFloatingPointNumbers_ToString
    {
        [Test]
        public void VerifyInvariantCulture()
        {
            Assert.AreEqual(".", NumberFormatInfo.InvariantInfo.NumberDecimalSeparator);
            Assert.AreEqual(",", NumberFormatInfo.InvariantInfo.NumberGroupSeparator);
            Assert.AreEqual(2, NumberFormatInfo.InvariantInfo.NumberDecimalDigits);
            Assert.AreEqual(1, NumberFormatInfo.InvariantInfo.NumberGroupSizes.Length);
            Assert.AreEqual(3, NumberFormatInfo.InvariantInfo.NumberGroupSizes[0]);

            CultureInfo.CurrentCulture = new CultureInfo("de-DE"); //set current thread to something not compatible with invariant

            Assert.AreEqual("1,222,000.50", 1222000.5M.ToString("N", CultureInfo.InvariantCulture));
            Assert.AreEqual("1222000.5", 1222000.5M.ToString(CultureInfo.InvariantCulture));
        }

        [Test]
        public void VerifyCultureWithCommaAsDecimalSeparator()
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            Assert.AreEqual(",", NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);
            Assert.AreEqual(".", NumberFormatInfo.CurrentInfo.NumberGroupSeparator);
            Assert.AreEqual(2, NumberFormatInfo.CurrentInfo.NumberDecimalDigits);
            Assert.AreEqual(1, NumberFormatInfo.CurrentInfo.NumberGroupSizes.Length);
            Assert.AreEqual(3, NumberFormatInfo.CurrentInfo.NumberGroupSizes[0]);

            Assert.AreEqual("1.222.000,50", 1222000.5M.ToString("N", CultureInfo.CurrentCulture));
            Assert.AreEqual("1222000,5", 1222000.5M.ToString(CultureInfo.CurrentCulture));
            Assert.AreEqual("1222000,5", $"{1222000.5M}");
        }

        [Test]
        public void VerifyCultureWithDotAsDecimalSeparator()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");

            Assert.AreEqual(".", NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);
            Assert.AreEqual(",", NumberFormatInfo.CurrentInfo.NumberGroupSeparator);
            Assert.AreEqual(2, NumberFormatInfo.CurrentInfo.NumberDecimalDigits);
            Assert.AreEqual(1, NumberFormatInfo.CurrentInfo.NumberGroupSizes.Length);
            Assert.AreEqual(3, NumberFormatInfo.CurrentInfo.NumberGroupSizes[0]);

            Assert.AreEqual("1,222,000.50", 1222000.5M.ToString("N", CultureInfo.CurrentCulture));
            Assert.AreEqual("1222000.5", 1222000.5M.ToString(CultureInfo.CurrentCulture));
            Assert.AreEqual("1222000.5", $"{1222000.5M}");
        }

        [Test]
        public void VerifyCultureWithCustomDecimalSeparator()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentCulture.NumberFormat.NumberGroupSizes = new []{2,3};
            CultureInfo.CurrentCulture.NumberFormat.NumberDecimalDigits = 4;
            CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator= "!";
            CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator = "-";
            CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator = "-";

            Assert.AreEqual("!", NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);
            Assert.AreEqual("-", NumberFormatInfo.CurrentInfo.NumberGroupSeparator);
            Assert.AreEqual(4, NumberFormatInfo.CurrentInfo.NumberDecimalDigits);
            Assert.AreEqual(2, NumberFormatInfo.CurrentInfo.NumberGroupSizes.Length);
            Assert.AreEqual(2, NumberFormatInfo.CurrentInfo.NumberGroupSizes[0]);
            Assert.AreEqual(3, NumberFormatInfo.CurrentInfo.NumberGroupSizes[1]);

            Assert.AreEqual("12-220-00!5000", 1222000.5M.ToString("N", CultureInfo.CurrentCulture));
            Assert.AreEqual("1222000!5", 1222000.5M.ToString(CultureInfo.CurrentCulture));
            Assert.AreEqual("1222000!5", $"{1222000.5M}");
        }

    }
}

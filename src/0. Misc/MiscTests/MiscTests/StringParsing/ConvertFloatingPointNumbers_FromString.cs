using System;
using System.Globalization;
using NUnit.Framework;

namespace MiscTests.StringParsing
{
    [TestFixture]
    // ReSharper disable once InconsistentNaming
    public class ConvertFloatingPointNumbers_FromString
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

            Assert.AreEqual(1222000.5M, decimal.Parse("1,222,000.50", CultureInfo.InvariantCulture));
            Assert.AreEqual(1222000.5M, Convert.ToDouble("1,222,000.50", CultureInfo.InvariantCulture));
            Assert.AreEqual( 1222000.5M, decimal.Parse("1222000.50", CultureInfo.InvariantCulture));
            Assert.AreEqual( 1222000.5M, Convert.ToDouble("1222000.50", CultureInfo.InvariantCulture));

            Assert.AreEqual(122, decimal.Parse("1,22", CultureInfo.InvariantCulture));
            Assert.AreEqual(122, Convert.ToDouble("1,22", CultureInfo.InvariantCulture));

            Assert.AreEqual(1.22, decimal.Parse("1.22", CultureInfo.InvariantCulture));
            Assert.AreEqual(1.22, Convert.ToDouble("1.22", CultureInfo.InvariantCulture));

            Assert.That(() => decimal.Parse("1.222.333,44", CultureInfo.InvariantCulture), Throws.Exception.TypeOf<FormatException>());
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


            Assert.AreEqual(1222000.5M, decimal.Parse("1.222.000,50", CultureInfo.CurrentCulture));
            Assert.AreEqual(1222000.5M, decimal.Parse("1222000,50", CultureInfo.CurrentCulture));

            Assert.AreEqual(122, double.Parse("1.22", CultureInfo.CurrentCulture));
            Assert.AreEqual(122, Convert.ToDouble("1.22", CultureInfo.CurrentCulture));

            Assert.AreEqual(1.22, double.Parse("1,22", CultureInfo.CurrentCulture));
            Assert.AreEqual(1.22, Convert.ToDouble("1,22", CultureInfo.CurrentCulture));

            Assert.That(() => decimal.Parse("1,222,333.44"), Throws.Exception.TypeOf<FormatException>());


            
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

            Assert.AreEqual(1222000.5M, decimal.Parse("1,222,000.50", CultureInfo.CurrentCulture));
            Assert.AreEqual(1222000.5M, decimal.Parse("1222000.50", CultureInfo.CurrentCulture));

            Assert.AreEqual(122, decimal.Parse("1,22", CultureInfo.CurrentCulture));
            Assert.AreEqual(122, Convert.ToDouble("1,22", CultureInfo.CurrentCulture));

            Assert.AreEqual(1.22, decimal.Parse("1.22", CultureInfo.CurrentCulture));
            Assert.AreEqual(1.22, Convert.ToDouble("1.22", CultureInfo.CurrentCulture));

            Assert.That(() => decimal.Parse("1.222.333,44"), Throws.Exception.TypeOf<FormatException>());
        }

        [Test]
        public void VerifyCultureWithCustomDecimalSeparator()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentCulture.NumberFormat.NumberGroupSizes = new[] { 2, 3 };
            CultureInfo.CurrentCulture.NumberFormat.NumberDecimalDigits = 4;
            CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator = "!";
            CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator = "-";

            Assert.AreEqual("!", NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);
            Assert.AreEqual("-", NumberFormatInfo.CurrentInfo.NumberGroupSeparator);
            Assert.AreEqual(4, NumberFormatInfo.CurrentInfo.NumberDecimalDigits);
            Assert.AreEqual(2, NumberFormatInfo.CurrentInfo.NumberGroupSizes.Length);
            Assert.AreEqual(2, NumberFormatInfo.CurrentInfo.NumberGroupSizes[0]);
            Assert.AreEqual(3, NumberFormatInfo.CurrentInfo.NumberGroupSizes[1]);

            Assert.AreEqual(1222000.5M, decimal.Parse("12-220-00!5000", CultureInfo.CurrentCulture));
            Assert.AreEqual(1222000.5M, decimal.Parse("1222000!5", CultureInfo.CurrentCulture));

        }

    }
}
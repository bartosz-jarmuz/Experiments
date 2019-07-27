using System.IO;
using NUnit.Framework;

namespace MiscTests.Disposing
{
    [TestFixture]
    public class NotDisposingStreamReadersTest
    {
        public class BadStreamConsumer
        {
            public string GetFirstLine(FileInfo file)
            {
                var reader = new StreamReader(new FileStream(file.FullName, FileMode.Open));
                //not disposing reader and stream...
                return reader.ReadLine();
            }
        }

        public class GoodStreamConsumer
        {
            public string GetFirstLine(FileInfo file)
            {
                using (var reader = new StreamReader(new FileStream(file.FullName, FileMode.Open)))
                {
                    //disposing reader and stream by using clause...
                    return reader.ReadLine();
                }
            }
        }

        public class SomeWrapperClass
        {
            public SomeWrapperClass()
            {
                //this class simply locks a file in a non-obvious and incorrect way
                var file = new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, @"Disposing\Resources\TextFile3.txt"));
                var badConsumer = new BadStreamConsumer();
                Assert.IsNotNull(badConsumer.GetFirstLine(file));
            }
        }

        public NotDisposingStreamReadersTest()
        {
            //this call simply invokes the constructor which locks one of the files
            // ReSharper disable once ObjectCreationAsStatement
            new SomeWrapperClass();
        }

        [Test]
        public void TestProperlyDisposedReader()
        {
            var file = new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, @"Disposing\Resources\TextFile1.txt"));
            var goodConsumer = new GoodStreamConsumer();
            //so, it works first time it's used
            Assert.IsNotNull(goodConsumer.GetFirstLine(file));
            //and again...
            Assert.IsNotNull(goodConsumer.GetFirstLine(file));
            //and even if you create a new instance of the consumer (duh!)
            Assert.IsNotNull(new GoodStreamConsumer().GetFirstLine(file));
        }

        [Test]
        public void TestNotDisposedReader()
        {
            var file = new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, @"Disposing\Resources\TextFile2.txt"));
            var badConsumer = new BadStreamConsumer();
            //so, it works first time it's used
            Assert.IsNotNull(badConsumer.GetFirstLine(file));

            //but then on a second access to that file it fails
            Assert.That(() => badConsumer.GetFirstLine(file), 
                Throws.Exception.TypeOf(typeof(IOException))
                    .With.Message.Contain("The process cannot access the file")
                    .And.With.Message.Contain(" because it is being used by another process"));

            //even if you create a new instance of the consumer (duh!)
            Assert.That(() => new BadStreamConsumer().GetFirstLine(file),
                Throws.Exception.TypeOf(typeof(IOException))
                    .With.Message.Contain("The process cannot access the file")
                    .And.With.Message.Contain(" because it is being used by another process"));

            //and even if you use the proper consumer (duh x2!)
            Assert.That(() => new GoodStreamConsumer().GetFirstLine(file),
                Throws.Exception.TypeOf(typeof(IOException))
                    .With.Message.Contain("The process cannot access the file")
                    .And.With.Message.Contain(" because it is being used by another process"));
        }

        [Test]
        public void TestFileAccessedInDifferentScope()
        {
            //this file was accessed in the constructor of a different class, called by a constructor - that's not visible, but still the file is not available
            FileInfo file = new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, @"Disposing\Resources\TextFile3.txt"));
            GoodStreamConsumer goodConsumer = new GoodStreamConsumer();
            //even a proper reader won't handle it
            Assert.That(() => goodConsumer.GetFirstLine(file),
                Throws.Exception.TypeOf(typeof(IOException))
                    .With.Message.Contain("The process cannot access the file")
                    .And.With.Message.Contain(" because it is being used by another process"));
        }

    }
}

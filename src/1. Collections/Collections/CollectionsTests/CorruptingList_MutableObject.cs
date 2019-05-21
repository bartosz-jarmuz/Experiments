using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming

namespace CollectionsTests
{
    [TestClass]
    public class CorruptingList_MutableObject
    {
        public class Extension
        {
            public Extension(string value)
            {
                Value = value;
            }

            public string Value { get; set; }
        }

        public class FileType
        {
            private readonly List<Extension> extensions = new List<Extension>();

            public FileType(params Extension[] extensions)
            {
                this.extensions.AddRange(extensions);
            }

            public IReadOnlyCollection<Extension> Extensions_IReadOnlyCollection_RealImplementation => extensions.AsReadOnly();
            public IEnumerable<Extension> Extensions_IEnumerable_Copy => new List<Extension>(extensions);

        }

        [TestMethod]
        public void ChangeListItems_IEnumerable_CorruptionFailed()
        {
            FileType picture = new FileType(new Extension(".bmp"), new Extension(".jpg"));

            Extension bmp = picture.Extensions_IEnumerable_Copy.First(x => x.Value == ".bmp");
            //we get the reference to one extensions and change it...
            bmp.Value = ".exe";

            //...and it's changed in the object, we have an invalid state
            Assert.AreEqual(1, picture.Extensions_IEnumerable_Copy.Count(x=>x.Value == ".exe"));
            Assert.AreEqual(0, picture.Extensions_IEnumerable_Copy.Count(x=>x.Value == ".bmp"));
            
        }

        [TestMethod]
        public void ChangeListItems_IReadOnlyCollection_CorruptionFailed()
        {
            FileType picture = new FileType(new Extension(".bmp"), new Extension(".jpg"));

            Extension bmp = picture.Extensions_IReadOnlyCollection_RealImplementation.First(x => x.Value == ".bmp");
            //we get the reference to one extensions and change it...
            //the 'Read only' does not protect from it
            bmp.Value = ".exe";

            //...and it's changed in the object, we have an invalid state
            Assert.AreEqual(1, picture.Extensions_IReadOnlyCollection_RealImplementation.Count(x => x.Value == ".exe"));
            Assert.AreEqual(0, picture.Extensions_IReadOnlyCollection_RealImplementation.Count(x => x.Value == ".bmp"));
        }

    }
}

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LuxExport.Logic;

namespace LuxExport.Tests
{
    [TestClass]
    public class TagResolverManagerTests
    {
        private TagResolverManager _manager;

        [TestInitialize]
        public void Setup()
        {
            _manager = new TagResolverManager();
        }

        [TestMethod]
        public void ResolveAll_NameTag_ShouldReturnFileNameWithoutExtension()
        {
            var result = _manager.ResolveAll("{name}", "photo.jpg", new Dictionary<string, string>(), 0);
            Assert.AreEqual("photo", result);
        }

        [TestMethod]
        public void ResolveAll_DateTag_ShouldReturnCurrentDate()
        {
            var result = _manager.ResolveAll("{date}", "any.jpg", new Dictionary<string, string>(), 0);
            StringAssert.Matches(result, new System.Text.RegularExpressions.Regex(@"\d{4}-\d{2}-\d{2}"));
        }

        [TestMethod]
        public void ResolveAll_CounterTag_ShouldReturnCounterValue()
        {
            var result = _manager.ResolveAll("{counter}", "img.png", new Dictionary<string, string>(), 42);
            Assert.AreEqual("42", result);
        }

        [TestMethod]
        public void ResolveAll_MetaTag_WithExistingKey_ShouldReturnMetaValue()
        {
            var metadata = new Dictionary<string, string> { { "Model", "Canon R5" } };
            var result = _manager.ResolveAll("{meta:Model}", "test.jpg", metadata, 0);
            Assert.AreEqual("Canon R5", result);
        }

        [TestMethod]
        public void ResolveAll_MetaTag_WithMissingKey_ShouldReturnUnknown()
        {
            var metadata = new Dictionary<string, string>();
            var result = _manager.ResolveAll("{meta:ISO}", "test.jpg", metadata, 0);
            Assert.AreEqual("unknown", result);
        }

        [TestMethod]
        public void ResolveAll_UnknownTag_ShouldReturnTagAsIs()
        {
            var result = _manager.ResolveAll("{notatag}", "file.jpg", new Dictionary<string, string>(), 0);
            Assert.AreEqual("{notatag}", result);
        }
    }
}

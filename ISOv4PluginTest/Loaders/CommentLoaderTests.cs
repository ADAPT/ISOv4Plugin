using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using NUnit.Framework;

namespace ISOv4PluginTest.Loaders
{
    [TestFixture]
    public class CommentLoaderTests
    {
        private string _directory;

        [SetUp]
        public void Setup()
        {
            _directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_directory);
        }

        [Test]
        public void LoadCommentTest()
        {
            // Setup
            var taskDocument = new  TaskDataDocument();
            var path = Path.Combine(_directory, "comment1.xml");
            File.WriteAllText(path, TestData.TestData.Comment1);

            // Act
            var result = taskDocument.LoadFromFile(path);

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.Comments);
            Assert.AreEqual(1, taskDocument.Comments.Count);
            var comment = taskDocument.Comments.First();
            Assert.AreEqual("CCT1", comment.Key);
            Assert.AreEqual("Comment 1", comment.Value.Comment.Description);
            Assert.AreEqual(2, comment.Value.Scope);
            Assert.AreEqual(2, comment.Value.Comment.EnumeratedMembers.Count);

            Assert.AreEqual("Comment value 1", comment.Value.Comment.EnumeratedMembers[0].Value);
            Assert.AreEqual("Comment value 2", comment.Value.Comment.EnumeratedMembers[1].Value);

            Assert.AreEqual(2, comment.Value.Values.Count);
            Assert.AreEqual("Comment value 1", comment.Value.Values["CCL1"].Value);
            Assert.AreEqual("Comment value 2", comment.Value.Values["CCL2"].Value);
        }

        [Test]
        public void CommentInExternalFileTest()
        {
            // Setup
            var taskDocument = new TaskDataDocument();
            var path = Path.Combine(_directory, "comment2.xml");
            File.WriteAllText(path, TestData.TestData.Comment2);
            File.WriteAllText(Path.Combine(_directory, "CCT00002.xml"), TestData.TestData.CCT00002);

            // Act
            var result = taskDocument.LoadFromFile(path);

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.Comments);
            Assert.AreEqual(2, taskDocument.Comments.Count);

            var comment = taskDocument.Comments["CCT1"];

            Assert.AreEqual("Comment 1", comment.Comment.Description);
            Assert.AreEqual(2, comment.Scope);
            Assert.AreEqual(2, comment.Comment.EnumeratedMembers.Count);

            Assert.AreEqual("Comment value 1", comment.Comment.EnumeratedMembers[0].Value);
            Assert.AreEqual("Comment value 2", comment.Comment.EnumeratedMembers[1].Value);

            Assert.AreEqual(2, comment.Values.Count);
            Assert.AreEqual("Comment value 1", comment.Values["CCL1"].Value);
            Assert.AreEqual("Comment value 2", comment.Values["CCL2"].Value);

            comment = taskDocument.Comments["CCT2"];

            Assert.AreEqual("Comment 2", comment.Comment.Description);
            Assert.AreEqual(2, comment.Scope);
            Assert.AreEqual(2, comment.Comment.EnumeratedMembers.Count);

            Assert.AreEqual("Comment value 3", comment.Comment.EnumeratedMembers[0].Value);
            Assert.AreEqual("Comment value 4", comment.Comment.EnumeratedMembers[1].Value);

            Assert.AreEqual(2, comment.Values.Count);
            Assert.AreEqual("Comment value 3", comment.Values["CCL3"].Value);
            Assert.AreEqual("Comment value 4", comment.Values["CCL4"].Value);
        }

        [Test]
        public void CustomerWithMissingRequiredInfoTest()
        {
            var comments = new List<String>
            {
                TestData.TestData.Comment3,
                TestData.TestData.Comment4,
                TestData.TestData.Comment5,
                TestData.TestData.Comment7,
                TestData.TestData.Comment8,
                TestData.TestData.Comment10
            };

            for (int i = 0; i < comments.Count; i++)
            {
                // Setup
                var taskDocument = new TaskDataDocument();
                var path = Path.Combine(_directory, String.Format("comment{0}.xml", i));
                File.WriteAllText(path, comments[i]);

                // Act
                var result = taskDocument.LoadFromFile(path);

                // Verify
                Assert.IsTrue(result);
                Assert.IsNotNull(taskDocument.Comments);
                Assert.AreEqual(0, taskDocument.Comments.Count);
            }
        }

        [Test]
        public void CommentWithNoValuesTest()
        {
            // Setup
            var taskDocument = new TaskDataDocument();
            var path = Path.Combine(_directory, "comment6.xml");
            File.WriteAllText(path, TestData.TestData.Comment6);

            // Act
            var result = taskDocument.LoadFromFile(path);

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.Comments);
            Assert.AreEqual(1, taskDocument.Comments.Count);

            var comment = taskDocument.Comments["CCT1"];

            Assert.AreEqual("Comment 1", comment.Comment.Description);
            Assert.AreEqual(2, comment.Scope);
            Assert.IsNull(comment.Comment.EnumeratedMembers);
            Assert.IsNull(comment.Values);
        }

        [Test]
        public void CommentWithInvalidValuesTest()
        {
            // Setup
            var taskDocument = new TaskDataDocument();
            var path = Path.Combine(_directory, "comment6.xml");
            File.WriteAllText(path, TestData.TestData.Comment9);

            // Act
            var result = taskDocument.LoadFromFile(path);

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.Comments);
            Assert.AreEqual(1, taskDocument.Comments.Count);

            var comment = taskDocument.Comments["CCT1"];

            Assert.AreEqual("Comment 1", comment.Comment.Description);
            Assert.AreEqual(2, comment.Scope);
            Assert.IsNull(comment.Comment.EnumeratedMembers);
            Assert.IsNull(comment.Values);
        }
    }
}

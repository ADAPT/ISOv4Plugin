using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.Plugins;
using NUnit.Framework;
using System.Linq;

namespace IsoPluginTest
{
    [TestFixture]
    public class CommentLoaderTests
    {
        [Test]
        public void LoadCommentTest()
        {
            // Setup
            var taskDocument = new  TaskDataDocument();

            // Act
            var result = taskDocument.LoadFromFile(@"TestData\Comment\Comment1.xml");

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

            // Act
            var result = taskDocument.LoadFromFile(@"TestData\Comment\Comment2.xml");

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

        [TestCase(@"TestData\Comment\Comment3.xml")]
        [TestCase(@"TestData\Comment\Comment4.xml")]
        [TestCase(@"TestData\Comment\Comment5.xml")]
        [TestCase(@"TestData\Comment\Comment7.xml")]
        [TestCase(@"TestData\Comment\Comment8.xml")]
        [TestCase(@"TestData\Comment\Comment10.xml")]
        public void CustomerWithMissingRequiredInfoTest(string testFileName)
        {
            // Setup
            var taskDocument = new TaskDataDocument();

            // Act
            var result = taskDocument.LoadFromFile(testFileName);

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.Comments);
            Assert.AreEqual(0, taskDocument.Comments.Count);
        }

        [Test]
        public void CommentWithNoValuesTest()
        {
            // Setup
            var taskDocument = new TaskDataDocument();

            // Act
            var result = taskDocument.LoadFromFile(@"TestData\Comment\Comment6.xml");

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

            // Act
            var result = taskDocument.LoadFromFile(@"TestData\Comment\Comment9.xml");

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

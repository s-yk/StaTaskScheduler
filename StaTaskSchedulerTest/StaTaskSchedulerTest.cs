using System.IO;
using System.IO.Packaging;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Xps.Packaging;
using NUnit.Framework;

namespace StaTaskSchedulerTest
{
    [TestFixture]
    public class StaTaskSchedulerTest
    {
        private const string TestDataPath = @"TestData\XpsSample.xps";

        [Test]
        public void StaTaskSchedulerの使用例()
        {
            var loadDocumentTask = Task.Factory.StartNew(LoadDocument,
                CancellationToken.None,
                TaskCreationOptions.None,
                new StaTaskScheduler.StaTaskScheduler());

            Assert.That(() => loadDocumentTask.Wait(), Throws.Nothing);
            Assert.That(loadDocumentTask.IsCompleted, Is.True);
            Assert.That(loadDocumentTask.Result, Is.Not.Null);
            Assert.That(loadDocumentTask.Result.DocumentPaginator.PageCount, Is.EqualTo(1));
        }

        [Test]
        public void DefaultTaskSchedulerの例()
        {
            var loadDocumentTask = Task.Factory.StartNew(LoadDocument,
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskScheduler.Default);

            Assert.That(() => loadDocumentTask.Wait(), Throws.Exception);
            Assert.That(loadDocumentTask.IsFaulted, Is.True);
            Assert.That(loadDocumentTask.Exception.InnerException, Is.TypeOf<System.Windows.Markup.XamlParseException>());
        }

        /// <summary>
        /// XPSの読み込み
        /// 要STA実行
        /// </summary>
        /// <returns></returns>
        private IDocumentPaginatorSource LoadDocument()
        {
            var path = Path.Combine(new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName, TestDataPath);
            IDocumentPaginatorSource source;
            using (var xpsDocument = new XpsDocument(path, FileAccess.Read, CompressionOption.NotCompressed))
            {
                source = xpsDocument.GetFixedDocumentSequence() as IDocumentPaginatorSource;
            }

            return source;
        }
    }
}

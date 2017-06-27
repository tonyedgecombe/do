using System.Threading;
using System.Windows.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace testdo
{
    [TestClass]
    public class TestDispatcher
    {
        [TestMethod]
        public void CurrentDispatcher()
        {
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

            Assert.IsNotNull(dispatcher);
        }

        [TestMethod]
        public void CurrentIsSameAsThread()
        {
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
            Dispatcher threadDispatcher = Dispatcher.FromThread(Thread.CurrentThread);

            Assert.AreSame(dispatcher, threadDispatcher);
        }
    }
}
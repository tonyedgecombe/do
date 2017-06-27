using System.Threading;
using System.Threading.Tasks;
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

        [TestMethod]
        public void CurrentIsDifferentToOtherThread()
        {
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
            Dispatcher threadDispatcher = CreateDispatcherOnOtherThread();

            Assert.IsNotNull(dispatcher);
            Assert.IsNotNull(threadDispatcher);
            Assert.AreNotSame(dispatcher, threadDispatcher);
        }

        private static Dispatcher CreateDispatcherOnOtherThread()
        {
            Dispatcher dispatcher = null;

            Task.Run(() =>
            {
                dispatcher = Dispatcher.CurrentDispatcher;
            }).Wait();

            return dispatcher;
        }
    }
}
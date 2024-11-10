using System.Threading;

namespace AppDrawers
{
    public class StaThread
    {
        private Thread thread;

        public StaThread(ThreadStart start)
        {
            this.thread = new Thread(start);

            this.thread.SetApartmentState(ApartmentState.STA);
        }

        public Thread Thread => this.thread;

        public static StaThread Start(ThreadStart start)
        {
            var sta = new StaThread(start);

            sta.Start();

            return sta;
        }

        public void Start()
        {
            this.thread.Start();
        }
    }
}
namespace ServerCore
{
    internal class MonitorExample
    {
        static int number = 0;
        static object _obj = new object();

        static void Thread_1()
        {
            for (int i = 0; i < 100000; i++)
            {
                // 상호배제(Mutual Exclusive)
                lock (_obj)
                {
                    number++;
                }

                //try
                //{
                //    Monitor.Enter(_obj);    // Lock
                //    number++;
                //} finally
                //{
                //    Monitor.Exit(_obj);     // Unlock
                //}
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 100000; i++)
            {
                Monitor.Enter(_obj);
                number--;
                Monitor.Exit(_obj);
            }
        }

        static void Main(string[] args)
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);
            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine(number);
        }
    }
}
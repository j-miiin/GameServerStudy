﻿namespace ServerCore
{
    class ReadWriteLockExample
    {
        static volatile int count = 0;
        static Lock _lock = new Lock();

        static void Main(string[] args)
        {
            Task t1 = new Task(delegate ()
            {
                for (int i = 0; i < 100000; i++)
                {
                    _lock.ReadLock();
                    count++;
                    _lock.ReadUnlock();
                }
            });

            Task t2 = new Task(delegate ()
            {
                for (int i = 0; i < 100000; i++)
                {
                    _lock.ReadLock();
                    count--;
                    _lock.ReadUnlock();
                }
            });

            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine(count);
        }
    }
}
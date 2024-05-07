namespace ServerCore
{
    class ManualResetEventLock
    {
        // 커널 모드에서 bool 값과 같다고 생각하면 됨
        // true면 문이 열린 상태, false면 닫힌 상태
        ManualResetEvent _available = new ManualResetEvent(true);

        public void Acquire()
        {
            // 두 동작이 나누어져 있으므로 문제가 발생할 수 있음
            _available.WaitOne();   // 입장 시도
            _available.Reset();     // 문을 닫음
        }

        public void Release()
        {
            _available.Set();
        }
    }

    class ManualResetEventExample
    {
        static int _num = 0;
        static ManualResetEventLock _lock = new ManualResetEventLock();

        static void Thread_1()
        {
            for (int i = 0; i < 10000; i++)
            {
                _lock.Acquire();
                _num++;
                _lock.Release();
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 10000; i++)
            {
                _lock.Acquire();
                _num--;
                _lock.Release();
            }
        }

        static void Main(string[] args)
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);

            t1.Start();
            t2.Start();

            Console.WriteLine(_num);
        }
    }
}
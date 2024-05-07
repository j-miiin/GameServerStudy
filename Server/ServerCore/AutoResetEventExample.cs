namespace ServerCore
{
    class AutoResetEventLock
    {
        // 커널 모드에서 bool 값과 같다고 생각하면 됨
        // true면 문이 열린 상태, false면 닫힌 상태
        AutoResetEvent _available = new AutoResetEvent(true);

        public void Acquire()
        {
            _available.WaitOne();   // 입장 시도 -> 입장하면 자동으로 문을 닫아줌
            //_available.Reset();   // flag = false와 같음 -> 위 동작에 포함되어 있음
        }

        public void Release()
        {
            _available.Set();
        }
    }

    class AutoResetEventExample
    {
        static int _num = 0;
        static AutoResetEventLock _lock = new AutoResetEventLock();

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
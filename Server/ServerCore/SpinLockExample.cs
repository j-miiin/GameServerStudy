namespace ServerCore
{
    class SpinLock
    {
        volatile int _locked = 0;

        public void Acquire()
        {
            // 원자성 X
            //while (_locked)
            //{
            //    // 잠금이 풀리기를 기다림
            //}

            //// 내 작업
            //_locked = true;

            // 원자성 O - Exchange 사용
            //while (true)
            //{
            //    int original = Interlocked.Exchange(ref _locked, 1);
            //    if (original == 0)
            //        break;
            //}

            // CAS (Compare-And-Swap) 계열 함수
            while (true)
            {
                int expected = 0;
                int desired = 1;
                if (Interlocked.CompareExchange(ref _locked, desired, expected) == expected)
                    break;


                // Thread.Sleep(1);
                // Thread.Sleep(0);
                Thread.Yield();
            }
        }

        public void Release()
        {
            _locked = 0;
        }
    }

    class SpinLockExample
    {
        static int _num = 0;
        static SpinLock _lock = new SpinLock();

        static void Thread_1()
        {
            for (int i = 0; i < 1000000; i++)
            {
                _lock.Acquire();
                _num++;
                _lock.Release();
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 1000000; i++)
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
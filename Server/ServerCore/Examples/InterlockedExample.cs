namespace ServerCore
{
    internal class InterlockedExample
    {
        static int number = 0;

        static void Thread_1()
        {
            for (int i = 0; i < 100000; i++)
            {
                //number++;
                Interlocked.Increment(ref number);
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 100000; i++)
            {
                //number--;
                Interlocked.Decrement(ref number);
            }
        }

        static void Main(string[] args)
        {
            //number++;
            // 어셈블리에서는 위 식이 아래와 같은 순서로 동작함
            //int temp = number;
            //temp += 1;
            //number = temp;

            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);
            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine(number);
        }
    }
}
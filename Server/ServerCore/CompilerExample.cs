namespace ServerCore
{
    internal class CompilerExample
    {
        volatile static bool _stop = false; // volatile 키워드를 통해 최적화 하지 않도록 강제

        static void ThreadMain()
        {
            Console.WriteLine("쓰레드 시작!");

            while (!_stop)
            {

            }

            Console.WriteLine("쓰레드 종료!");
        }

        static void Main(string[] args)
        {
            Task t = new Task(ThreadMain);
            t.Start();

            Thread.Sleep(1000);

            _stop = true;

            Console.WriteLine("Stop 호출");
            Console.WriteLine("종료 대기중");
            t.Wait();   // Thread.Join()과 같음
            Console.WriteLine("종료 성공");
        }
    }
}
namespace ServerCore
{
    // 메모리 배리어
    // 1. 코드 재배치 억제
    // 2. 가시성

    internal class MemoryBarrierExample2
    {
        int _answer;
        bool _complete;

        void A()
        {
            // Store가 2번이므로 Barrier를 2개 사용
            // Barrier 1은 _answer에 대한 가시성만 보장
            _answer = 123;
            Thread.MemoryBarrier(); // Barrier 1
            _complete = true;
            Thread.MemoryBarrier(); // Barrier 2
        }

        void B()
        {
            Thread.MemoryBarrier(); // Barrier 3
            if (_complete)
            {
                Thread.MemoryBarrier(); // Barrier 4
                Console.WriteLine(_answer);
            }
        }

        static void Main(string[] args)
        {
        }
    }
}
namespace ServerCore
{
    class ReaderWriterLockSlimExample
    {
        static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        class Reward { }

        static Reward GetRewardById(int id)
        {
            _lock.EnterReadLock();

            _lock.ExitReadLock();

            return null;
        }

        static void AddReward(Reward reward)
        {
            _lock.EnterWriteLock();

            _lock.ExitWriteLock();
        }

        static void Main(string[] args)
        {
        }
    }
}
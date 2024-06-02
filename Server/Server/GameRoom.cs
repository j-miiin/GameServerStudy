using ServerCore;

namespace Server
{
    internal class GameRoom : IJobQueue
    {
        List<ClientSession> _sessions = new List<ClientSession>();
        JobQueue _jobQueue = new JobQueue();

        public void Push(Action job)
        {
            _jobQueue.Push(job);
        }

        public void Broadcast(ClientSession session, string chat)
        {
            S_Chat packet = new S_Chat();
            packet.playerId = session.SessionId;
            packet.chat = $"{chat} I am {packet.playerId}";
            ArraySegment<byte> segment = packet.Write();

            // 바로 lock을 잡으면 결국 한 번에 1명씩만 통과 가능 -> 스레드가 계속 생성됨
            // JobQueue를 사용하면 lock을 잡을 필요가 없어지게 됨
            //lock (_lock)
            //{

            // n명의 유저들에게 n번 패킷을 보내므로 시간 복잡도가 O(N^2)이 됨
            // 패킷을 모아보내서 부하를 완화할 수 있음
            foreach (ClientSession s in _sessions)
                s.Send(segment);
            //}
        }

        public void Enter(ClientSession session)
        {
            _sessions.Add(session);
            session.Room = this;
        }

        public void Exit(ClientSession session)
        {
            _sessions.Remove(session);
        }
    }
}

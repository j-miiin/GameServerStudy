namespace Server
{
    internal class GameRoom
    {
        List<ClientSession> _sessions = new List<ClientSession>();
        object _lock = new object();

        public void Broadcast(ClientSession session, string chat)
        {
            S_Chat packet = new S_Chat();
            packet.playerId = session.SessionId;
            packet.chat = $"{chat} I am {packet.playerId}";
            ArraySegment<byte> segment = packet.Write();

            // 바로 lock을 잡으면 결국 한 번에 1명씩만 통과 가능 -> 스레드가 계속 생성됨
            lock (_lock)
            {
                foreach (ClientSession s in _sessions)
                    s.Send(segment);
            }
        }

        public void Enter(ClientSession session)
        {
            lock (_lock)
            {
                _sessions.Add(session);
                session.Room = this;
            }
        }

        public void Exit(ClientSession session)
        {
            lock (_lock)
            {
                _sessions.Remove(session);
            }
        }
    }
}

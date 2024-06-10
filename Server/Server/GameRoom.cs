using ServerCore;

namespace Server
{
    internal class GameRoom : IJobQueue
    {
        List<ClientSession> _sessions = new List<ClientSession>();
        JobQueue _jobQueue = new JobQueue();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

        public void Push(Action job)
        {
            _jobQueue.Push(job);
        }

        public void Flush()
        {
            foreach (ClientSession s in _sessions)
                s.Send(_pendingList);
            //Console.WriteLine($"Flushed {_pendingList.Count} items");
            _pendingList.Clear();
        }

        public void Broadcast(ArraySegment<byte> segment)
        {
            //S_Chat packet = new S_Chat();
            //packet.playerId = session.SessionId;
            //packet.chat = $"{chat} I am {packet.playerId}";
            //ArraySegment<byte> segment = packet.Write();

            // 바로 lock을 잡으면 결국 한 번에 1명씩만 통과 가능 -> 스레드가 계속 생성됨
            // JobQueue를 사용하면 lock을 잡을 필요가 없어지게 됨
            //lock (_lock)
            //{

            // n명의 유저들에게 n번 패킷을 보내므로 시간 복잡도가 O(N^2)이 됨
            // 패킷을 모아보내서 부하를 완화할 수 있음
            _pendingList.Add(segment);
            //  foreach (ClientSession s in _sessions)
            //      s.Send(segment);
            //}
        }

        public void Enter(ClientSession session)
        {
            // 플레이어 추가
            _sessions.Add(session);
            session.Room = this;

            // 신입생한테 모든 플레이어 목록 전송
            S_PlayerList players = new S_PlayerList();
            foreach (ClientSession s in _sessions)
            {
                players.players.Add(new S_PlayerList.Player()
                {
                    isSelf = (s == session),
                    playerId = s.SessionId,
                    posX = s.PosX,
                    posY = s.PosY,
                    posZ = s.PosZ,
                });
            }
            session.Send(players.Write());

            // 신입생 입장을 모두에게 알림
            S_BroadcastEnterGame enter = new S_BroadcastEnterGame();
            enter.playerId = session.SessionId;
            enter.posX = 0;
            enter.posY = 0;
            enter.posZ = 0;
            Broadcast(enter.Write());
        }

        public void Leave(ClientSession session)
        {
            // 플레이어를 제거하고
            _sessions.Remove(session);

            // 모두에게 알림
            S_BroadcastLeaveGame leave = new S_BroadcastLeaveGame();
            leave.playerId = session.SessionId;
            Broadcast(leave.Write());
        }

        public void Move(ClientSession session, C_Move packet)
        {
            // 좌표 바꿔주고
            session.PosX = packet.posX; 
            session.PosY = packet.posY; 
            session.PosZ = packet.posZ;

            // 모두에게 알림
            S_BroadcastMove move = new S_BroadcastMove();
            move.playerId = session.SessionId;
            move.posX = session.PosX;
            move.posY = session.PosY;
            move.posZ = session.PosZ;
            Broadcast(move.Write());
        }
    }
}

namespace Server
{
    // 아래처럼 클래스를 만들어서 사용할 수도 있고, Action 타입과 람다를 이용할 수도 있음
    public interface ITask
    {
        void Execute();
    }

    class BroadcastTask : ITask
    {
        GameRoom _room;
        ClientSession _session;
        string _chat;

        BroadcastTask(GameRoom room, ClientSession session, string chat)
        {
            _room = room;
            _session = session;
            _chat = chat;   
        }

        public void Execute()
        {
            //_room.Broadcast(_session, _chat);
        }
    }

    internal class TaskQueue
    {
        Queue<ITask> _queue = new Queue<ITask>();
    }
}

using System.Net;
using System.Net.Sockets;

namespace ServerCore
{
    public abstract class Session
    {
        Socket _socket;
        int _disconnected = 0;

        RecvBuffer _recvBuffer = new RecvBuffer(1024);

        object _lock = new object();
        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        public abstract void OnConnected(EndPoint endPoint);    // 클라가 접속했음을 알리는 시점
        public abstract int OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);

        public void Start(Socket socket)
        {
            _socket = socket;
            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            //recvArgs.UserToken = this;    // 식별자로 구분하거나 연동하고싶은 데이터가 있을 때 사용
            //_recvArgs.SetBuffer(new byte[1024], 0, 1024);

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRevc();
        }

        // Recv 시점은 정해져 있지만, Send 시점은 알 수 없음
        // SocketAsyncEvent를 재사용하면서, Send를 덩어리로 묶어서 보낼 수 있는 방법 필요
        // 재사용 -> _sendArgs를 전역에 선언하여 사용
        // 묶어서 보내기 -> 누군가 이미 Send 중인 경우 Send 요청을 Queue에 저장했다가 Completed가 되면 Queue를 비우는 작업
        public void Send(ArraySegment<byte> sendBuff)
        {
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuff);
                // 다른 곳에서 이미 Send 중이여서 pending이 true일 경우
                // 큐에 Send 작업만 넣고 종료
                if (_pendingList.Count == 0)
                    RegisterSend();
            }
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
                return;

            OnDisconnected(_socket.RemoteEndPoint);
            // 쫓아낸다
            _socket.Shutdown(SocketShutdown.Both); // 미리 주의를 주는 것
            _socket.Close();
        }

        #region 네트워크 통신

        void RegisterSend()
        {
            while (_sendQueue.Count > 0)
            {
                ArraySegment<byte> buff = _sendQueue.Dequeue();
                // ArraySegment : 어떤 배열의 일부를 나타내는 구조체
                _pendingList.Add(buff);
            }

            // BufferList 안에 있는 버퍼들을 한 번에 Send
            // BufferList에 바로 Add하면 안됨
            // list를 따로 만들어서 Add를 한 뒤, 최종적으로 BufferList에 대입해줘야 함
            _sendArgs.BufferList = _pendingList;
            
            bool pending = _socket.SendAsync(_sendArgs);
            if (!pending)
                OnSendCompleted(null, _sendArgs);
        }

        // _sendArgs.Completed에 콜백으로 연결되어 다른 Thread에서 실행될 수 있으므로
        // lock을 걸어서 작업
        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        _sendArgs.BufferList = null;
                        _pendingList.Clear();

                        OnSend(_sendArgs.BytesTransferred);

                        if (_sendQueue.Count > 0)
                            RegisterSend();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleted Failed {e}");
                    }
                }
                else
                {
                    Disconnect();
                }
            }
        }

        void RegisterRevc()
        {
            _recvBuffer.Clean();
            ArraySegment<byte> segment = _recvBuffer.WriteSegment;
            _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            bool pending = _socket.ReceiveAsync(_recvArgs);
            // 바로 성공한 경우
            if (!pending)
                OnRecvCompleted(null, _recvArgs);
        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    // Write 커서 이동
                    if (!_recvBuffer.OnWrite(args.BytesTransferred))
                    {
                        Disconnect();
                        return;
                    }

                    // 컨텐츠 쪽으로 데이터를 넘겨주고 얼마나 처리했는지 받음
                    //OnRecv(new ArraySegment<byte>(args.Buffer, args.Offset, args.BytesTransferred));
                    int processLen = OnRecv(_recvBuffer.ReadSegment);
                    if (processLen < 0 || _recvBuffer.DataSize < processLen)
                    {
                        Disconnect();
                        return;
                    }

                    // Read 커서 이동
                    if (!_recvBuffer.OnRead(processLen))
                    {
                        Disconnect();
                        return;
                    }

                    RegisterRevc();
                } catch(Exception e)
                {
                    Console.WriteLine($"OnRecvCompleted Failed {e}");
                }
            }
            else
            {
                Disconnect();
            }
        }
        #endregion
    }
}

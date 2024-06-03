using System.Net;
using System.Net.Sockets;

namespace ServerCore
{
    public abstract class PacketSession : Session
    {
        public static readonly int HeaderSize = 2;

        // sealed : 봉인했다 
        // 다른 클래스가 PacketSession을 상속받아서 OnRecv를 오버라이드 할 수 없음
        public sealed override int OnRecv(ArraySegment<byte> buffer)
        {
            int processLen = 0;
            int packetCount = 0;

            while (true)
            {
                // 최소한 헤더는 파싱할 수 있는지 확인
                if (buffer.Count < HeaderSize) break;

                // 패킷이 완전체로 도착했는지 확인
               ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
                if (buffer.Count < dataSize) break;

                // 패킷 조립 가능
                OnRecvPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));
                packetCount++;

                processLen += dataSize;
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
            }

            if (packetCount > 1) Console.WriteLine($"패킷 모아보내기 : {packetCount}");

            return processLen;
        }

        public abstract void OnRecvPacket(ArraySegment<byte> buffer);
    }

    public abstract class Session
    {
        Socket _socket;
        int _disconnected = 0;

        RecvBuffer _recvBuffer = new RecvBuffer(65535);

        object _lock = new object();
        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        public abstract void OnConnected(EndPoint endPoint);    // 클라가 접속했음을 알리는 시점
        public abstract int OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);

        void Clear()
        {
            lock (_lock)
            {
                _sendQueue.Clear();
                _pendingList.Clear();
            }
        }

        public void Start(Socket socket)
        {
            _socket = socket;
            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            //recvArgs.UserToken = this;    // 식별자로 구분하거나 연동하고싶은 데이터가 있을 때 사용
            //_recvArgs.SetBuffer(new byte[1024], 0, 1024);

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();
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
        public void Send(List<ArraySegment<byte>> sendBuffList)
        {
            if (sendBuffList.Count == 0) return;

            lock (_lock)
            {
                foreach (ArraySegment<byte> sendBuff in sendBuffList)   
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
            Clear();
        }

        #region 네트워크 통신

        void RegisterSend()
        {
            if (_disconnected == 1) return;

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
            
            try
            {
                bool pending = _socket.SendAsync(_sendArgs);
                if (!pending)
                    OnSendCompleted(null, _sendArgs);
            }
            catch (Exception e)
            {
                Console.WriteLine($"RegisterSend Failed {e}");
            }
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

        void RegisterRecv()
        {
            if (_disconnected == 1) return;

            _recvBuffer.Clean();
            ArraySegment<byte> segment = _recvBuffer.WriteSegment;
            _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            try
            {
                bool pending = _socket.ReceiveAsync(_recvArgs);
                // 바로 성공한 경우
                if (!pending)
                    OnRecvCompleted(null, _recvArgs);
            }
            catch (Exception e)
            {
                Console.WriteLine($"RegisterRecv Failed {e}");
            }
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

                    RegisterRecv();
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

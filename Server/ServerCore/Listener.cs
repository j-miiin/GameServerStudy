using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    internal class Listener
    {
        Socket _listenSocket;
        Action<Socket> _onAcceptHandler;

        public void Init(IPEndPoint endPoint, Action<Socket> onAcceptHandler)
        {
            // 문지기가 든 휴대폰
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _onAcceptHandler += onAcceptHandler;

            // 문지기 교육
            _listenSocket.Bind(endPoint); // 식당 주소, 정문인지 후문인지(포트 번호)

            // 영업 시작
            // backlog : 최대 대기수
            _listenSocket.Listen(10);

            // 처음에는 직접 낚싯대 만들어서 던져놓기
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            RegisterAccept(args);
        }

        void RegisterAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;

            bool pending = _listenSocket.AcceptAsync(args);

            // 낚싯대를 던지자마자 바로 걸린 경우
            if (!pending)
                OnAcceptCompleted(null, args);
        }

        // 낚싯대 끌어올리기
        // 별도의 Thread에서 실행됨
        void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                _onAcceptHandler.Invoke(args.AcceptSocket);
            } 
            else
                Console.WriteLine(args.SocketError.ToString());

            // 낚싯대 다시 던지기
            RegisterAccept(args);
        }

        //public Socket Accept()
        //{
        //    _listenSocket.AcceptAsync();
        //    return _listenSocket.Accept();
        //}
    }
}

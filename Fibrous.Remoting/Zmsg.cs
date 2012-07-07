namespace Fibrous.Zmq
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using CrossroadsIO;

    public class ZMessage
    {
        private readonly Encoding _encoding = Encoding.Unicode;
        private readonly List<byte[]> _msgParts = new List<byte[]>();
        private readonly byte[] _buffer = new byte[1024 * 1024 * 2];

        public ZMessage()
        {
        }

        public List<byte[]> MsgParts
        {
            get
            {
                return _msgParts;
            }
        }

        public ZMessage(Socket skt)
        {
            Recv(skt);
        }

        public ZMessage(string body, Encoding encoding)
        {
            _encoding = encoding;
            Append(_encoding.GetBytes(body));
        }

        public ZMessage(string body)
        {
            Append(_encoding.GetBytes(body));
        }

        public ZMessage(byte[] body)
        {
            Append(body);
        }

        public void Recv(Socket socket)
        {
            _msgParts.Clear();
            _msgParts.Add(Receive(socket)); //block before read more
            while (socket.ReceiveMore)
            {
                _msgParts.Add(Receive(socket));
            }
        }

        private byte[] Receive(Socket socket)
        {
            int length = socket.Receive(_buffer);
            var reply = new byte[length];
            Array.Copy(_buffer, reply, length);
            return reply;
        }

        private byte[] Receive(Socket socket, TimeSpan timeout)
        {
            int length = socket.Receive(_buffer, timeout);
            var reply = new byte[length];
            Array.Copy(_buffer, reply, length);
            return reply;
        }

        public bool Recv(Socket socket, TimeSpan timeout)
        {
            _msgParts.Clear();
            byte[] receive = Receive(socket, timeout);
            if (receive == null || receive.Length == 0)
            {
                return false;
            }
            _msgParts.Add(receive); //block before read more
            while (socket.ReceiveMore)
            {
                _msgParts.Add(Receive(socket));
            }
            return true;
        }

        public void Send(Socket socket)
        {
            try
            {
                for (int index = 0; index < _msgParts.Count - 1; index++)
                {
                    socket.SendMore(_msgParts[index]);
                }
                socket.Send(Body);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //   throw;
            }
        }

        public string BodyToString()
        {
            return _encoding.GetString(Body);
        }

        public void StringToBody(string body)
        {
            Body = _encoding.GetBytes(body);
        }

        public void Append(byte[] data)
        {
            _msgParts.Add(data);
        }

        public byte[] Pop()
        {
            byte[] data = _msgParts[0];
            _msgParts.RemoveAt(0);
            return data;
        }

        public void Push(byte[] data)
        {
            _msgParts.Insert(0, data);
        }

        public void Wrap(byte[] address, byte[] delim)
        {
            if (delim != null)
            {
                Push(delim);
            }
            Push(address);
        }

        public byte[] Unwrap()
        {
            byte[] addr = Pop();
            if (Address.Length == 0)
            {
                Pop();
            }
            return addr;
        }

        public int PartCount
        {
            get
            {
                return _msgParts.Count;
            }
        }
        public byte[] Address
        {
            get
            {
                return _msgParts[0];
            }
            set
            {
                Push(value);
            }
        }
        public byte[] Body
        {
            get
            {
                return _msgParts[_msgParts.Count - 1];
            }
            set
            {
                _msgParts[_msgParts.Count - 1] = value;
            }
        }
    }
}
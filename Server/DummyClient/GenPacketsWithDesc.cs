//using ServerCore;
//using System.Text;

//class PlayerInfoReq
//{
//    public long playerId;
//    public string name;

//    public struct SkillInfo
//    {
//        public int id;
//        public short level;
//        public float duration;

//        public bool Write(Span<byte> s, ref ushort count)
//        {
//            bool success = true;
//            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), id);
//            count += sizeof(int);
//            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), level);
//            count += sizeof(short);
//            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), duration);
//            count += sizeof(float);
//            return success;
//        }

//        public void Read(ReadOnlySpan<byte> s, ref ushort count)
//        {
//            id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
//            count += sizeof(int);
//            level = BitConverter.ToInt16(s.Slice(count, s.Length - count));
//            count += sizeof(short);
//            duration = BitConverter.ToSingle(s.Slice(count, s.Length - count));
//            count += sizeof(float);
//        }
//    }

//    public List<SkillInfo> skills = new List<SkillInfo>();

//    public void Read(ArraySegment<byte> segment)
//    {
//        ushort count = 0;

//        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);

//        //ushort size = BitConverter.ToUInt16(s.Array, s.Offset);
//        count += sizeof(ushort);
//        //ushort id = BitConverter.ToUInt16(s.Array, s.Offset + count);
//        count += sizeof(ushort);

//        //this.playerId = BitConverter.ToInt64(s.Array, s.Offset + count);
//        // 범위를 초과하는 값을 파싱하려고 하면 예외가 발생하도록 함
//        this.playerId = BitConverter.ToInt64(s.Slice(count, s.Length - count));
//        count += sizeof(long);

//        // string
//        ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
//        count += sizeof(ushort);
//        this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
//        count += nameLen;

//        // Skill List
//        skills.Clear();
//        ushort skillLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
//        count += sizeof(ushort);
//        for (int i = 0; i < skillLen; i++)
//        {
//            SkillInfo skill = new SkillInfo();
//            skill.Read(s, ref count);
//            skills.Add(skill);
//        }
//    }

//    public ArraySegment<byte> Write()
//    {
//        //byte[] sendBuff = Encoding.UTF8.GetBytes($"Hello World! {i}");
//        ArraySegment<byte> segment = SendBufferHelper.Open(4096);

//        ushort count = 0;
//        bool success = true;

//        Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

//        // size는 마지막에 알 수 있기 때문에 뒤로 옮김
//        //success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset, s.Count), packet.size);
//        count += sizeof(ushort);
//        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.PlayerInfoReq);
//        count += sizeof(ushort);
//        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
//        count += sizeof(long);

//        // string
//        // string 길이를 2byte로 보내고 string을 byte 배열로 보내기
//        //ushort nameLen = (ushort)Encoding.Unicode.GetByteCount(this.name);
//        //success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
//        //count += sizeof(ushort);
//        //Array.Copy(Encoding.Unicode.GetBytes(this.name), 0, segment.Array, count, nameLen);
//        //count += nameLen;

//        // 위 코드를 정리한 것
//        ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segment.Array, segment.Offset + count + sizeof(ushort));
//        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
//        count += sizeof(ushort);
//        count += nameLen;

//        // Skill List
//        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)skills.Count);
//        count += sizeof(ushort);
//        foreach (SkillInfo skill in skills)
//        {
//            success &= skill.Write(s, ref count);
//        }

//        success &= BitConverter.TryWriteBytes(s, count);

//        #region 이전 방법
//        //byte[] size = BitConverter.GetBytes(packet.size);   // 2byte
//        //byte[] packetId = BitConverter.GetBytes(packet.packetId);   // 2byte
//        //byte[] playerId = BitConverter.GetBytes(packet.playerId);   // 8byte

//        //Array.Copy(size, 0, s.Array, s.Offset + count, size.Length);
//        //count += 2;
//        //Array.Copy(packetId, 0, s.Array, s.Offset + count, packetId.Length);
//        //count += 2;
//        //Array.Copy(playerId, 0, s.Array, s.Offset + count, playerId.Length);
//        //count += 8;
//        #endregion

//        if (!success) return null;

//        return SendBufferHelper.Close(count);
//    }
//}

////class PlayerInfoOk : Packet
////{
////    public int hp;
////    public int attack;
////}

//public enum PacketID
//{
//    PlayerInfoReq = 1,
//    PlayerInfoOk = 2,
//}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServer.Net
{
    public class ByteArray
    {
        public const int DefaultSize = 1024;
        private int initSize;
        public byte[] bytes;
        public int readIndex;
        public int writeIndex;
        //容量
        private int Capacity;
        //剩余可读字节数
        public int Remain { get { return Capacity -writeIndex; } }
        //当前数据长度
        public int Length { get { return writeIndex-readIndex; } }

        public ByteArray()
        {
            initSize = DefaultSize;
            bytes = new byte[initSize];
            readIndex = 0;
            writeIndex = 0;
            Capacity = initSize;
        }
        //重载缓冲区
        public void CheckAndMoveBytes()
        {
            if (readIndex > 0)
            {
                int remainingLength = Length;
                Array.Copy(bytes, readIndex, bytes, 0, remainingLength);
                readIndex = 0;
                writeIndex = remainingLength;
            }
        }
        //扩容缓冲区
        public void ExpandCapacity(int size)
        {
            if (size <= Capacity)
            {
                return;
            }
            int remainingLength = Length;
            int newCapacity = DefaultSize;
            while (newCapacity < size)
            {
                newCapacity *= 2;
            }
            Capacity = newCapacity;
            byte[] newBytes = new byte[Capacity];
            Array.Copy(bytes, readIndex, newBytes, 0, remainingLength);
            bytes = newBytes;
            readIndex = 0;
            writeIndex = remainingLength;
        }
    }
}

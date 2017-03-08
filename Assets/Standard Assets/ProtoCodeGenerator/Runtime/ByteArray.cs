using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
//这个类是对memorystream的封装，在c++代码里只要类名相同
//代码生成是一样的逻辑，只是基础支持类不一样
//包括发包逻辑等也是基于ByteArray来做就可以
//这个是导出给c#用，不要转换为js
public class ByteArray
{
    static byte[] TmpBuffer4Bytes = new byte[4];
    private MemoryStream m_bytes = new MemoryStream(256);
   
    public int Length
    {
        get { return (int)m_bytes.Length; }
    }

    public byte[] Bytes
    {
        get { return m_bytes.ToArray(); }
    }

    public ByteArray(byte[] bytes)
    {
        m_bytes = new MemoryStream(bytes);
    }

    public ByteArray()
    {

    }

    public void SerializeInt(int value)
    {
        byte[] intBytes = BitConverter.GetBytes(value);
        m_bytes.Write(intBytes, 0, 4);
    }

    public int DeserializeInt()
    {
        m_bytes.Read(TmpBuffer4Bytes, 0, 4);
        return BitConverter.ToInt32(TmpBuffer4Bytes, 0);
    }

    public void SerializeFloat(float value)
    {
        byte[] floatBytes = BitConverter.GetBytes(value);
        m_bytes.Write(floatBytes, 0, 4);
    }

    public float DeserializeFloat()
    {
        m_bytes.Read(TmpBuffer4Bytes, 0, 4);
        return BitConverter.ToSingle(TmpBuffer4Bytes, 0);
    }

    public void SerializeString(string value)
    {
        byte[] utf8StringBytes = Encoding.UTF8.GetBytes(value);
        int utf8BytesLen = utf8StringBytes.Length;
        byte[] utf8StringBytesLenBytes = BitConverter.GetBytes(utf8BytesLen);
        m_bytes.Write(utf8StringBytesLenBytes, 0, 4);
        m_bytes.Write(utf8StringBytes, 0, utf8BytesLen);
    }

    public string DeserializeString()
    {
        m_bytes.Read(TmpBuffer4Bytes, 0, 4);
        int stringBytesLen = BitConverter.ToInt32(TmpBuffer4Bytes, 0);
        byte[] stringBytes = new byte[stringBytesLen];
        m_bytes.Read(stringBytes, 0, stringBytesLen);
        return BitConverter.ToString(stringBytes);
    }
}


 
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
//这个类是所有协议的父类，是需要转换为js的
public abstract class ProtoUtils
{
    public static void SerializeIntArray(ByteArray buffer, List<int> value)
    {
        //首先写入count
        int valueCount = value.Count;
        buffer.SerializeInt(valueCount);
        //依次写入各个值
        List<int>.Enumerator enu = value.GetEnumerator();
        while (enu.MoveNext())
        {
            buffer.SerializeInt(enu.Current);
        }
    }

    public static List<int> DeserializeIntArray(ByteArray buffer)
    {
        //首先获得count
        int count = buffer.DeserializeInt();
        List<int> ret = new List<int>(count);
        for(int i=0;i<count;++i)
        {
            ret.Add(buffer.DeserializeInt());
        }
        return ret;
    }

    public static void SerializeFloatArray(ByteArray buffer,List<float> value)
    {
        //首先写入count
        int valueCount = value.Count;
        buffer.SerializeInt(valueCount);
       
        //依次写入各个值
        List<float>.Enumerator enu = value.GetEnumerator();
        while (enu.MoveNext())
        {
            buffer.SerializeFloat(enu.Current);
        }
    }

    public static List<float> DeserializeFloatArray(ByteArray buffer)
    {
        //首先获得count
        int count = buffer.DeserializeInt();
        List<float> ret = new List<float>(count);
        for (int i = 0; i < count; ++i)
        {
            ret.Add(buffer.DeserializeFloat());
        }
        return ret;
    }
}


 
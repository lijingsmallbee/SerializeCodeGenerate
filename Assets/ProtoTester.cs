using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Text;
using protocol.msg2;
namespace Test
{
	/// The script to initilize the game start.
	public class ProtoTester : MonoBehaviour
	{

        void Start()
        {
        }
		void OnGUI()
        {
            if (GUI.Button(new Rect(20, 150, 150, 40), "Test LoginMsg"))
            {
                LoginMsg msg = new LoginMsg();
                msg.passport = "ºôºô¹þ¹þ";
                msg.platform = 2;
                StringBuilder sb = new StringBuilder(512);
                sb.Length = 0;
                sb.Append("input string is ").Append(msg.passport).Append(" int is ").Append(msg.platform);
                Debug.LogError(sb.ToString());
                ByteArray array = new ByteArray();
                msg.Serialize(array);

                ByteArray receive = new ByteArray(array.Bytes);
                LoginMsg ret = new LoginMsg();
                ret.Deserialize(receive);
                sb.Length = 0;
                sb.Append("output string is ").Append(ret.passport).Append(" int is ").Append(ret.platform);
                Debug.LogError(sb.ToString());

            }
            if (GUI.Button(new Rect(20, 200, 150, 40), "RoleListInfoMsg"))
            {
                RoleListInfoMsg msg = new RoleListInfoMsg();
                msg.testClass.items.Add(100);
                msg.testClass.items.Add(1000);
                msg.testClass.testfloats.Add(101.01f);
                msg.testClass.testfloats.Add(1001.001f);

                msg.roles.Add(msg.testClass);
                msg.roles.Add(msg.testClass);

                ByteArray array = new ByteArray();
                msg.Serialize(array);

                ByteArray receive = new ByteArray(array.Bytes);
                RoleListInfoMsg ret = new RoleListInfoMsg();
                ret.Deserialize(receive);
                Debug.LogError("you can check the result by break point,oupt class object is too ma fan");
                byte[] none = receive.Bytes;
            }
        }
	}
}

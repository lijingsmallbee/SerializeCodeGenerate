using System;
using System.IO;
using System.Collections.Generic;
namespace protocol.msg2
{
class LoginMsg
{
	public int platform;
	public string passport = string.Empty;

	
	public void Serialize(ByteArray buffer)
	{
		buffer.SerializeInt(platform);
		buffer.SerializeString(passport);

	}
	
	public void Deserialize(ByteArray buffer)
	{
		platform = buffer.DeserializeInt();
		passport = buffer.DeserializeString();

	}
}
class CreateRoleMsg
{
	public int classType;
	public string name = string.Empty;

	
	public void Serialize(ByteArray buffer)
	{
		buffer.SerializeInt(classType);
		buffer.SerializeString(name);

	}
	
	public void Deserialize(ByteArray buffer)
	{
		classType = buffer.DeserializeInt();
		name = buffer.DeserializeString();

	}
}
class MoneyInfoMsg
{
	public int coins;
	public int golds;

	
	public void Serialize(ByteArray buffer)
	{
		buffer.SerializeInt(coins);
		buffer.SerializeInt(golds);

	}
	
	public void Deserialize(ByteArray buffer)
	{
		coins = buffer.DeserializeInt();
		golds = buffer.DeserializeInt();

	}
}
class ReconnectMsg
{
	public string token = string.Empty;
	public string passport = string.Empty;

	
	public void Serialize(ByteArray buffer)
	{
		buffer.SerializeString(token);
		buffer.SerializeString(passport);

	}
	
	public void Deserialize(ByteArray buffer)
	{
		token = buffer.DeserializeString();
		passport = buffer.DeserializeString();

	}
}
class RoleBasicInfoMsg
{
	public List<int> items =new List<int>();
	public List<float> testfloats =new List<float>();

	
	public void Serialize(ByteArray buffer)
	{
		ProtoUtils.SerializeIntArray( buffer ,items);
		ProtoUtils.SerializeFloatArray( buffer ,testfloats);

	}
	
	public void Deserialize(ByteArray buffer)
	{
		items = ProtoUtils.DeserializeIntArray(buffer);
		testfloats = ProtoUtils.DeserializeFloatArray(buffer);

	}
}
class RoleListInfoMsg
{
	public List<RoleBasicInfoMsg> roles =new List<RoleBasicInfoMsg>();
	public RoleBasicInfoMsg testClass = new RoleBasicInfoMsg();

	
	public void Serialize(ByteArray buffer)
	{
		int countroles = roles.Count; 
		buffer.SerializeInt(countroles); 
		for(int i=0;i<countroles;++i) 
		{
			RoleBasicInfoMsg instroles = roles[i]; 
			instroles.Serialize(buffer);
		}
		testClass.Serialize(buffer);

	}
	
	public void Deserialize(ByteArray buffer)
	{
		int countroles = buffer.DeserializeInt(); 
		for(int i=0;i<countroles;++i) 
		{
			RoleBasicInfoMsg instroles = new RoleBasicInfoMsg(); 
			instroles.Deserialize(buffer);
			roles.Add(instroles);
		}
		testClass.Deserialize(buffer);

	}
}

}
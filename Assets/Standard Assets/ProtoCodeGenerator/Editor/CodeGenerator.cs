using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;
public class CodeGenerator : EditorWindow 
{
 
    enum FieldType
    {
        FT_None,
        FT_Int,
        FT_Float,
        FT_String,
        FT_Class,
    }
    class FieldInfo
    {
        public string m_name;
        public FieldType m_type;
        public bool isrepeat = false;
        public string className;  //类型名，针对class类型
    }

    class ClassInfo
    {
        public List<FieldInfo> m_allFields = new List<FieldInfo>();
        public string className;
    }

    class FileInfo
    {
        public string fileName;
        public string nameSpace;
        public Dictionary<string, ClassInfo> m_allClasses = new Dictionary<string, ClassInfo>();
        public Dictionary<string, enumInfo> m_allEnums = new Dictionary<string, enumInfo>();
    }

    class enumInfo
    {
        public string enumName; 
    }

    [MenuItem ("Tools/CodeGenerator")]
    static void AddWindow ()
	{       
		//新建窗口
		Rect  wr = new Rect (0,0,400,100);
        CodeGenerator window = (CodeGenerator)EditorWindow.GetWindowWithRect (typeof (CodeGenerator),wr,true,"Code Generator");	
		window.Show();
 
    }
	public void Awake () 
	{
	}
 
	//绘制窗口时调用
    void OnGUI () 
	{
        UnityEngine.Object obj = Selection.activeObject;
        bool isValid = false;
        if (obj != null) 
        {
            System.Type type = obj.GetType();
            if(type == typeof(TextAsset))
            {
                isValid = true;
            }
        }
        if (isValid) 
        {
            EditorGUILayout.LabelField("当前选中的文件是", AssetDatabase.GetAssetPath(obj));
        } 
        else 
        {
            EditorGUILayout.LabelField ("无效选中,请选择一个文本格式的输入文件");
            return;
        }
        if (GUILayout.Button("生成C#代码", GUILayout.Width(200)))
        {
            TextAsset asset = obj as TextAsset;
            FileInfo info = ReadFileInfo(asset.text);
            UnityEngine.Object protoAsset = AssetDatabase.LoadAssetAtPath("Assets/Standard Assets/ProtoCodeGenerator/Editor/protofile.txt", typeof(TextAsset));
            if (protoAsset)
            {
                StringBuilder protofilesb = new StringBuilder(2048);
                TextAsset protofileText = protoAsset as TextAsset;
                string protostring = protofileText.text.Clone() as string;
                foreach(KeyValuePair<string,ClassInfo> cinfo in info.m_allClasses)
                {
                    protofilesb.Append(GenerateClass(cinfo.Value));
                }
                protostring = protostring.Replace("$classfield$", protofilesb.ToString());
                protostring = protostring.Replace("$namespace$", info.nameSpace);

                string dir = AssetDatabase.GetAssetPath(asset);
                string filename = Path.GetFileName(dir);
                filename = filename.Replace(".txt", ".cs");
                string path = EditorUtility.SaveFilePanel("get save path", Application.dataPath,filename, "cs");
                EditorUtils.SaveTextFile(path, protostring);
                AssetDatabase.Refresh();
            }
            
        }
        if(GUILayout.Button("生成C艹代码",GUILayout.Width(200)))
        {
            return;
        }
    }
 
	//更新
	void Update()
	{
 
	}
 
	void OnFocus()
	{
		Debug.Log("当窗口获得焦点时调用一次");
	}
 
	void OnLostFocus()
	{
		Debug.Log("当窗口丢失焦点时调用一次");
	}
 
	void OnHierarchyChange()
	{
		Debug.Log("当Hierarchy视图中的任何对象发生改变时调用一次");
	}
 
	void OnProjectChange()
	{
		Debug.Log("当Project视图中的资源发生改变时调用一次");
	}
 
	void OnInspectorUpdate()
	{
	   //Debug.Log("窗口面板的更新");
	   //这里开启窗口的重绘，不然窗口信息不会刷新
	   this.Repaint();
	}
 
	void OnSelectionChange()
	{
	}
 
	void OnDestroy()
	{
		Debug.Log("当窗口关闭时调用");
	}
    //通过字符串获取文件信息
    FileInfo ReadFileInfo(string text)
    {
        int start = 0;
        FileInfo fileinfo = new FileInfo();
        GetAllEnums(text, fileinfo);
        //第一遍遍历先获得所有的message类型
        while (start < text.Length)
        {
            //逐行解析
            string lineNew = EditorUtils.readLine(text, ref start);
            List<string> allWords = EditorUtils.splitLine(lineNew);
            if(allWords.Count > 0)
            {
                string keyWord = allWords[0];
                if(keyWord.Contains("//"))  //是注释，直接忽略
                {
                    //do nothing
                }
                else if(keyWord == "package")  //是包名
                {
                    //测试代码，防止发生跟已经生成的protobuff的文件发生冲突
                    fileinfo.nameSpace = allWords[1];
                    fileinfo.nameSpace = fileinfo.nameSpace.Replace(";","2");
                }
                else if(keyWord == "message")  //是消息，要读取到结束
                {
                    ClassInfo classInfo = new ClassInfo();
                    classInfo.className = allWords[1];
                    fileinfo.m_allClasses.Add(classInfo.className, classInfo);
                    while(start < text.Length)
                    {
                        string msgLine = EditorUtils.readLine(text, ref start);
                        List<string> allMsgWords = EditorUtils.splitLine(msgLine);
                        if(allMsgWords.Count > 0)
                        {
                            string msgKeyword = allMsgWords[0];
                            if (msgKeyword.Contains("//"))  //是注释，直接忽略
                            {
                                //do nothing
                            }
                            else if (msgKeyword == "{")
                            {
                                //do nothing
                            }
                            else if (msgKeyword == "}")
                            {
                                break;
                            }
                            else //字段定义
                            {
                                //字段分析利用了protobuff里的等号的功能进行分析，
                                //如果直接对字段类型进行分析，则要考虑很多因素，例如import的支持
                                //目前做法是如果有等号，就认为是有效字段，不能识别的类型默认为class
                                for(int i=0;i<allMsgWords.Count;++i)
                                {
                                    string cur = allMsgWords[i];
                                    //等号前后都有空格
                                    if(cur == "=")
                                    {
                                        FieldInfo fieldInfo = new FieldInfo();
                                        fieldInfo.m_name = allMsgWords[i - 1];
                                        fieldInfo.className = allMsgWords[i - 2];
                                        fieldInfo.m_type = GetTypeInfo(allMsgWords[i - 2],fileinfo);
                                        if(i == 3)  //带有repeated或者optional字段
                                        {
                                            string msg0 = allMsgWords[0].Replace("\t", "");
                                            fieldInfo.isrepeat = msg0 == "repeated";
                                        }
                                        classInfo.m_allFields.Add(fieldInfo);
                                    }
                                    else if(cur.Contains("="))
                                    {
                                        //等号与前后连写有三种情况 a=2 a= 2 a =2
                                        string[] subStrings = cur.Split('=');
                                        //第一种情况，a=2
                                        if(subStrings.Length == 2)
                                        {
                                            FieldInfo fieldInfo = new FieldInfo();
                                            fieldInfo.m_name = subStrings[0];
                                            fieldInfo.className = allMsgWords[i - 1];
                                            fieldInfo.m_type = GetTypeInfo(allMsgWords[i - 1],fileinfo);
                                            if (i == 2)  //带有repeated或者optional字段
                                            {
                                                string msg0 = allMsgWords[0].Replace("\t", "");
                                                fieldInfo.isrepeat = msg0 == "repeated";
                                            }
                                            classInfo.m_allFields.Add(fieldInfo);
                                        }
                                        //第二三中情况，a= 2 或者 a =2
                                        else if(subStrings.Length == 1)
                                        {
                                            //根据能否转为数字决定是哪种情况
                                            int temp = 0;
                                            //这是跟后边的数字连在一起，所以名字是上个字符串 a =2
                                            if(int.TryParse(subStrings[0], out temp))
                                            {
                                                FieldInfo fieldInfo = new FieldInfo();
                                                fieldInfo.m_name = allMsgWords[i-1];
                                                fieldInfo.className = allMsgWords[i - 2];
                                                fieldInfo.m_type = GetTypeInfo(allMsgWords[i - 2],fileinfo);
                                                if (i == 3)  //带有repeated或者optional字段
                                                {
                                                    string msg0 = allMsgWords[0].Replace("\t", "");
                                                    fieldInfo.isrepeat = msg0 == "repeated";
                                                }
                                                classInfo.m_allFields.Add(fieldInfo);
                                            }
                                            //剩余情况是等号与前边的变量名在一起 a= 2
                                            else
                                            {
                                                FieldInfo fieldInfo = new FieldInfo();
                                                fieldInfo.m_name = subStrings[0];
                                                fieldInfo.className = allMsgWords[i - 2];
                                                fieldInfo.m_type = GetTypeInfo(allMsgWords[i - 2],fileinfo);
                                                if (i == 2)  //带有repeated或者optional字段
                                                {
                                                    string msg0 = allMsgWords[0].Replace("\t", "");
                                                    fieldInfo.isrepeat = msg0 == "repeated";
                                                }
                                                classInfo.m_allFields.Add(fieldInfo);
                                            }
                                        }
                                    }
                                }
                            }
                            
                        }
                    }
                }
            } 
        }
        return fileinfo;
    }

    void GetAllEnums(string text, FileInfo info)
    {
        int start = 0;
        //第一遍遍历先获得所有的message类型
        while (start < text.Length)
        {
            //逐行解析
            string lineNew = EditorUtils.readLine(text, ref start);
            List<string> allWords = EditorUtils.splitLine(lineNew);
            if (allWords.Count > 0)
            {
                string keyWord = allWords[0];
                if (keyWord == "enum")  //枚举
                {
                    enumInfo enuminfo = new enumInfo();
                    enuminfo.enumName = allWords[1];
                    info.m_allEnums.Add(enuminfo.enumName, enuminfo);
                }
            }
        }
    }

    FieldType GetTypeInfo(string word,FileInfo info)
    {
        if (word == "int32" || word == "int")
        {
            return FieldType.FT_Int;
        }
        else if (word == "float")
        {
            return FieldType.FT_Float;
        }
        else if (word == "string")
        {
            return FieldType.FT_String;
        }
        else if (word == "double")
        {
            return FieldType.FT_Float;
        }
        else
        {
            enumInfo enuinfo = null;
            //是枚举类型，改为int
            if(info.m_allEnums.TryGetValue(word, out enuinfo))
            {
                return FieldType.FT_Int;
            }
        }
        return FieldType.FT_Class;
    }

    string GenerateClass(ClassInfo classInfo)
    {
        UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath("Assets/Standard Assets/ProtoCodeGenerator/Editor/protoclass.txt", typeof(TextAsset));
        if(asset)
        {
            TextAsset classtext = asset as TextAsset;
            string classT = classtext.text;
            classT = classT.Replace("$className$", classInfo.className);
            StringBuilder fields = new StringBuilder(1024);
            for (int i=0;i<classInfo.m_allFields.Count;++i)
            {
                fields.Append(GenerateField(classInfo.m_allFields[i]));
            }
            classT = classT.Replace("$field$", fields.ToString());
            fields.Length = 0;
            for (int i = 0; i < classInfo.m_allFields.Count; ++i)
            {
                fields.Append(GenerateSerializeField(classInfo.m_allFields[i]));
            }
            classT = classT.Replace("$readfield$", fields.ToString());
            fields.Length = 0;
            for (int i = 0; i < classInfo.m_allFields.Count; ++i)
            {
                fields.Append(GenerateDeserializeField(classInfo.m_allFields[i]));
            }
            classT = classT.Replace("$writefield$", fields.ToString());
            return classT;
        }
        return null;
    }

    string GenerateSerializeField(FieldInfo fieldInfo)
    {
        StringBuilder fieldsb = new StringBuilder(256);
        fieldsb.Append("\t\t");
        switch(fieldInfo.m_type)
        {
            case FieldType.FT_Float:
                if(fieldInfo.isrepeat)
                {
                    fieldsb.Append("ProtoUtils.SerializeFloatArray( buffer ,").Append(fieldInfo.m_name).Append(");\r\n");
                }
                else
                {
                    fieldsb.Append("buffer.SerializeFloat(").Append(fieldInfo.m_name).Append(");\r\n");
                }
                break;
            case FieldType.FT_Int:
                if (fieldInfo.isrepeat)
                {
                    fieldsb.Append("ProtoUtils.SerializeIntArray( buffer ,").Append(fieldInfo.m_name).Append(");\r\n");
                }
                else
                {
                    fieldsb.Append("buffer.SerializeInt(").Append(fieldInfo.m_name).Append(");\r\n");
                }
                break;
            case FieldType.FT_String:
                if (fieldInfo.isrepeat)
                {
                    Debug.LogError("string list is not support");
                }
                else
                {
                    fieldsb.Append("buffer.SerializeString(").Append(fieldInfo.m_name).Append(");\r\n");
                }
                break;
            case FieldType.FT_Class:
                if (fieldInfo.isrepeat)
                {
                    string fieldNameCount = "count" + fieldInfo.m_name;
                    fieldsb.Append("int ").Append(fieldNameCount).Append(" = ").Append(fieldInfo.m_name).Append(".Count; \r\n");
                    fieldsb.Append("\t\tbuffer.SerializeInt(").Append(fieldNameCount).Append("); \r\n");
                    fieldsb.Append("\t\tfor(int i=0;i<").Append(fieldNameCount).Append(";++i) \r\n");
                    fieldsb.Append("\t\t{\r\n");
                    string newInstName = "inst" + fieldInfo.m_name;
                    fieldsb.Append("\t\t\t").Append(fieldInfo.className).Append(" ").Append(newInstName).Append(" = ").Append(fieldInfo.m_name).Append("[i]; \r\n");
                    fieldsb.Append("\t\t\t").Append(newInstName).Append(".Serialize(buffer);\r\n");
                    fieldsb.Append("\t\t}\r\n");
                }
                else
                {
                    fieldsb.Append(fieldInfo.m_name).Append(".Serialize(buffer);\r\n");
                }
                break;
        }
        return fieldsb.ToString();
    }

    string GenerateDeserializeField(FieldInfo fieldInfo)
    {
        StringBuilder fieldsb = new StringBuilder(256);
        fieldsb.Append("\t\t");
        switch (fieldInfo.m_type)
        {
            case FieldType.FT_Float:
                if (fieldInfo.isrepeat)
                {
                    //写入一个List<float>的代码
                    fieldsb.Append(fieldInfo.m_name).Append(" = ").Append("ProtoUtils.DeserializeFloatArray(buffer);\r\n");
                }
                else
                {
                    fieldsb.Append(fieldInfo.m_name).Append(" = ").Append("buffer.DeserializeFloat();\r\n");
                }
                break;
            case FieldType.FT_Int:
                if (fieldInfo.isrepeat)
                {
                    fieldsb.Append(fieldInfo.m_name).Append(" = ").Append("ProtoUtils.DeserializeIntArray(buffer);\r\n");
                }
                else
                {
                    fieldsb.Append(fieldInfo.m_name).Append(" = ").Append("buffer.DeserializeInt();\r\n");
                }
                break;
            case FieldType.FT_String:
                if (fieldInfo.isrepeat)
                {
                    Debug.LogError("暂时不支持");
                }
                else
                {
                    fieldsb.Append(fieldInfo.m_name).Append(" = ").Append("buffer.DeserializeString();\r\n");
                }
                break;
            case FieldType.FT_Class:
                if (fieldInfo.isrepeat)
                {
                    string fieldNameCount = "count" + fieldInfo.m_name;
                    fieldsb.Append("int ").Append(fieldNameCount).Append(" = ").Append("buffer.DeserializeInt(); \r\n");
                    fieldsb.Append("\t\tfor(int i=0;i<").Append(fieldNameCount).Append(";++i) \r\n");
                    fieldsb.Append("\t\t{\r\n");
                    string newInstName = "inst" + fieldInfo.m_name;
                    fieldsb.Append("\t\t\t").Append(fieldInfo.className).Append(" ").Append(newInstName).Append(" = ").Append("new ").Append(fieldInfo.className).Append("(); \r\n");
                    fieldsb.Append("\t\t\t").Append(newInstName).Append(".Deserialize(buffer);\r\n");
                    fieldsb.Append("\t\t\t").Append(fieldInfo.m_name).Append(".Add(").Append(newInstName).Append(");\r\n");
                    fieldsb.Append("\t\t}\r\n");
                }
                else
                {
                    fieldsb.Append(fieldInfo.m_name).Append(".Deserialize(buffer);\r\n");
                }
                break;
        }
        return fieldsb.ToString();
    }

    string GenerateField(FieldInfo fieldInfo) 
    {
        StringBuilder fieldsb = new StringBuilder(256);
        fieldsb.Append('\t');
        switch (fieldInfo.m_type)
        {
            case FieldType.FT_Float:
                if (fieldInfo.isrepeat)
                {
                    fieldsb.Append("public List<float> ").Append(fieldInfo.m_name).Append(' ').Append('=').Append("new List<float>();\r\n");
                }
                else
                {
                    fieldsb.Append("public float ").Append(fieldInfo.m_name).Append(";\r\n");
                }
                break;
            case FieldType.FT_Int:
                if (fieldInfo.isrepeat)
                {
                    fieldsb.Append("public List<int> ").Append(fieldInfo.m_name).Append(' ').Append('=').Append("new List<int>();\r\n");
                }
                else
                {
                    fieldsb.Append("public int ").Append(fieldInfo.m_name).Append(";\r\n");
                }
                break;
            case FieldType.FT_String:
                if (fieldInfo.isrepeat)
                {
                    fieldsb.Append("public List<string> ").Append(fieldInfo.m_name).Append(' ').Append('=').Append("new List<string>();\r\n");
                }
                else
                {
                    fieldsb.Append("public string ").Append(fieldInfo.m_name).Append(" = string.Empty").Append(";\r\n");
                }
                break;
            case FieldType.FT_Class:
                if (fieldInfo.isrepeat)
                {
                    fieldsb.Append("public List<").Append(fieldInfo.className).Append("> ").Append(fieldInfo.m_name).Append(' ').Append('=').Append("new List<").Append(fieldInfo.className).Append(">();\r\n");
                }
                else
                {
                    fieldsb.Append("public ").Append(fieldInfo.className).Append(' ').Append(fieldInfo.m_name).Append(" = new ").Append(fieldInfo.className).Append("();\r\n");
                }
                break;
        }
        return fieldsb.ToString();
    }
}

 
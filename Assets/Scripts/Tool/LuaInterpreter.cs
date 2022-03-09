using System.IO;
using UnityEngine;
using XLua;

public class LuaInterpreter : MonoSingletonBase<LuaInterpreter>
{
    /// <summary>
    /// LuaEnv 对象
    /// </summary>
    private LuaEnv _obj_luaEnv = null;

    /// <summary>
    /// 是否使用本地文件
    /// </summary>
    private bool _bIsUseLocalFile = false;

    /// <summary>
    /// Lua的AB包包名
    /// </summary>
    private string _sLuaABPackName = "lua.ab";

    /// <summary>
    /// Lua文件的后缀名
    /// </summary>
    private string _sLuaFileSuffix = ".lua.txt";

    void Awake()
    {
        if (_obj_luaEnv == null)
        {
            _obj_luaEnv = new LuaEnv();
            _obj_luaEnv.AddLoader(CustomLuaLoader);
        }
    }

    private void OnDestroy()
    {
        if (_obj_luaEnv != null)
        {
            _obj_luaEnv.Dispose();
            _obj_luaEnv = null;
        }
    }

    /// <summary>
    /// 执行Lua文件
    /// </summary>
    /// <param name="sLuaName"></param>
    public void RequireLua(string sLuaName)
    {
        if (_obj_luaEnv != null)
        {
            _obj_luaEnv.DoString(string.Format("require '{0}'", sLuaName));
            Debug.Log("执行lua >>>>>> " + sLuaName);
        }
    }

    private byte[] CustomLuaLoader(ref string sFilePath)
    {
#if UNITY_EDITOR && _bIsUseLocalFile
        string sLuaPath = Application.dataPath + @"/Scripts/Lua/" + sFilePath + ".lua.txt";
        Debug.Log("Editor 读取Lua: " + sLuaPath);
        
        string sAllStr = File.ReadAllText(sLuaPath);
        return System.Text.Encoding.UTF8.GetBytes(sAllStr);
#else
        //string sLuaPath = Application.persistentDataPath + @"/" + sFilePath + _sLuaFileSuffix;
        Debug.Log("调用包名是 >>>> " + _sLuaABPackName);
        TextAsset uObj_textAsset = AssetBundleMgr.GetInstance().LoadABPackRes<TextAsset>(_sLuaABPackName, sFilePath + _sLuaFileSuffix);
        return System.Text.Encoding.UTF8.GetBytes(uObj_textAsset.text);
#endif
    }
}

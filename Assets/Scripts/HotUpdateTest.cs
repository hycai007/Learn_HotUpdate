using UnityEngine;

public class HotUpdateTest : MonoSingletonBase<HotUpdateTest>
{
    void Start()
    {
        HotUpdateMgr.GetInstance().StartHotUpdate();
    }

    public void RunLua()
    {
        LuaInterpreter.GetInstance().RequireLua("HelloWorld");
        LuaInterpreter.GetInstance().RequireLua("Test");
    }

    public void InitShow()
    {
        GameObject obj_cube = AssetBundleMgr.GetInstance().LoadABPackRes<GameObject>("mode.ab", "Cube");
        Debug.Log("实例化 Cube");
    }
}

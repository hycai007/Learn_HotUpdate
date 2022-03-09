using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AssetBundleMgr : MonoSingletonBase<AssetBundleMgr>
{

    /// <summary>
    /// 是否使用本地文件
    /// </summary>
    public bool _bIsUseLocalFile = false;

    /// <summary>
    /// 是否使用加密
    /// </summary>
    public bool _bIsEncrypt = true;

    /// <summary>
    /// AB包的主包对象
    /// </summary>
    private AssetBundle _obj_mainABPack = null;
    /// <summary>
    /// AB包主包的相关依赖信息对象
    /// </summary>
    private AssetBundleManifest _obj_mainManifest = null;

    /// <summary>
    /// AB包不能重复加载，重复加载会报错
    /// 用字典的方式来储存，已经加载过的AB包
    /// </summary>
    /// <typeparam name="string"> AB包路径 </typeparam>
    /// <typeparam name="AssetBundle"> AB包对象 </typeparam>
    /// <returns></returns>
    private Dictionary<string, AssetBundle> _dict_ABObj = new Dictionary<string, AssetBundle>();

    /// <summary>
    /// AB包存放路径
    /// </summary>
    /// <value></value>
    private string _sPathUrl;

    /// <summary>
    /// 存放AB包的主文件夹，主包名 
    /// </summary>
    /// <value></value>
    private string _sMainABName {
        get {
            #if UNITY_IOS
                return "IOS";
            #elif UNITY_ANDROID
                return "Android";
            #else
                return "StandaloneWindows";
            #endif
        }   
    }

    private void Awake()
    {
        string sPlatformStr = ABPackUtils.GetABPackPathPlatformStr();
        if (_bIsUseLocalFile) // 是否从本地中读取
        {
            _sPathUrl = Application.streamingAssetsPath + sPlatformStr;
        }
        else
        {
            _sPathUrl = Application.persistentDataPath + sPlatformStr;
        }
    }

    /// <summary>
    /// 加载AB包
    /// </summary>
    /// <param name="sABName"></param>
    /// <returns></returns>
    private AssetBundle LoadPack(string sABName)
    {
        bool bIsEncryptFile = false;
        if (sABName.Contains(ABPackUtils.ABPackExName))
        {
            bIsEncryptFile = true;
        }

        if (_bIsEncrypt && bIsEncryptFile)
        {
            //Debug.Log("加载AB包文件路径 To Stream >>>> " + _sPathUrl + sABName);
            byte[] arr_abData = AESEncryptMgr.AESDecryptFileToStream(_sPathUrl + sABName);
            if (arr_abData != null)
            {
                //Debug.Log("AB包解密, 加载流 成功 >>>>>>");
                return AssetBundle.LoadFromMemory(arr_abData);
            }
            Debug.LogError("Error AB包解密, 加载流 失败 >>>>>>");
            return null;
        }
        else
        {
            return AssetBundle.LoadFromFile(_sPathUrl + sABName);
        }
    }

    #region 同时加载AB包与资源
    /// <summary>
    /// 加载某个AB包
    /// </summary>
    /// <param name="sABName"></param>
    /// <returns></returns>
    public bool LoadABPack(string sABName)
    {
        if (_obj_mainABPack == null)
        {
            _obj_mainABPack = LoadPack(_sMainABName);
            if (_obj_mainABPack == null) return false;
            _obj_mainManifest = _obj_mainABPack.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }

        // 加载sABName的所有依赖包
        AssetBundle obj_relyOnAB = null;
        string[] arr_sManifest = _obj_mainManifest.GetAllDependencies(sABName);
        for (int i = 0; i < arr_sManifest.Length; i++)
        {
            // 未加载过的才进行加载，已加载的AB包，不能重复加载
            if(!_dict_ABObj.ContainsKey(arr_sManifest[i]))
            {
                obj_relyOnAB = LoadPack(arr_sManifest[i]);
                if (obj_relyOnAB == null) return false;
                _dict_ABObj.Add(arr_sManifest[i], obj_relyOnAB);
            }
        }

        AssetBundle obj_curLoadAB = null;
        if(!_dict_ABObj.ContainsKey(sABName))
        {
            obj_curLoadAB = LoadPack(sABName);
            if (obj_curLoadAB == null) return false;
            _dict_ABObj.Add(sABName, obj_curLoadAB);
        }

        return true;
    }

    /// <summary>
    /// 同步加载AB包中的资源
    /// </summary>
    /// <param name="sABName">AB包的包名</param>
    /// <param name="sResName">AB包的资源名</param>
    /// <returns>返回资源对象，GameObject则是实例后的对象，资源一类的则直接返回，注：错误时返回null</returns>
    public Object LoadABPackRes(string sABName, string sResName)
    {
        bool bIsLoadSuc = LoadABPack(sABName);
        if (!bIsLoadSuc) return null;

        if(_dict_ABObj.ContainsKey(sABName))
        {
            Object uObj_cur = _dict_ABObj[sABName].LoadAsset(sResName);
            if(uObj_cur is GameObject)
            {
                return Instantiate(uObj_cur);
            }
            else
            {
                return uObj_cur;
            }
        }

        return null;
    }

    
    /// <summary>
    /// 同步加载AB包中的资源, 重载增加转换的参数类型
    /// </summary>
    /// <param name="sABName">AB包的包名</param>
    /// <param name="sResName">AB包的资源名</param>
    /// <param name="eToType">加载后的资源转换成的资源类型</param>
    /// <returns>返回资源对象，GameObject则是实例后的对象，资源一类的则直接返回，注：错误时返回null</returns>
    public Object LoadABPackRes(string sABName, string sResName, System.Type eToType)
    {
        bool bIsLoadSuc = LoadABPack(sABName);
        if (!bIsLoadSuc) return null;

        if (_dict_ABObj.ContainsKey(sABName))
        {
            Object uObj_cur = _dict_ABObj[sABName].LoadAsset(sResName, eToType);
            if(uObj_cur is GameObject)
            {
                return Instantiate(uObj_cur);
            }
            else
            {
                return uObj_cur;
            }
        }

        return null;
    }

    /// <summary>
    /// 同步加载AB包中的资源，重载的泛型方法
    /// </summary>
    /// <param name="sABName">AB包的包名</param>
    /// <param name="sResName">AB包的资源名</param>
    /// <returns>返回资源对象，GameObject则是实例后的对象，资源一类的则直接返回，注：错误时返回null</returns>
    public T LoadABPackRes<T>(string sABName, string sResName) where T: Object
    {
        bool bIsLoadSuc = LoadABPack(sABName);
        if (!bIsLoadSuc) return null;

        if (_dict_ABObj.ContainsKey(sABName))
        {
            T obj_cur = _dict_ABObj[sABName].LoadAsset<T>(sResName);
            if(obj_cur is GameObject)
            {
                return Instantiate(obj_cur);
            }
            else
            {
                return obj_cur;
            }
        }

        return null;
    }

#endregion

#region 同步加载AB包，异步加载资源
    /// <summary>
    /// 同步加载AB包，异步加载资源
    /// </summary>
    /// <param name="sABName">AB包的包名</param>
    /// <param name="sResName">AB包的资源名</param>
    /// <param name="fun_callback">异步加载完成后的回调方法</param>
    public void LoadResAsync(string sABName, string sResName, UnityAction<Object> fun_callback)
    {
        bool bIsLoadSuc = LoadABPack(sABName);
        if (bIsLoadSuc) return;
        StartCoroutine(IE_LoadResAsync(sABName, sResName, fun_callback));
    }
    /// <summary>
    /// 异步操作：同步加载AB包，异步加载资源
    /// </summary>
    /// <param name="sABName">AB包的包名</param>
    /// <param name="sResName">AB包的资源名</param>
    /// <param name="fun_callback">异步加载完成后的回调方法</param>
    /// <returns></returns>
    private IEnumerator IE_LoadResAsync(string sABName, string sResName, UnityAction<Object> fun_callback)
    {
        if(_dict_ABObj.ContainsKey(sABName))
        {
            AssetBundleRequest uObj_cur = _dict_ABObj[sABName].LoadAssetAsync(sResName);
            yield return uObj_cur;
            if(uObj_cur.asset is GameObject)
            {
                fun_callback(Instantiate(uObj_cur.asset));
            }
            else
            {
                fun_callback(uObj_cur.asset);
            }
        }
        else
        {
            Debug.LogError("异步加载资源失败，未找到AB包");   
        }
    }

    
    /// <summary>
    /// 同步加载AB包，异步加载资源
    /// </summary>
    /// <param name="sABName">AB包的包名</param>
    /// <param name="sResName">AB包的资源名</param>
    /// <param name="fun_callback">异步加载完成后的回调方法</param>
    public void LoadResAsync(string sABName, string sResName, System.Type eToType, UnityAction<Object> fun_callback)
    {
        bool bIsLoadSuc = LoadABPack(sABName);
        if (bIsLoadSuc) return;
        StartCoroutine(IE_LoadResAsync(sABName, sResName, eToType, fun_callback));
    }
    /// <summary>
    /// 异步操作：同步加载AB包，异步加载资源
    /// </summary>
    /// <param name="sABName">AB包的包名</param>
    /// <param name="sResName">AB包的资源名</param>
    /// <param name="fun_callback">异步加载完成后的回调方法</param>
    /// <returns></returns>
    private IEnumerator IE_LoadResAsync(string sABName, string sResName, System.Type eToType, UnityAction<Object> fun_callback)
    {
        if(_dict_ABObj.ContainsKey(sABName))
        {
            AssetBundleRequest uObj_cur = _dict_ABObj[sABName].LoadAssetAsync(sResName, eToType);
            yield return uObj_cur;
            if(uObj_cur.asset is GameObject)
            {
                fun_callback(Instantiate(uObj_cur.asset));
            }
            else
            {
                fun_callback(uObj_cur.asset);
            }
        }
        else
        {
            Debug.LogError("异步加载资源失败，未找到AB包");   
        }
    }

    /// <summary>
    /// 同步加载AB包，异步加载资源
    /// </summary>
    /// <param name="sABName">AB包的包名</param>
    /// <param name="sResName">AB包的资源名</param>
    /// <param name="fun_callback">异步加载完成后的回调方法</param>
    public void LoadResAsync<T>(string sABName, string sResName, UnityAction<T> fun_callback) where T: Object
    {
        bool bIsLoadSuc = LoadABPack(sABName);
        if (bIsLoadSuc) return;
        StartCoroutine(IE_LoadResAsync<T>(sABName, sResName, fun_callback));
    }
    /// <summary>
    /// 异步操作：同步加载AB包，异步加载资源
    /// </summary>
    /// <param name="sABName">AB包的包名</param>
    /// <param name="sResName">AB包的资源名</param>
    /// <param name="fun_callback">异步加载完成后的回调方法</param>
    /// <returns></returns>
    private IEnumerator IE_LoadResAsync<T>(string sABName, string sResName, UnityAction<T> fun_callback) where T: Object
    {
        if(_dict_ABObj.ContainsKey(sABName))
        {
            AssetBundleRequest uObj_cur = _dict_ABObj[sABName].LoadAssetAsync<T>(sResName);
            yield return uObj_cur;
            if(uObj_cur.asset is GameObject)
            {
                fun_callback(Instantiate(uObj_cur.asset) as T);
            }
            else
            {
                fun_callback(uObj_cur.asset as T);
            }
        }
        else
        {
            Debug.LogError("异步加载资源失败，未找到AB包");   
        }
    }
#endregion

#region 异步加载AB包，异步加载资源
    /// <summary>
    /// 异步加载AB包（外部调用接口）
    /// </summary>
    /// <param name="sABName">AB名称</param>
    public void AsyncLoadABPack(string sABName)
    {
        StartCoroutine(IE_AsyncLoadABPack(sABName));
    }

    /// <summary>
    /// 异步加载AB包（类内部调用用接口）
    /// </summary>
    /// <param name="sABName">AB名称</param>
    /// <returns></returns>
    private IEnumerator IE_AsyncLoadABPack(string sABName)
    {
        if (_obj_mainABPack == null)
        {
            _obj_mainABPack = LoadPack(_sMainABName);
            _obj_mainManifest = _obj_mainABPack.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            yield return _obj_mainManifest;
        }

        // 加载sABName的所有依赖包
        AssetBundle obj_relyOnAB = null;
        string[] arr_sManifest = _obj_mainManifest.GetAllDependencies(sABName);
        for (int i = 0; i < arr_sManifest.Length; i++)
        {
            // 未加载过的才进行加载，已加载的AB包，不能重复加载
            if(!_dict_ABObj.ContainsKey(arr_sManifest[i]))
            {
                obj_relyOnAB = LoadPack(arr_sManifest[i]);
                _dict_ABObj.Add(arr_sManifest[i], obj_relyOnAB);
                yield return obj_relyOnAB;
            }
        }

        AssetBundle obj_curLoadAB = null;
        if(!_dict_ABObj.ContainsKey(sABName))
        {
            {
                obj_curLoadAB = LoadPack(sABName);
                _dict_ABObj.Add(sABName, obj_curLoadAB);
            }
        }
    }

    /// <summary>
    /// 异步加载AB包与资源，只返回Object，类型外部自己传参（外部调用接口）
    /// </summary>
    /// <param name="sABName">AB包的包名</param>
    /// <param name="sResName">AB包的资源名</param>
    /// <param name="fun_callback">异步加载完成后的回调方法</param>
    public void AsyncLoadABPackAndRes(string sABName, string sResName, UnityAction<Object> fun_callback)
    {
        StartCoroutine(IE_AsyncLoadABPackAndRes(sABName, sResName, fun_callback));
    }

    /// <summary>
    /// 异步加载AB包与资源（类内部调用接口）
    /// </summary>
    /// <param name="sABName">AB包的包名</param>
    /// <param name="sResName">AB包的资源名</param>
    /// <param name="fun_callback">异步加载完成后的回调方法</param>
    /// <returns></returns>
    private IEnumerator IE_AsyncLoadABPackAndRes(string sABName, string sResName, UnityAction<Object> fun_callback)
    {
        yield return StartCoroutine(IE_AsyncLoadABPack(sABName));
        StartCoroutine(IE_LoadResAsync(sABName, sResName, fun_callback));
    }

    /// <summary>
    /// 异步加载AB包与资源，传资源转换类型（类内部调用接口）
    /// </summary>
    /// <param name="sABName">AB包的包名</param>
    /// <param name="sResName">AB包的资源名</param>
    /// <param name="eToType">资源转换类型</param>
    /// <param name="fun_callback">异步加载完成后的回调方法</param>
    public void AsyncLoadABPackAndRes(string sABName, string sResName, System.Type eToType, UnityAction<Object> fun_callback)
    {
        StartCoroutine(IE_AsyncLoadABPackAndRes(sABName, sResName, eToType, fun_callback));
    }

    /// <summary>
    /// 异步加载AB包与资源（类内部调用接口）
    /// </summary>
    /// <param name="sABName">AB包的包名</param>
    /// <param name="sResName">AB包的资源名</param>
    /// <param name="fun_callback">异步加载完成后的回调方法</param>
    /// <returns></returns>
    private IEnumerator IE_AsyncLoadABPackAndRes(string sABName, string sResName, System.Type eToType, UnityAction<Object> fun_callback)
    {
        yield return StartCoroutine(IE_AsyncLoadABPack(sABName));
        StartCoroutine(IE_LoadResAsync(sABName, sResName, eToType, fun_callback));
    }

    /// <summary>
    /// 异步加载AB包与资源，重载使用泛型数据（外部调用接口）
    /// </summary>
    /// <param name="sABName">AB包的包名</param>
    /// <param name="sResName">AB包的资源名</param>
    /// <param name="fun_callback">异步加载完成后的回调方法</param>
    /// <typeparam name="T">泛型</typeparam>
    public void AsyncLoadABPackAndRes<T>(string sABName, string sResName, UnityAction<T> fun_callback) where T: Object
    {
        StartCoroutine(IE_AsyncLoadABPackAndRes<T>(sABName, sResName, fun_callback));
    }
    
    /// <summary>
    /// 异步加载AB包与资源，重载使用泛型数据（类内部调用接口）
    /// </summary>
    /// <param name="sABName">AB包的包名</param>
    /// <param name="sResName">AB包的资源名</param>
    /// <param name="fun_callback">异步加载完成后的回调方法</param>
    /// <typeparam name="T">泛型</typeparam>
    /// <returns></returns>
    private IEnumerator IE_AsyncLoadABPackAndRes<T>(string sABName, string sResName, UnityAction<T> fun_callback) where T: Object
    {
        yield return StartCoroutine(IE_AsyncLoadABPack(sABName));
        StartCoroutine(IE_LoadResAsync<T>(sABName, sResName, fun_callback));
    }

#endregion

#region AB包资源卸载
    /// <summary>
    /// 单个AB包卸载
    /// </summary>
    /// <param name="sABName"></param>
    public void UnLoadABPack(string sABName)
    {
        if(_dict_ABObj.ContainsKey(sABName))
        {
            _dict_ABObj[sABName].Unload(false);
            _dict_ABObj.Remove(sABName);
        }
    }

    /// <summary>
    /// 卸载所有的AB包
    /// </summary>
    public void ClearAllABPack()
    {
        AssetBundle.UnloadAllAssetBundles(false);
        _obj_mainABPack = null;
        _obj_mainManifest = null;

    }
#endregion
}

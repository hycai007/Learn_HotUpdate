using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Text;

public class HotUpdateMgr : MonoSingletonBase<HotUpdateMgr>
{
    /// <summary>
    /// _sBaseUrl下载网址
    /// </summary>
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
    public static string _sBaseUrl = "http://127.0.0.1:5858";
#elif UNITY_ANDROID
    public static string _sBaseUrl = "http://192.168.255.10:5858";
#elif UNITY_IPHONE
    public static string _sBaseUrl = "http://192.168.255.10:5858";
#endif

    private string _sABVersionName = "";

    /// <summary>
    /// 本地版本信息缓存路径
    /// </summary>
    private string _sVersionLocalFilePath = "";

    /// <summary>
    /// 同时下载的最大数量
    /// </summary>
    private int _nMaxDownloader = 5;

    /// <summary>
    /// 当前需要下载的AB包数据
    /// </summary>
    List<ABPackInfo> _list_allNeedABPack = new List<ABPackInfo>();

    /// <summary>
    /// 所需下载资源总大小
    /// </summary>
    private float _nDownloadTotalSize = 0;

    /// <summary>
    /// 当前已下载资源的大小
    /// </summary>
    private float _nCurDownloadedSize = 0;

    /// <summary>
    /// AB包下载器
    /// </summary>
    private List<ABDownloader> _list_allABDownloader = new List<ABDownloader>();

    /// <summary>
    /// 客户端的AB版本数据
    /// </summary>
    private Dictionary<string, ABPackInfo> _dict_clientABInfoList = null;

    protected override void Awake()
    {
        string sPlatformStr = ABPackUtils.GetABPackPathPlatformStr();
        _sABVersionName = sPlatformStr + ABPackUtils.sABVersionName;
        _sVersionLocalFilePath = Application.persistentDataPath + _sABVersionName;
        IOUtils.CreateDirectroryOfFile(_sVersionLocalFilePath);
    }

    /// <summary>
    /// 开始热更
    /// </summary>
    public void StartHotUpdate()
    {
        Debug.Log("开始热更 >>>>>> ");
        StartCoroutine(DownloadAllABPackVersion());
    }

    /// <summary>
    /// 解析版本文件，返回一个文件列表
    /// </summary>
    /// <param name="sContent"></param>
    /// <returns></returns>
    public Dictionary<string, ABPackInfo> ConvertToAllABPackDesc(string sContent)
    {
        Dictionary<string, ABPackInfo> dict_allABPackDesc = new Dictionary<string, ABPackInfo>();
        string[] arrLines = sContent.Split('\n');//用回车 字符 \n 分割每一行
        foreach (string item in arrLines)
        {
            string[] arrData = item.Split(' ');//用空格分割每行数据的三个类型
            if (arrData.Length == 3)
            {
                ABPackInfo obj_ABPackData = new ABPackInfo();
                obj_ABPackData.sABName = arrData[0]; // 名称即路径
                obj_ABPackData.sMd5 = arrData[1]; // md5值
                obj_ABPackData.nSize = int.Parse(arrData[2]); // AB包大小

                //Debug.Log(string.Format("解析的路径：{0}\n解析的MD5：{1}\n解析的文件大小KB：{2}", obj_ABPackData.sABName, obj_ABPackData.sMd5, obj_ABPackData.nSize));
                dict_allABPackDesc.Add(obj_ABPackData.sABName, obj_ABPackData);
            }
        }

        return dict_allABPackDesc;
    }


    /// <summary>
    /// 获取服务端的AB包版本信息
    /// </summary>
    /// <returns></returns>
    IEnumerator DownloadAllABPackVersion()
    {
        string sVersionUrl = _sBaseUrl + @"/" + _sABVersionName;
        //Debug.Log("下载版本数据路径：" + sVersionUrl);

        using (UnityWebRequest uObj_versionWeb = UnityWebRequest.Get(sVersionUrl))
        {
            yield return uObj_versionWeb.SendWebRequest(); // 等待资源下载
            if (uObj_versionWeb.isNetworkError || uObj_versionWeb.isHttpError)
            {
                Debug.LogError("获取版本AB包数据错误: " + uObj_versionWeb.error);
                yield break;
            }
            else
            {
                string sVersionData = uObj_versionWeb.downloadHandler.text;
                //Debug.Log("成功获取到版本相关数据 >>>> \n" + sVersionData);
                CheckNeedDownloadABPack(sVersionData);
            }
        }
    }

    /// <summary>
    /// 检测需要下载
    /// </summary>
    /// <param name="sServerVersionData"></param>
    void CheckNeedDownloadABPack(string sServerVersionData)
    {
        //Debug.Log("运行平台：" + Application.platform);
        //Debug.Log("本地版本文件路径是：" + _sVersionLocalFilePath);

        Dictionary<string, ABPackInfo> dict_serverDownList = ConvertToAllABPackDesc(sServerVersionData); // 服务端获取的资源下载列表

        if (File.Exists(_sVersionLocalFilePath))
        {
            //Debug.Log("存在本地，对比服务器版本信息");
            string sClientVersionData = File.ReadAllText(_sVersionLocalFilePath); // 本地版本信息
            _dict_clientABInfoList = ConvertToAllABPackDesc(sClientVersionData); // 客户端本地缓存的资源下载列表

            //遍历服务器文件
            foreach (ABPackInfo obj_itemData in dict_serverDownList.Values)
            {
                // 存在对应已下载文件，对比Md5值是否一致
                if (_dict_clientABInfoList.ContainsKey(obj_itemData.sABName))
                {
                    // md5值不一致，则更新文件
                    if (_dict_clientABInfoList[obj_itemData.sABName].sMd5 != obj_itemData.sMd5)
                    {
                        _list_allNeedABPack.Add(obj_itemData);
                        _nDownloadTotalSize = _nDownloadTotalSize + obj_itemData.nSize;

                        //Debug.Log("MD5 值不一样，资源存在变更，增加文件 >>>>> " + obj_itemData.sABName);
                    }
                }
                else
                {
                    _list_allNeedABPack.Add(obj_itemData);
                    _nDownloadTotalSize = _nDownloadTotalSize + obj_itemData.nSize;
                }
            }
        }
        else // 如果说不存在本地缓存，那就直接下载所有的AB包
        {
            foreach (ABPackInfo obj_itemData in dict_serverDownList.Values)
            {
                _list_allNeedABPack.Add(obj_itemData);
                _nDownloadTotalSize = _nDownloadTotalSize + obj_itemData.nSize;
                //Debug.Log("所需下载文件 >>>>> " + obj_itemData.sABName);
            }
        }
        StartDownloadAllABPack();
    }

    /// <summary>
    /// 开始下载所有所需下载的AB包资源
    /// </summary>
    /// <param name="list_allABPack"></param>
    void StartDownloadAllABPack()
    {
        int nMaxCount = _list_allNeedABPack.Count;
        if (nMaxCount <= 0) 
        {
            HotUpdateEnd();
            return;
        }

        int nNeedCount = Mathf.Min(nMaxCount, _nMaxDownloader);
        for (int i = 0; i < nNeedCount; i++)
        {
            ABPackInfo obj_ABPackDesc = _list_allNeedABPack[0];
            ABDownloader obj_downloader = new ABDownloader();
            _list_allABDownloader.Add(obj_downloader);
            StartCoroutine(obj_downloader.DownloadABPack(obj_ABPackDesc));
            _list_allNeedABPack.RemoveAt(0);
        }
    }

    /// <summary>
    /// 切换下载下一个AB包
    /// </summary>
    /// <param name="obj_ABDownloader">需要切换的下载器</param>
    public void ChangeDownloadNextABPack(ABDownloader obj_ABDownloader)
    {
        //Debug.Log("切换下载下一个 AB 包");
        _nCurDownloadedSize += obj_ABDownloader.GetDownloadResSize();

        if (_list_allNeedABPack.Count > 0) // 还存在需要下载的资源，下载器切换资源，继续下载
        {
            StartCoroutine(obj_ABDownloader.DownloadABPack(_list_allNeedABPack[0]));
            _list_allNeedABPack.RemoveAt(0);
        }
        else
        {
            bool bIsDownloadSuc = true; // 资源是否全部下载完成
            foreach(ABDownloader obj_downloader in _list_allABDownloader)
            {
                if(obj_downloader.bIsDownloading) // 存在一个下载中，即表示当前还有未下载完成的部分
                {
                    bIsDownloadSuc = false;
                    break;
                }
            }

            if (bIsDownloadSuc) // 已完成全部下载
            {
                HotUpdateEnd();
            }
        }
    }

    /// <summary>
    /// 更新本地缓存的AB包版本数据
    /// </summary>
    /// <param name="obj_ABPackDecs"></param>
    public void UpdateClientABInfo(ABPackInfo obj_ABPackDecs)
    {
        if (_dict_clientABInfoList == null)
        {
            _dict_clientABInfoList = new Dictionary<string, ABPackInfo>();
        }

        _dict_clientABInfoList[obj_ABPackDecs.sABName] = obj_ABPackDecs;

        StringBuilder obj_sb = new StringBuilder();
        foreach (ABPackInfo obj_temp in _dict_clientABInfoList.Values)
        {
            obj_sb.AppendLine(ABPackUtils.GetABPackVersionStr(obj_temp.sABName, obj_temp.sMd5, obj_temp.nSize.ToString()));
        }

        IOUtils.CreatTextFile(_sVersionLocalFilePath, obj_sb.ToString());
    }

    /// <summary>
    /// 热更新结束，进入下一个阶段
    /// </summary>
    private void HotUpdateEnd()
    {
        // TODO 进入下一个阶段
        Debug.Log("热更新: 已完成所有的AB包下载, 进入下一个阶段 TODO");
        HotUpdateTest.GetInstance().RunLua();
        HotUpdateTest.GetInstance().InitShow();
    }
}

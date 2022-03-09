using UnityEngine;
/// <summary>
/// AB包相关处理的工具
/// </summary>
public static class ABPackUtils
{
    /// <summary>
    /// AB包的后缀扩展名
    /// </summary>
    private static string _sABPackExName = ".ab";
    public static string ABPackExName { get { return _sABPackExName; } }

    /// <summary>
    /// 缓存AB包版本信息的文件名
    /// </summary>
    private static string _sABVersionName = "ABVersionFile.txt";
    public static string sABVersionName { get { return _sABVersionName; } }

    /// <summary>
    /// 获取AB包的版本信息字符串
    /// </summary>
    /// <param name="sABName">包名（含AssetBundle路径）</param>
    /// <param name="sFileVersionMd5">版本信息的MD5值</param>
    /// <param name="nFileSize">文件大小</param>
    /// <returns></returns>
    public static string GetABPackVersionStr(string sABName, string sFileVersionMd5, string sFileSize)
    {
        return string.Format("{0} {1} {2}", sABName, sFileVersionMd5, sFileSize);
    }

    /// <summary>
    /// 获取不同平台AB包存放路径的字符串
    /// </summary>
    /// <returns></returns>
    public static string GetABPackPathPlatformStr()
    {
        RuntimePlatform obj_platform = Application.platform;
        string sPlatformStr = "/AssetBundles/";
        if (obj_platform == RuntimePlatform.WindowsEditor || obj_platform == RuntimePlatform.WindowsPlayer)
        {
            sPlatformStr += "StandaloneWindows/";
        }
        else if (obj_platform == RuntimePlatform.Android)
        {
            sPlatformStr += "Android/";
        }
        else if (obj_platform == RuntimePlatform.IPhonePlayer)
        {
            sPlatformStr += "iOS/";
        }

        return sPlatformStr;
    }
}

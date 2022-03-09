using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ABPackMenu : Editor
{
    [MenuItem("My Tool/AB包加密/创建AB包版本文件/Window 版本")]
    private static void EncrypABPackVersionFile_Window()
    {
        Debug.Log("加密并创建 Window 平台的AB包版本信息");
        EncryptAndCreateVersionFile(BuildTarget.StandaloneWindows);
    }

    [MenuItem("My Tool/AB包加密/解密AB包版本文件/Window 版本")]
    private static void DecryptABPack_Window()
    {
        Debug.Log("解密 Window 平台的AB包版本信息");
        DecryptVersionFile(BuildTarget.StandaloneWindows);
    }

    [MenuItem("My Tool/AB包加密/创建AB包版本文件/Android 版本")]
    private static void CreateABPackVersionFile_Android()
    {
        Debug.Log("加密并创建 Android 平台的AB包版本信息");
        EncryptAndCreateVersionFile(BuildTarget.Android);
    }

    [MenuItem("My Tool/AB包加密/解密AB包版本文件/Android 版本")]
    private static void DecryptABPack_Android()
    {
        Debug.Log("解密 Android 平台的AB包版本信息");
        DecryptVersionFile(BuildTarget.Android);
    }

    [MenuItem("My Tool/AB包加密/创建AB包版本文件/IOS 版本")]
    private static void CreateABPackVersionFile_IOS()
    {
        Debug.Log("加密并创建 IOS 平台的AB包版本信息");
        DecryptVersionFile(BuildTarget.iOS);
    }

    [MenuItem("My Tool/AB包加密/解密AB包版本文件/IOS 版本")]
    private static void DecryptABPack_IOS()
    {
        Debug.Log("解密 IOS 平台的AB包版本信息");
        DecryptVersionFile(BuildTarget.iOS);
    }

    private static void EncryptAndCreateVersionFile(BuildTarget e_buildTarget)
    {
        /// ABPack版本信息保存路径
        Debug.Log("平台是： " + e_buildTarget.ToString());

        /// 加密文件存放路径
        string sBasePath = Application.dataPath + @"/../AssetBundlesEncrypt/" + e_buildTarget.ToString() + @"/";
        if (!Directory.Exists(sBasePath))
        {
            Directory.CreateDirectory(sBasePath);
        }

        StringBuilder obj_sb = new StringBuilder();
        string sAllABPath = Application.dataPath + @"/../AssetBundles/" + e_buildTarget.ToString();
        DirectoryInfo obj_folder = new DirectoryInfo(sAllABPath); // 获取输出路径的文件夹管理器
        FileInfo[] arr_allFiles = obj_folder.GetFiles("*", SearchOption.AllDirectories); // 取得所有文件
        foreach (FileInfo obj_item in arr_allFiles)
        {
            string sFilePath = obj_item.FullName; // 获取文件全名（包含路径 C:/ D:/ 全路径）
            string sFileName = obj_item.Name;   // 
            //Debug.Log("AB包 全路径 >>>>> " + sFilePath);

            string sExName = sFilePath.Substring(sFilePath.LastIndexOf(".") + 1); // 得到后缀名
            // 加密后的AB包存放路径
            string sEncryptABOutPath = sBasePath + sFileName;
            if (ABPackUtils.ABPackExName.IndexOf(sExName) > -1) // 匹配AB包的后缀名，取得对应的AB包文件
            {
                AESEncryptMgr.AESEncryptFile(sFilePath, sEncryptABOutPath);
            }
            else
            {
                // 不用加密的文件，拷贝的加密后的对应目录中
                bool bIsReWrite = true; // true=覆盖已存在的同名文件, false 则反之
                System.IO.File.Copy(sFilePath, sEncryptABOutPath, bIsReWrite);
            }

            string sABName = sFilePath.Substring(sFilePath.IndexOf("AssetBundles"));
            string sFileVersion = MD5Mgr.GetABPackEncryptVersion(sEncryptABOutPath);
            //Debug.Log("加密AB包的版本字符串是 >>>>> " + sFileVersion);
            if (sFileVersion == null)
            {
                Debug.LogError("有文件没有拿到MD5：" + sABName);
                continue;
            }
            string sFileSize = Mathf.Ceil(obj_item.Length / 1024f).ToString();//文件大小
            sABName = sABName.Replace("\\", "/");
            string sABVersionStr = ABPackUtils.GetABPackVersionStr(sABName, sFileVersion, sFileSize); //每个文件的版本信息
            obj_sb.AppendLine(sABVersionStr); // 写入版本文件要构建的内容中,按行写入
        }

        string sABVersionFile = sBasePath + ABPackUtils.sABVersionName;
        //Debug.Log("AB包版本信息文件保存路径是 >>> " + sABVersionFile);
        // 判断是否存在AB包的版本文件信息，存在则删除
        IOUtils.CreatTextFile(sABVersionFile, obj_sb.ToString());
    }

    /// <summary>
    /// 解密AB包文件
    /// </summary>
    /// <param name="e_buildTarget"></param>
    private static void DecryptVersionFile(BuildTarget e_buildTarget)
    {
        /// ABPack版本信息保存路径
        Debug.Log("平台是： " + e_buildTarget.ToString());
        string sAllABFile = Application.dataPath + @"/../AssetBundlesEncrypt/" + e_buildTarget.ToString();
        Debug.Log("加密版本的AB包文件路径是 >>> " + sAllABFile);

        DirectoryInfo obj_folder = new DirectoryInfo(sAllABFile); // 获取输出路径的文件夹管理器
        FileInfo[] arr_allFiles = obj_folder.GetFiles("*", SearchOption.AllDirectories); // 取得所有文件

        /// 解密文件存放路径
        string sBasePath = Application.dataPath + @"/../AssetBundlesDecrypt/" + e_buildTarget.ToString() + @"/";
        if (!Directory.Exists(sBasePath))
        {
            Directory.CreateDirectory(sBasePath);
        }

        foreach (FileInfo obj_item in arr_allFiles)
        {
            string sFilePath = obj_item.FullName; // 获取文件全名（包含路径 C:/ D:/ 全路径）
            Debug.Log("AB包 全路径 >>>>> " + sFilePath);

            string sExName = sFilePath.Substring(sFilePath.LastIndexOf(".") + 1);//得到后缀名
            // 加密后的AB包存放路径
            string sDecryptABOutPath = sBasePath + obj_item.Name;
            if (ABPackUtils.ABPackExName.IndexOf(sExName) > -1) // 匹配AB包的后缀名，取得对应的AB包文件
            {
                // 加密后的AB包存放路径
                AESEncryptMgr.AESDecryptFile(sFilePath, sDecryptABOutPath);
            }
            else
            {
                // 不用加密的文件，拷贝的加密后的对应目录中
                bool bIsReWrite = true; // true=覆盖已存在的同名文件, false 则反之
                System.IO.File.Copy(sFilePath, sDecryptABOutPath, bIsReWrite);
            }
        }
    }
}

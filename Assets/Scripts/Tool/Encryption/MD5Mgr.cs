using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class MD5Mgr
{
    /// <summary>
    /// 获取加密后的AB包内容
    /// </summary>
    /// <param name="sFilePath">AB包文件路径</param>
    /// <returns></returns>
    public static string GetABPackEncryptVersion(string sFilePath)
    {
        try
        {
            FileStream obj_fileStream = new FileStream(sFilePath, System.IO.FileMode.Open);
            MD5 obj_md5 = new MD5CryptoServiceProvider();
            byte[] retVal = obj_md5.ComputeHash(obj_fileStream);
            obj_fileStream.Close();
            StringBuilder obj_sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                obj_sb.Append(retVal[i].ToString("x2"));
            }
            return obj_sb.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
        }
    }
}

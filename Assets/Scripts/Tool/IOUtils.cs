﻿using System.IO;

public class IOUtils
{
    /// <summary>
    /// 创建txt文件的方法
    /// </summary>
    /// <param name="sFilePath"></param>
    /// <param name="sContent"></param>
    public static void CreatTextFile(string sFilePath, string sContent)
    {
        //文件存在则删除
        if (File.Exists(sFilePath))
        {
            File.Delete(sFilePath);
        }
        using (FileStream obj_versionStream = File.Create(sFilePath))
        {
            using (StreamWriter obj_writer = new StreamWriter(obj_versionStream))
            {
                obj_writer.WriteLine(sContent);
            }
        }
    }

    /// <summary>
    /// 根据文件路径，创建其文件的文件夹路径
    /// </summary>
    /// <param name="sFilePath"></param>
    public static void CreateDirectroryOfFile(string sFilePath)
    {
        //Debug.Log($"根据文件创建对应的文件夹路径 文件 >>>> {sFilePath}");
        if (!string.IsNullOrEmpty(sFilePath))
        {
            string sDirName = Path.GetDirectoryName(sFilePath);
            if (!Directory.Exists(sDirName))
            {
                //Debug.Log($"不存在路径 {sDirName},");
                Directory.CreateDirectory(sDirName);
            }
            //else
            //{
            //    Debug.Log($"已存在路径 >>>> {sDirName},");
            //}
        }
    }
}

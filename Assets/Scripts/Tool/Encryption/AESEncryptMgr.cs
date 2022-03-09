using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

public class AESEncryptMgr : SingletonBase<AESEncryptMgr>
{
    /// <summary>
    /// 缓存加密使用的加密相关数据对象
    /// </summary>
    private static AesCryptoServiceProvider _obj_AesCSP;

    /// <summary>
    /// 加密使用的 Key
    /// </summary>
    private const string _sKey = "hycai12365479807";
    /// <summary>
    /// 加密使用的 IV
    /// </summary>
    private const string _sIV = "987465132hycai07";

    /// <summary>
    /// 加密文件
    /// </summary>
    /// <param name="sFilePath"></param>
    /// <param name="sOutSavePath"></param>
    /// <param name="sKey"></param>
    /// <param name="sIV"></param>
    /// <returns></returns>
    public static int AESEncryptFile(string sFilePath, string sOutSavePath, string sKey = _sKey, string sIV = _sIV)
    {
        if (!File.Exists(sFilePath))
        {
            Debug.Log("Error: 不存在AES加密的文件，sFilePath = null ！！！");
            return -1;
        }

        int nKeyCount = sKey.Length;
        int nIvCount = sIV.Length;
        if (nKeyCount < 7 || nKeyCount > 16 || nIvCount < 7 || nIvCount > 16)
        {
            Debug.Log("AES错误，秘钥sKey与sIV偏移量长度必须是8到16位");
            return -1;
        }

        AesCryptoServiceProvider obj_aesCSP = CreateAES_CSP(sKey, sIV);
        ICryptoTransform obj_trans = obj_aesCSP.CreateEncryptor();
        if (AESFileCoding(sFilePath, sOutSavePath, obj_trans) == -1)
        {
            return -1;
        }

        return 1;
    }

    /// <summary>
    /// 解密文件
    /// </summary>
    /// <param name="sFilePath"></param>
    /// <param name="sOutSavePath"></param>
    /// <param name="sKey"></param>
    /// <param name="sIV"></param>
    /// <returns></returns>
    public static int AESDecryptFile(string sFilePath, string sOutSavePath, string sKey = _sKey, string sIV = _sIV)
    {
        if (!File.Exists(sFilePath))
        {
            Debug.Log("Error: 不存在AES解密的文件，sFilePath = null ！！！");
            return -1;
        }

        int nKeyCount = sKey.Length;
        int nIvCount = sIV.Length;
        if (nKeyCount < 7 || nKeyCount > 16 || nIvCount < 7 || nIvCount > 16)
        {
            Debug.Log("AES错误，秘钥sKey与sIV偏移量长度必须是8到16位");
            return -1;
        }

        AesCryptoServiceProvider obj_aesCSP = CreateAES_CSP(sKey, sIV);
        ICryptoTransform obj_trans = obj_aesCSP.CreateDecryptor();
        if (AESFileCoding(sFilePath, sOutSavePath, obj_trans) == -1)
        {
            return -1;
        }
        return 1;
    }

    /// <summary>
    /// 解密文件为Stream数据流
    /// </summary>
    /// <param name="sFilePath"></param>
    /// <param name="sOutSavePath"></param>
    /// <param name="sKey"></param>
    /// <param name="sIV"></param>
    /// <returns></returns>
    public static byte[] AESDecryptFileToStream(string sFilePath, string sKey = _sKey, string sIV = _sIV)
    {
        if (!File.Exists(sFilePath))
        {
            Debug.Log("Error: 不存在AES解密的文件，sFilePath = null ！！！");
            return null;
        }


        int nKeyCount = sKey.Length;
        int nIvCount = sIV.Length;
        if (nKeyCount < 7 || nKeyCount > 16 || nIvCount < 7 || nIvCount > 16)
        {
            Debug.Log("AES错误，秘钥sKey与sIV偏移量长度必须是8到16位");
            return null;
        }

        AesCryptoServiceProvider obj_aesCSP = CreateAES_CSP(sKey, sIV);
        ICryptoTransform obj_trans = obj_aesCSP.CreateDecryptor();
        return AESFileCodingToStream(sFilePath, obj_trans);
    }

    /// <summary>
    /// 根据传入的 key 与 算法的向量 IV 创建 AES 的数据对象
    /// </summary>
    /// <param name="sKey"></param>
    /// <param name="sIV"></param>
    /// <returns></returns>
    public static AesCryptoServiceProvider CreateAES_CSP(string sKey, string sIV)
    {
        if (_obj_AesCSP != null)
        {
            return _obj_AesCSP;
        }

        byte[] arr_keySource = System.Text.Encoding.ASCII.GetBytes(sKey);
        byte[] arr_ivSource = System.Text.Encoding.ASCII.GetBytes(sIV);
        int nKeySounceLen = arr_keySource.Length;
        int nIVSounceLen = arr_ivSource.Length;

        byte[] arr_key = new byte[16];
        byte[] arr_iv = new byte[16];
        int nKeyTargetLen = arr_key.Length;
        int nIVTargetLen = arr_iv.Length;

        // 确保加密Key与IV是在16 byte 内
        nKeyTargetLen = nKeySounceLen > nKeyTargetLen ? nKeyTargetLen : nKeySounceLen;
        nIVTargetLen = nIVSounceLen > nIVTargetLen ? nIVTargetLen : nIVSounceLen;
        System.Array.Copy(arr_keySource, arr_key, nKeyTargetLen);
        System.Array.Copy(arr_ivSource, arr_iv, nIVTargetLen);

        //string sShowKey = System.Text.Encoding.Default.GetString(arr_key);
        //Debug.Log("输出 >>>> sShowKey >>>>> " + sShowKey);
        //string sShowIV = System.Text.Encoding.Default.GetString(arr_iv);
        //Debug.Log("输出 >>>> sShowIV >>>>>> " + sShowIV);

        // 创建对称 AES 加密的数据模式对象
        _obj_AesCSP = new AesCryptoServiceProvider()
        {
            Mode = CipherMode.CBC,          // 设置对称算法的运算模式
            Padding = PaddingMode.PKCS7,    // 设置对称算法中使用的填充模式
            KeySize = 128,                  // 设置密钥的大小（以位为单位）
            BlockSize = 128,                // 设置加密操作的块大小（以位为单位）
            Key = arr_key,                  // 设置用于加密和解密的对称密钥
            IV = arr_iv,                    // 设置对称算法的初始化向量 (IV)
        };

        return _obj_AesCSP;
    }

    /// <summary>
    /// 将加密解密文件转换并保存到 sOutSavePath 路径下
    /// </summary>
    /// <param name="sFilePath"></param>
    /// <param name="sOutSavePath"></param>
    /// <param name="obj_trans"></param>
    /// <returns></returns>
    private static int AESFileCoding(string sFilePath, string sOutSavePath, ICryptoTransform obj_trans)
    {
        try
        {
            FileStream obj_fileStream; // 创建文件流
            byte[] arr_input = null;
            using (MemoryStream obj_memory = new MemoryStream())
            {
                // 创建加密解密流，从
                using (CryptoStream obj_cryptoStream = new CryptoStream(obj_memory, obj_trans, CryptoStreamMode.Write))
                {
                    obj_fileStream = File.OpenRead(sFilePath); // 从文件中读取数据到文件流中
                    // 将文件流转换成2进制数据
                    using (BinaryReader obj_binaryReader = new BinaryReader(obj_fileStream))
                    {
                        arr_input = new byte[obj_fileStream.Length];
                        obj_binaryReader.Read(arr_input, 0, arr_input.Length);
                    }
                    // 解密操作
                    obj_cryptoStream.Write(arr_input, 0, arr_input.Length);
                    // 释放解密操作
                    obj_cryptoStream.FlushFinalBlock();
                    // 写入到保存文件中
                    using (obj_fileStream = File.OpenWrite(sOutSavePath))
                    {
                        obj_memory.WriteTo(obj_fileStream);
                    }
                }
            }
            Debug.Log("AES文件转换成功...");
            return 1;
        }
        catch(Exception obj_ex)
        {
            Debug.Log("Error AES 加密失败  " + obj_ex.Message);
            return -1;
        }
    }

    /// <summary>
    /// 将加密解密文件转换并保存到 sOutSavePath 路径下
    /// </summary>
    /// <param name="sFilePath"></param>
    /// <param name="sOutSavePath"></param>
    /// <param name="obj_trans"></param>
    /// <returns></returns>
    private static byte[] AESFileCodingToStream(string sFilePath, ICryptoTransform obj_trans)
    {
        try
        {
            FileStream obj_fileStream; // 创建文件流
            byte[] arr_input = null;
            byte[] arr_data = null;
            using (MemoryStream obj_memory = new MemoryStream())
            {
                // 创建加密解密流，从文件中获取解密到其中
                using (CryptoStream obj_cryptoStream = new CryptoStream(obj_memory, obj_trans, CryptoStreamMode.Write))
                {
                    obj_fileStream = File.OpenRead(sFilePath); // 从文件中读取数据到文件流中
                    // 将文件流转换成2进制数据
                    using (BinaryReader obj_binaryReader = new BinaryReader(obj_fileStream))
                    {
                        arr_input = new byte[obj_fileStream.Length];
                        obj_binaryReader.Read(arr_input, 0, arr_input.Length);
                    }

                    obj_cryptoStream.Write(arr_input, 0, arr_input.Length);
                    // 释放解密操作
                    obj_cryptoStream.FlushFinalBlock();
                    //obj_memory.WriteTo(obj_fileStream);
                    arr_data = obj_memory.ToArray();
                }
                Debug.Log("AES文件解密到Stream完成...");
                return arr_data;
            }
        }
        catch (Exception obj_ex)
        {
            Debug.Log("Error AES 加密失败  " + obj_ex.Message);
            return null;
        }
    }
}



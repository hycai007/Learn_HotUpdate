using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AB包资源信息
/// </summary>
public class ABPackInfo
{
    /// <summary>
    /// 资源名称
    /// </summary>
    private string _sABName;
    public string sABName { get => _sABName; set => _sABName = value; }

    /// <summary>
    /// 版本md5值
    /// </summary>
    private string _sMd5;
    public string sMd5 { get => _sMd5; set => _sMd5 = value; }

    /// <summary>
    /// 文件大小
    /// </summary>
    private int _nSize;
    public int nSize { get => _nSize; set => _nSize = value; }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UFrame.DressBundle;
using UFrame.DressBundle.Editors;

public class BuildAB
{
    [MenuItem("Tools/OpenCacheFolder")]
    public static void OpenCacheFolder()
    {
        Application.OpenURL(new System.Uri(Application.persistentDataPath).AbsoluteUri);
    }
    [MenuItem("Tools/BuildAssetBundle")]
    public static void BuildAssetBundle()
    {
        var defineObj = AddressDefineObjectSetting.Instance.activeAddressDefineObject;
        if (defineObj)
        {
            AddressABBuilder.AutoBuildAddressDefine(ScriptableObject.Instantiate(defineObj), $"Assets/StreamingAssets/AssetBundle");
        }
    }
}

//*************************************************************************************
//* 作    者： zht
//* 创建时间： 2023-05-24
//* 描    述：

//* ************************************************************************************
using System;
using System.Collections.Generic;
using UFrame.DressBundle;
using UnityEngine;
using UnityEngine.Networking;

public class AssetBundleManager:MonoBehaviour
{
    private static AssetBundleManager _instance;
    public static AssetBundleManager Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = new GameObject("AssetBundleManager").AddComponent<AssetBundleManager>();
                GameObject.DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }

    private Dictionary<string, AssetBundleLoadCtrl> m_abLoadCtrls;
    private Dictionary<RuntimePlatform, string> m_platformMap;
    private HashSet<string> m_loaded;

    protected AssetBundleManager()
    {
        m_loaded = new HashSet<string>();
        m_abLoadCtrls = new Dictionary<string, AssetBundleLoadCtrl>();
        m_platformMap = new Dictionary<RuntimePlatform, string>() {
            { RuntimePlatform.OSXEditor, "StandaloneOSX"},
            { RuntimePlatform.WindowsEditor, "StandaloneWindows64"},
            { RuntimePlatform.Android, "Android"},
            { RuntimePlatform.IPhonePlayer, "iOS"},
        };
    }

    public AsyncCatlogOperation LoadCatalog(string module, string rootUrl, Action<bool> callback)
    {
        if (!m_abLoadCtrls.TryGetValue(module, out var abCtrl))
        {
            abCtrl = new AssetBundleLoadCtrl(OnDownloadFile, OnDownloadText, $"{Application.persistentDataPath}/HotResDir/{module}");
            var catlogUrl = $"{rootUrl}/{module}";
            var operation = abCtrl.LoadCatlogAsync(catlogUrl);
            if (callback != null)
            {
                operation.RegistComplete(x => callback?.Invoke(!string.IsNullOrEmpty(x.catlogPath)));
            }
            m_abLoadCtrls[module] = abCtrl;
            return operation;
        }
        return null;
    }

    public AsyncAssetOperation<T> LoadAssetAsync<T>(string address, Action<T> callback = null) where T : UnityEngine.Object
    {
        foreach (var abCtrl in m_abLoadCtrls.Values)
        {
            if (abCtrl.ExistAddress(address))
            {
                var operation = abCtrl.LoadAssetAsync<T>(address);
                if(operation != null)
                {
                    operation.RegistComplete(callback);
                    return operation;
                }
            }
        }
        foreach (var abCtrl in m_abLoadCtrls.Values)
        {
            if (abCtrl.TryGetAddressGroup(address,out string addressGroup,out string assetName))
            {
                var operation = abCtrl.LoadAssetAsync<T>(addressGroup, assetName);
                if (operation != null)
                {
                    operation.RegistComplete(callback);
                    return operation;
                }
            }
        }
        callback?.Invoke(null);
        Debug.LogError("no module exist address:" + address);
        return null;
    }
    public bool ExistAddress(string address)
    {
        foreach (var abCtrl in m_abLoadCtrls.Values)
        {
            if (abCtrl.ExistAddress(address))
            {
                return true;
            }
            else if (abCtrl.TryGetAddressGroup(address, out string addressGroup, out string assetName))
            {
                return true;
            }
        }
        return false;
    }

    public AsyncAssetOperation<T> LoadAssetAsync<T>(string address, string assetName, Action<T> callback = null) where T : UnityEngine.Object
    {
        foreach (var abCtrl in m_abLoadCtrls.Values)
        {
            if (abCtrl.ExistAddress(address))
            {
                var operation = abCtrl.LoadAssetAsync<T>(address, assetName);
                operation.RegistComplete(callback);
                return operation;
            }
        }
        foreach (var abCtrl in m_abLoadCtrls.Values)
        {
            if (abCtrl.TryGetAddressGroup(address, out string addressGroup, out string groupAssetName) && assetName == groupAssetName)
            {
                var operation = abCtrl.LoadAssetAsync<T>(addressGroup, assetName);
                if (operation != null)
                {
                    operation.RegistComplete(callback);
                    return operation;
                }
            }
        }
        Debug.LogError("no module exist:" + address);
        return null;
    }

    public AsyncAssetsOperation<T> LoadAssetsAsync<T>(string address, Action<T[]> callback = null) where T : UnityEngine.Object
    {
        foreach (var abCtrl in m_abLoadCtrls.Values)
        {
            if (abCtrl.ExistAddress(address))
            {
                var operation = abCtrl.LoadAssetsAsync<T>(address);
                operation.RegistComplete(callback);
                return operation;
            }
        }
        return null;
    }

    public AsyncSceneOperation LoadSceneAsync(string address, UnityEngine.SceneManagement.LoadSceneMode mode, Action<AsyncSceneOperation> callback = null)
    {
        foreach (var abCtrl in m_abLoadCtrls.Values)
        {
            if (abCtrl.ExistAddress(address))
            {
                var operaiton = abCtrl.LoadSceneAsync(address, loadSceneMode: mode);
                operaiton.RegistComplete(callback);
                return operaiton;
            }
        }
        return null;
    }

    public AsyncSceneOperation LoadSceneAsync(string address, UnityEngine.SceneManagement.LoadSceneMode mode, Action<AsyncOperation> callback = null)
    {
        foreach (var abCtrl in m_abLoadCtrls.Values)
        {
            if (abCtrl.ExistAddress(address))
            {
                var operaiton = abCtrl.LoadSceneAsync(address, loadSceneMode: mode);
                operaiton.RegistSceneLoadComplete(callback);
                return operaiton;
            }
        }
        return null;
    }

    private void OnDownloadText(string url, Action<string,object> onDownloadFinish,object content)
    {
        var www = UnityEngine.Networking.UnityWebRequest.Get(url);
        var handle = www.SendWebRequest();
        handle.completed += (x) => {
            onDownloadFinish?.Invoke(www.downloadHandler.text, content);
        };
    }

    private void OnDownloadFile(string url, string localPath, Action<string,object> onDownloadFinish,Action<float> onProgress,object content)
    {
        if (System.IO.File.Exists(localPath))
            System.IO.File.Delete(localPath);
        var www = UnityEngine.Networking.UnityWebRequest.Get(url);
        www.downloadHandler = new UnityEngine.Networking.DownloadHandlerFile(localPath);
        var handle = www.SendWebRequest();
        handle.completed += (x) =>
        {
            onDownloadFinish?.Invoke(localPath, content);
        };
    }
}



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoBehaviour : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var rootUrl = $"file:///{Application.streamingAssetsPath}";
        AssetBundleManager.Instance.LoadCatalog("AssetBundle", rootUrl, (OnLoadCatlogFinish));
    }

    void OnLoadCatlogFinish(bool ok)
    {
        var handle = AssetBundleManager.Instance.LoadAssetAsync<GameObject>("cube_address");
        handle.RegistComplete(x => {
            Instantiate(x);
        });
    }
}

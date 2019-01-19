﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class Initialization : MonoSingleton<Initialization>
{
    public bool IsABMode = false;
    [SerializeField] private GameObject Manager;

    private Initialization()
    {
    }

    void Awake()
    {
#if !UNITY_EDITOR
            IsABMode = true;
#endif

        if (IsABMode)
        {
            LoadShaders_AB();
            LoadSpriteAtlas_AB();
            LoadAudios_AB();
            LoadManager_AB();
        }
        else
        {
            LoadSpriteAtlas_Editor();
            LoadManager_Editor();
        }

        Debug.Log("Start the client...");
    }

    private void LoadManager_AB()
    {
        AssetBundle manager_bundle = ABManager.LoadAssetBundle("manager");
        GameObject manager = manager_bundle.LoadAsset<GameObject>("Assets/Prefabs/Manager.prefab");
        Instantiate(manager);
        Debug.Log("LoadManager_AB");
    }

    private void LoadManager_Editor()
    {
        Instantiate(Manager);
    }

    private void LoadShaders_AB()
    {
        AssetBundle ab = ABManager.LoadAssetBundle("shaders");
        ab.LoadAllAssets();
        Shader.WarmupAllShaders();
        Debug.Log("LoadShaders_AB");
    }

    private void LoadSpriteAtlas_AB()
    {
        List<AssetBundle> list = ABManager.LoadAllAssetBundleNamedLike("atlas_");
        foreach (AssetBundle assetBundle in list)
        {
            SpriteAtlas[] sas = assetBundle.LoadAllAssets<SpriteAtlas>();
            foreach (SpriteAtlas sp in sas)
            {
                if (!AtlasManager.SpriteAtlasDict.ContainsKey(sp.name))
                {
                    AtlasManager.SpriteAtlasDict.Add(sp.name, sp);
                }
            }
        }

        Debug.Log("LoadSpriteAtlas_AB");
    }

    private void LoadSpriteAtlas_Editor()
    {
        SpriteAtlas[] sas = Resources.LoadAll<SpriteAtlas>("SpriteAtlas");
        foreach (SpriteAtlas sa in sas)
        {
            if (!AtlasManager.SpriteAtlasDict.ContainsKey(sa.name))
            {
                AtlasManager.SpriteAtlasDict.Add(sa.name.Split('.')[0], sa);
            }
        }

        Debug.Log("LoadSpriteAtlas_Editor");
    }

    private void LoadAudios_AB()
    {
        List<AssetBundle> list = ABManager.LoadAllAssetBundleNamedLike("audios_");
        foreach (AssetBundle assetBundle in list)
        {
            string prefix = "";
            if (assetBundle.name.StartsWith("audios_sfx"))
            {
                prefix = "sfx/";
            }
            else if (assetBundle.name.StartsWith("audios_bgm"))
            {
                prefix = "bgm/";
            }

            AudioClip[] audioClips = assetBundle.LoadAllAssets<AudioClip>();
            foreach (AudioClip audioClip in audioClips)
            {
                AudioManager.AudioClipDict_ABModeOnly.Add(prefix + audioClip.name, audioClip);
            }
        }

        Debug.Log("LoadAudios_AB");
    }
}
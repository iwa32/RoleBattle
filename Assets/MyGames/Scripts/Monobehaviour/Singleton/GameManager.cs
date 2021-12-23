using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static InitializationData;
using static PlayerPrefsKey;

public class GameManager : MonoBehaviour
{
    [HideInInspector]
    public static GameManager _instance;

    [SerializeField]
    [Header("キャラクターリストのスクリプタブルオブジェクトを設定")]
    SelectableCharacterList _selectableCharacterList;

    [SerializeField]
    [Header("SEリストのスクリプタブルオブジェクトを設定")]
    SEList _seList;


    AudioSource _seSudioSource;
    AudioSource _bgmSudioSource;
    float _seVolume = 1.0f;
    float _bgmVolume = 1.0f;

    public SelectableCharacterList SelectableCharacterList => _selectableCharacterList;
    public float SEVolume => _seVolume;
    public float BgmVolume => _bgmVolume;

    private void Awake()
    {
        //シングルトン化する
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            _seSudioSource = gameObject.AddComponent<AudioSource>();
            _bgmSudioSource = gameObject.AddComponent<AudioSource>();
            SetAudioVolume();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// シーンを読み込みます
    /// </summary>
    /// <param name="scene"></param>
    public void ClickToLoadScene(SceneType scene)
    {
        SceneManager.LoadScene(CommonAttribute.GetStringValue(scene));
    }

    /// <summary>
    /// 音量を設定します
    /// </summary>
    public void SetAudioVolume()
    {
        if (PlayerPrefs.HasKey(SE_VOLUME))
            _seVolume = PlayerPrefs.GetFloat(SE_VOLUME);

        if (PlayerPrefs.HasKey(BGM_VOLUME))
            _bgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME);
    }

    #region// seを再生します
    public void PlaySE(SEType seType)
    {
        AudioClip seClip = _seList.FindSEClipByType(seType);
        PlayerOneShotForSE(seClip, _seVolume);
    }

    public void PlaySE(SEType seType, float volume)
    {
        AudioClip seClip = _seList.FindSEClipByType(seType);
        PlayerOneShotForSE(seClip, volume);
    }

    void PlayerOneShotForSE(AudioClip seClip, float volume)
    {
        if (seClip == null) return;
        _seSudioSource.PlayOneShot(seClip, volume);
    }
    #endregion

    /// <summary>
    /// プレイヤー名を取得します
    /// </summary>
    /// <returns></returns>
    public string GetPlayerName()
    {
        if (PlayerPrefs.HasKey(PLAYER_NAME))
            return PlayerPrefs.GetString(PLAYER_NAME);

        return PLAYER_NAME_FOR_UNEDITED_PLAYER;
    }

    /// <summary>
    /// プレイヤーのキャラクターを取得します
    /// </summary>
    /// <returns></returns>
    public SelectableCharacter GetPlayerCharacter()
    {
        int searchId;
        if (PlayerPrefs.HasKey(SELECTED_CHARACTER_ID))
            searchId = PlayerPrefs.GetInt(SELECTED_CHARACTER_ID);
        else
            searchId = CHARACTER_ID_FOR_UNSELECTED_PLAYER;//未選択時はフェンサーのidを指定する

        return _selectableCharacterList.FindCharacterById(searchId);
    }

    /// <summary>
    /// idからキャラクターを取得します
    /// </summary>
    /// <param name="characterId"></param>
    /// <returns></returns>
    public SelectableCharacter FindCharacterById(int characterId)
    {
        return _selectableCharacterList.FindCharacterById(characterId);
    }

    /// <summary>
    /// ランダムなプレイヤーのキャラクターを取得します
    /// </summary>
    /// <returns></returns>
    public SelectableCharacter GetRandomPlayerCharacter()
    {
        return _selectableCharacterList.GetRandomPlayerCharacter();
    }
}
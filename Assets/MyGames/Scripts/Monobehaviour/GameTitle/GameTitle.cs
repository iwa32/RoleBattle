using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SEType;
using static SceneType;

public class GameTitle : MonoBehaviour
{
    [SerializeField]
    [Header("OptionCanvasを設定")]
    GameObject _optionCanvas;

    bool _isFirstClick;

    void Start()
    {
        //最初はオプションをオフにする
        _optionCanvas.SetActive(false);
    }

    /// <summary>
    /// CPUバトルを開始する
    /// </summary>
    public void OnClickToStartCpuBattle()
    {
        if (_isFirstClick) return;
        _isFirstClick = true;
        GameManager._instance.PlaySE(BATTLE);
        GameManager._instance.ClickToLoadScene(Battle);
    }

    /// <summary>
    /// マルチバトルを開始する
    /// </summary>
    public void OnClickToStartMultiBattle()
    {
        if (_isFirstClick) return;
        _isFirstClick = true;
        GameManager._instance.PlaySE(BATTLE);
        GameManager._instance.ClickToLoadScene(MultiBattle);
    }

    /// <summary>
    /// オプション画面を表示する
    /// </summary>
    public void OnClickToShowOptionWindow()
    {
        GameManager._instance.PlaySE(OPTION_CLICK);
        CanvasForObjectPool._instance.ToggleUIGameObject(_optionCanvas, true, transform);
    }
}
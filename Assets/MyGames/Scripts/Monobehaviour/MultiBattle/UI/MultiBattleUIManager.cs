using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UIStrings;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using Photon.Realtime;
using static WaitTimes;
using static BattlePhase;

public class MultiBattleUIManager : MonoBehaviour
    //, IBattleUIManager
{
    [SerializeField]
    [Header("プレイヤーのUI")]
    PlayerUI _playerUI;

    [SerializeField]
    [Header("エネミーのUI")]
    PlayerUI _enemyUI;

    [SerializeField]
    [Header("未使用、使用済みの順にアイコン画像を必殺技ボタンに設定する")]
    Sprite[] _spButtonIcons;

    [SerializeField]
    [Header("ラウンド数表示テキスト")]
    TextMeshProUGUI _roundCountText;

    [SerializeField]
    [Header("カードリストを設定する(ScriptableObjectを参照)")]
    CardEntityList _cardEntityList;

    [SerializeField]
    [Header("カードプレハブ")]
    CardController _cardPrefab;

    [SerializeField]
    [Header("自分のターンであることを知らせるUI")]
    GameObject _announceThePlayerTurn;

    [SerializeField]
    [Header("相手のターンであることを知らせるUI")]
    GameObject _announceTheEnemyTurn;

    [SerializeField]
    [Header("カウントダウンのテキスト")]
    TextMeshProUGUI _countDownText;

    [SerializeField]
    [Header("開始時に非表示にするUIを設定します")]
    GameObject[] _hideUIs;




    //下は保留の値

    [SerializeField]
    [Header("ゲームの進行に関するUIマネージャーを設定")]
    DirectionUIManager _directionUIManager;

    [SerializeField]
    [Header("必殺技のUIマネージャーを設定")]
    SpecialSkillUIManager _specialSkillUIManager;

    [SerializeField]
    [Header("バトル中に使用する確認画面のUIを格納する")]
    GameObject[] BattleConfirmationPanels;

    //[SerializeField]
    //[Header("オブジェクトプールに使用する非表示にしたUIを格納するCanvasを設定")]
    //GameObject CanvasForObjectPool;

    //IHideableUIsAtStart _hideableUIsAtStartByDir;
    //IHideableUIsAtStart _hideableUIsAtStartBySP;

    IMultiConfirmationPanelManager _multiConfirmationPanelManager;
    PhotonView _photonView;


    #region//プロパティ
    public SpecialSkillUIManager SpecialSkillUIManager => _specialSkillUIManager;
    public DirectionUIManager DirectionUIManager => _directionUIManager;
    #endregion

    void Awake()
    {
        //ServiceLocator.Register<IBattleUIManager>(this);
        //_hideableUIsAtStartByDir = _directionUIManager.GetComponent<IHideableUIsAtStart>();
        //_hideableUIsAtStartBySP = _specialSkillUIManager.GetComponent<IHideableUIsAtStart>();
        _photonView = GetComponent<PhotonView>();
    }

    void OnDestroy()
    {
        //ServiceLocator.UnRegister<IBattleUIManager>(this);
    }

    void Start()
    {
        _multiConfirmationPanelManager = ServiceLocator.Resolve<IMultiConfirmationPanelManager>();
    }

    void Update()
    {
        TryToMoveToField(_multiConfirmationPanelManager.MovingFieldCard);
    }

    /// <summary>
    /// プレイヤーかエネミーのUIを取得する
    /// </summary>
    /// <returns></returns>
    public PlayerUI GetPlayerUI(bool isPlayer)
    {
        if (isPlayer) return _playerUI;
        return _enemyUI;
    }

    /// <summary>
    /// ポイントの表示
    /// </summary>
    public void ShowPointBy(bool isPlayer, int point)
    {
        GetPlayerUI(isPlayer).ShowPoint(point);
    }

    ///<summary>
    //プレイヤーの必殺技のImageの状態を設定する
    ///</summary>
    public void SetSpButtonImageBy(bool isPlayer, bool canUseSpSkill)
    {
        Sprite setSprite = null;
        if (canUseSpSkill) setSprite = _spButtonIcons[0];//未使用
        else setSprite = _spButtonIcons[1];//使用済み

        GetPlayerUI(isPlayer).SetSpButtonSprite(setSprite);
    }

    /// <summary>
    /// プレイヤーアイコンを配置する
    /// </summary>
    /// <param name="isPlayer"></param>
    /// <param name="targetGo"></param>
    public void PlacePlayerIconBy(bool isPlayer, GameObject targetGo)
    {
        GetPlayerUI(isPlayer).PlacePlayerIcon(targetGo);
    }

    /// <summary>
    /// ラウンド数を表示する
    /// </summary>
    /// <param name="roundCount"></param>
    /// <returns></returns>
    public async UniTask ShowRoundCountText(int roundCount)
    {
        //public async UniTask ShowRoundCountText(int roundCount, int maxRoundCount)
        ToggleRoundCountText(true);
        SetRoundCountText(roundCount);

        await UniTask.Delay(TimeSpan.FromSeconds(ROUND_COUNT_DISPLAY_TIME));
        ToggleRoundCountText(false);
    }

    /// <summary>
    /// ラウンド表示用のテキストを設定する
    /// </summary>
    void SetRoundCountText(int roundCount)
    {
        _roundCountText.text = ROUND_PREFIX + roundCount.ToString();
        //if (roundCount == maxRoundCount)
        //{
        //    //最終ラウンド
        //    _roundCountText.text = FINAL_ROUND;
        //}
        //else
        //{
        //    _roundCountText.text = ROUND_PREFIX + roundCount.ToString();
        //}
    }

    /// <summary>
    /// ラウンド数表示用テキストの切り替え
    /// </summary>
    /// <param name="isActive"></param>
    public void ToggleRoundCountText(bool isActive)
    {
        CanvasForObjectPool._instance.ToggleUIGameObject(_roundCountText.gameObject, isActive, transform);
    }

    /// <summary>
    /// カードを配ります
    /// </summary>
    public void DistributeCards()
    {
        //プレイヤーとエネミーにそれぞれ三種類のカードを作成する
        for (int i = 0; i < _cardEntityList.GetCardEntityList.Count; i++)
        {
            AddingCardToHand(i, true);
            AddingCardToHand(i, false);
        }
        //お互いのカードをシャッフルする
        //※実際には手札は同期されていないので不要な処理だが
        //相手に手の内がバレているのはないかといった不安を与えないよう演出させる
        ShuffleHandCard(true);
        ShuffleHandCard(false);
    }

    /// <summary>
    /// カードを手札に加えます
    /// </summary>
    /// <param name="cardIndex"></param>
    void AddingCardToHand(int cardIndex, bool isPlayer)
    {
        CardController card = CreateCard(cardIndex, isPlayer);
        GetPlayerUI(isPlayer).AddingCardToHand(card);
    }

    /// <summary>
    /// カードを生成する
    /// </summary>
    CardController CreateCard(int cardIndex, bool isPlayer)
    {
        CardController card = Instantiate(_cardPrefab, Vector3.zero, Quaternion.identity);
        card.Init(cardIndex, isPlayer);
        return card;
    }

    /// <summary>
    /// 手札のカードをシャッフルする
    /// </summary>
    void ShuffleHandCard(bool isPlayer)
    {
        CardController[] handCards = GetPlayerUI(isPlayer).GetAllHandCards();

        for (int i = 0; i < handCards.Length; i++)
        {
            int tempIndex = handCards[i].transform.GetSiblingIndex();
            int randomIndex = UnityEngine.Random.Range(0, handCards.Length);
            handCards[i].transform.SetSiblingIndex(randomIndex);
            handCards[randomIndex].transform.SetSiblingIndex(tempIndex);
        }
    }

    /// <summary>
    /// 盤面をリセットします
    /// </summary>
    public void ResetFieldCards()
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Card"))
        {
            Destroy(go);
        }
    }

    /// <summary>
    /// プレイヤーのターン時にテキストを表示する
    /// </summary>
    /// <param name="isPlayer"></param>
    /// <returns></returns>
    public async UniTask ShowThePlayerTurnText(bool isPlayer)
    {
        ToggleAnnounceTurnFor(true, isPlayer);
        await UniTask.Delay(TimeSpan.FromSeconds(ANNOUNCEMENT_TIME_TO_TURN_TEXT));
        ToggleAnnounceTurnFor(false, isPlayer);
    }

    /// <summary>
    /// プレイヤーのターン時に表示するUIの切り替え
    /// </summary>
    /// <param name="isActive"></param>
    public void ToggleAnnounceTurnFor(bool isActive, bool isPlayer)
    {
        GameObject AnnounceThePlayerTurn = GetAnnounceThePlayerTurnBy(isPlayer);
        CanvasForObjectPool._instance.ToggleUIGameObject(AnnounceThePlayerTurn, isActive, transform);
    }

    /// <summary>
    /// プレイヤーのターンのアナウンス用のゲームオブジェクトを取得する
    /// </summary>
    /// <param name="isPlayer"></param>
    /// <returns></returns>
    GameObject GetAnnounceThePlayerTurnBy(bool isPlayer)
    {
        if (isPlayer) return _announceThePlayerTurn;
        return _announceTheEnemyTurn;
    }

    /// <summary>
    /// カウントダウンを表示
    /// </summary>
    public void ShowCountDownText(int countDownTime)
    {
        _countDownText.text = countDownTime.ToString();
    }

    /// <summary>
    /// フィールドへの移動を試みます
    /// </summary>
    /// <returns></returns>
    void TryToMoveToField(CardController movingCard)
    {
        if (movingCard == null) return;
        _multiConfirmationPanelManager.DestroyMovingBattleCard();

        //すでにバトル場にカードが置かれているなら何もしない
        if (PhotonNetwork.LocalPlayer.GetCanPlaceCardToField() == false) return;
        MoveToBattleField(movingCard).Forget();
    }

    /// <summary>
    /// カードを移動する
    /// </summary>
    async UniTask MoveToBattleField(CardController movingCard)
    {
        RegisterCardType(movingCard.CardType);
        //カードを配置済みにする
        PhotonNetwork.LocalPlayer.SetCanPlaceCardToField(false);
        PhotonNetwork.CurrentRoom.SetIntBattlePhase(SELECTED);

        //playerのカードを移動する、対戦相手の視点ではEnemyのカードを移動する
        await _playerUI.MoveToBattleField(movingCard);
        _photonView.RPC("RpcMoveEnemyCardToField", RpcTarget.Others);

        await UniTask.Delay(TimeSpan.FromSeconds(TIME_BEFORE_CHANGING_TURN));
        //ターンを終了する
        PhotonNetwork.LocalPlayer.SetIsMyTurnEnd(true);
    }

    /// <summary>
    /// カードタイプを登録します
    /// </summary>
    void RegisterCardType(CardType cardType)
    {
        PhotonNetwork.LocalPlayer.SetIntBattleCardType(cardType);
    }

    /// <summary>
    /// エネミーのカードをフィールドに移動します
    /// </summary>
    [PunRPC]
    void RpcMoveEnemyCardToField()
    {
        //演出用にランダムなカードを選び移動させる。
        //※実際にフィールドに出すカードは異なります、カンニングを阻止する意もあります。
        CardController randomFieldCard = _enemyUI.GetRandomHandCard();
        _enemyUI.MoveToBattleField(randomFieldCard).Forget();
    }

    /// <summary>
    /// 開始時にUIを非表示にします
    /// </summary>
    public void HideUIAtStart()
    {

    }









    //下記は保留

    /// <summary>
    /// 確認画面UIを全てを非表示にする
    /// </summary>
    public void CloseAllConfirmationPanels()
    {
        foreach (GameObject targetPanel in BattleConfirmationPanels)
        {
            targetPanel.GetComponent<IToggleable>()?.ToggleUI(false);
        }
    }

    /// <summary>
    /// UIデータの初期設定
    /// </summary>
    public void InitUIData()
    {
        //_specialSkillUIManager.InitSpecialSkillButtonImageByPlayers();
        _specialSkillUIManager.InitSpecialSkillDescriptions();
    }

    /// <summary>
    /// カードを開くことをアナウンスします
    /// </summary>
    /// <returns></returns>
    public async UniTask AnnounceToOpenTheCard()
    {
        await _directionUIManager.AnnounceToOpenTheCard();
    }

    /// <summary>
    /// ラウンド数を表示する
    /// </summary>
    /// <param name="roundCount"></param>
    /// <param name="maxRoundCount"></param>
    /// <returns></returns>
    public async UniTask ShowRoundCountText(int roundCount, int maxRoundCount)
    {
        await _directionUIManager.ShowRoundCountText(roundCount, maxRoundCount);
    }

    /// <summary>
    /// ゲーム結果の表示の切り替え
    /// </summary>
    /// <param name="isAcitve"></param>
    public void ToggleGameResultUI(bool isActive)
    {
        _directionUIManager.ToggleGameResultUI(isActive);
    }

    /// <summary>
    /// ゲームの勝敗のテキストを表示する
    /// </summary>
    /// <returns></returns>
    public void SetGameResultText(string text)
    {
        _directionUIManager.SetGameResultText(text);
    }

    /// <summary>
    /// ラウンドの勝敗の結果を表示
    /// </summary>
    public async UniTask ShowJudgementResultText(string result)
    {
        await _directionUIManager.ShowJudgementResultText(result);
    }

    /// <summary>
    /// 必殺技を発動する
    /// </summary>
    public async UniTask ActivateSpecialSkill(bool isPlayer)
    {
        await _specialSkillUIManager.ActivateSpecialSkill(isPlayer);
    }
}
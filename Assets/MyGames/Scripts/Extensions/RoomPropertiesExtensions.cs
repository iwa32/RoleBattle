using ExitGames.Client.Photon;
using Photon.Realtime;
using static InitializationData;
using static BattlePhase;

public static class RoomPropertiesExtensions
{
    static readonly Hashtable propsToSet = new Hashtable();
    static readonly string RoundCountKey = "RoundCount";
    static readonly string PlayerTurnCoinKey = "PlayerTurnCoin";
    static readonly string BattlePhaseKey = "BattlePhase";

    /// <summary>
    /// ルームのラウンド数を取得する
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public static int GetRoundCount(this Room room)
    {
        return (room.CustomProperties[RoundCountKey] is int roundCount) ? roundCount : INITIAL_ROUND_COUNT;
    }

    /// <summary>
    /// ルームのラウンド数を設定する
    /// </summary>
    /// <param name="player"></param>
    /// <param name="point"></param>
    public static void SetRoundCount(this Room room, int roundCount)
    {
        propsToSet[RoundCountKey] = roundCount;
        room.SetCustomProperties(propsToSet);
        propsToSet.Clear();
    }

    /// <summary>
    /// プレイヤーのターンを決めるコインを取得する
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public static bool GetPlayerTurnCoin(this Room room)
    {
        return (room.CustomProperties[PlayerTurnCoinKey] is bool playerTurnCoin) ? playerTurnCoin : false;
    }

    /// <summary>
    /// プレイヤーのターンを決めるコインを設定する
    /// </summary>
    /// <param name="player"></param>
    /// <param name="point"></param>
    public static void SetPlayerTurnCoin(this Room room, bool playerTurnCoin)
    {
        propsToSet[PlayerTurnCoinKey] = playerTurnCoin;
        room.SetCustomProperties(propsToSet);
        propsToSet.Clear();
    }

    /// <summary>
    /// バトルのフェーズを取得する
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public static int GetBattlePhase(this Room room)
    {
        return (room.CustomProperties[BattlePhaseKey] is int battlePhase) ? battlePhase : 0;
    }

    /// <summary>
    /// バトルのフェーズを設定する
    /// </summary>
    /// <param name="player"></param>
    /// <param name="point"></param>
    public static void SetBattlePhase(this Room room, BattlePhase battlePhase)
    {
        propsToSet[BattlePhaseKey] = (int)battlePhase;
        room.SetCustomProperties(propsToSet);
        propsToSet.Clear();
    }
}

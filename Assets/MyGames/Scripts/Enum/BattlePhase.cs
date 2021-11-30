using System;
/// <summary>
/// バトルの段階
/// </summary>
public enum BattlePhase
{
    NONE,//初期値
    SELECTION,//選択
    SELECTED,//選択済み
    JUDGEMENT,//判定
    RESULT//判定終了
}

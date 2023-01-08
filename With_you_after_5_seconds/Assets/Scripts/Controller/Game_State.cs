using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum OutGame
{
    None,           // インゲーム時
    Title,          // タイトル画面時
    MeinMenu,       // メインメニュー画面時
    StageSelect,    // ステージセレクト画面時
    HowToPlay,      // あそびかた画面時
    StartStory,     // スタートストーリーのシーン
    EndingStory     // エンディングストーリーのシーン
}

public enum InGame
{
    None,               // アウトゲーム時
    CutIn,              // ゲーム開始前にページをめくる演出を作成（松島）
    ChangeStartView,    // ステージを見せる場面
    EntryPlayer,        // プレイヤーが入場してくる演出時
    PlayGame,           // ゲームプレイ時
    Pause,              // ポーズ中
    EntryGoal,          // ゴールゲートに進んでいるとき
    InGoal,             // ゴールゲートを潜り抜ける時
    GoalCompletion      // ゴールし終わったとき
}

public partial class GameData
{
    private OutGame outGameState = OutGame.None; // アウトゲーム時の状態
    private InGame inGameState = InGame.None;    // インゲーム時の状態

    public OutGame OutGameState { get { return outGameState; } }
    public InGame InGameState { get { return inGameState; } }


    // --------------- 各画面遷移時(アウトゲーム時) --------------- //
    // タイトル画面
    public void TitleTransition()
    {
        outGameState = OutGame.Title;
        inGameState = InGame.None;
        Debug.Log("現在の状態(OutGame)：" + outGameState);
    }

    // メインメニュー画面
    public void MeinMenuTransition()
    {
        outGameState = OutGame.MeinMenu;
        inGameState = InGame.None;
        Debug.Log("現在の状態(OutGame)：" + outGameState);
    }

    // ステージセレクト画面
    public void StageSelectTransition()
    {
        outGameState = OutGame.StageSelect;
        inGameState = InGame.None;
        Debug.Log("現在の状態(OutGame)：" + outGameState);
    }

    // あそびかた画面
    public void HowToPlayTransition()
    {
        outGameState = OutGame.HowToPlay;
        inGameState = InGame.None;
        Debug.Log("現在の状態(OutGame)：" + outGameState);
    }

    // スタートストーリー
    public void StartStoryTransition()
    {
        outGameState = OutGame.StartStory;
        inGameState = InGame.None;
        Debug.Log("現在の状態(OutGame)：" + outGameState);
    }

    // エンディングストーリー
    public void EndingStoryTransition()
    {
        outGameState = OutGame.EndingStory;
        inGameState = InGame.None;
        Debug.Log("現在の状態(OutGame)：" + outGameState);
    }

    // --------------- 各画面遷移時(インゲーム時) --------------- //
    // ゲーム開始前にページをめくる演出を入れる（松島）
    public void ChangeCutInTransition()
    {
        outGameState = OutGame.None;
        inGameState = InGame.CutIn;
        Debug.Log("現在の状態(InGame)：" + inGameState);
    }

    // ステージを見せる場面
    public void ChangeStartViewTransition()
    {
        outGameState = OutGame.None;
        inGameState = InGame.ChangeStartView;
        Debug.Log("現在の状態(InGame)：" + inGameState);
    }

    // プレイヤーが入場してくる演出時
    public void EntryPlayerTransition()
    {
        outGameState = OutGame.None;
        inGameState = InGame.EntryPlayer;
        Debug.Log("現在の状態(InGame)：" + inGameState);
    }

    // ゲームプレイ時
    public void PlayGameTransition()
    {
        outGameState = OutGame.None;
        inGameState = InGame.PlayGame;
        Debug.Log("現在の状態(InGame)：" + inGameState);
    }

    // ポーズ中
    public void PauseTransition()
    {
        outGameState = OutGame.None;
        inGameState = InGame.Pause;
        Debug.Log("現在の状態(InGame)：" + inGameState);
    }

    // ゴールゲートに進んでいるとき
    public void EntryGoalTransition()
    {
        outGameState = OutGame.None;
        inGameState = InGame.EntryGoal;
        Debug.Log("現在の状態(InGame)：" + inGameState);
    }

    // ゴールゲートを潜り抜ける時
    public void InGoalTransition()
    {
        outGameState = OutGame.None;
        inGameState = InGame.InGoal;
        Debug.Log("現在の状態(InGame)：" + inGameState);
    }

    // ゴールし終わったとき
    public void GoalCompletionTransition()
    {
        outGameState = OutGame.None;
        inGameState = InGame.GoalCompletion;
        Debug.Log("現在の状態(InGame)：" + inGameState);
    }
}

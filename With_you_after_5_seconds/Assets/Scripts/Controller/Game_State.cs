using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum OutGame
{
    None,           // �C���Q�[����
    Title,          // �^�C�g����ʎ�
    MeinMenu,       // ���C�����j���[��ʎ�
    StageSelect,    // �X�e�[�W�Z���N�g��ʎ�
    HowToPlay,      // �����т�����ʎ�
    StartStory,     // �X�^�[�g�X�g�[���[�̃V�[��
    EndingStory     // �G���f�B���O�X�g�[���[�̃V�[��
}

public enum InGame
{
    None,               // �A�E�g�Q�[����
    CutIn,              // �Q�[���J�n�O�Ƀy�[�W���߂��鉉�o���쐬�i�����j
    ChangeStartView,    // �X�e�[�W����������
    EntryPlayer,        // �v���C���[�����ꂵ�Ă��鉉�o��
    PlayGame,           // �Q�[���v���C��
    Pause,              // �|�[�Y��
    EntryGoal,          // �S�[���Q�[�g�ɐi��ł���Ƃ�
    InGoal,             // �S�[���Q�[�g����蔲���鎞
    GoalCompletion      // �S�[�����I������Ƃ�
}

public partial class GameData
{
    private OutGame outGameState = OutGame.None; // �A�E�g�Q�[�����̏��
    private InGame inGameState = InGame.None;    // �C���Q�[�����̏��

    public OutGame OutGameState { get { return outGameState; } }
    public InGame InGameState { get { return inGameState; } }


    // --------------- �e��ʑJ�ڎ�(�A�E�g�Q�[����) --------------- //
    // �^�C�g�����
    public void TitleTransition()
    {
        outGameState = OutGame.Title;
        inGameState = InGame.None;
        Debug.Log("���݂̏��(OutGame)�F" + outGameState);
    }

    // ���C�����j���[���
    public void MeinMenuTransition()
    {
        outGameState = OutGame.MeinMenu;
        inGameState = InGame.None;
        Debug.Log("���݂̏��(OutGame)�F" + outGameState);
    }

    // �X�e�[�W�Z���N�g���
    public void StageSelectTransition()
    {
        outGameState = OutGame.StageSelect;
        inGameState = InGame.None;
        Debug.Log("���݂̏��(OutGame)�F" + outGameState);
    }

    // �����т������
    public void HowToPlayTransition()
    {
        outGameState = OutGame.HowToPlay;
        inGameState = InGame.None;
        Debug.Log("���݂̏��(OutGame)�F" + outGameState);
    }

    // �X�^�[�g�X�g�[���[
    public void StartStoryTransition()
    {
        outGameState = OutGame.StartStory;
        inGameState = InGame.None;
        Debug.Log("���݂̏��(OutGame)�F" + outGameState);
    }

    // �G���f�B���O�X�g�[���[
    public void EndingStoryTransition()
    {
        outGameState = OutGame.EndingStory;
        inGameState = InGame.None;
        Debug.Log("���݂̏��(OutGame)�F" + outGameState);
    }

    // --------------- �e��ʑJ�ڎ�(�C���Q�[����) --------------- //
    // �Q�[���J�n�O�Ƀy�[�W���߂��鉉�o������i�����j
    public void ChangeCutInTransition()
    {
        outGameState = OutGame.None;
        inGameState = InGame.CutIn;
        Debug.Log("���݂̏��(InGame)�F" + inGameState);
    }

    // �X�e�[�W����������
    public void ChangeStartViewTransition()
    {
        outGameState = OutGame.None;
        inGameState = InGame.ChangeStartView;
        Debug.Log("���݂̏��(InGame)�F" + inGameState);
    }

    // �v���C���[�����ꂵ�Ă��鉉�o��
    public void EntryPlayerTransition()
    {
        outGameState = OutGame.None;
        inGameState = InGame.EntryPlayer;
        Debug.Log("���݂̏��(InGame)�F" + inGameState);
    }

    // �Q�[���v���C��
    public void PlayGameTransition()
    {
        outGameState = OutGame.None;
        inGameState = InGame.PlayGame;
        Debug.Log("���݂̏��(InGame)�F" + inGameState);
    }

    // �|�[�Y��
    public void PauseTransition()
    {
        outGameState = OutGame.None;
        inGameState = InGame.Pause;
        Debug.Log("���݂̏��(InGame)�F" + inGameState);
    }

    // �S�[���Q�[�g�ɐi��ł���Ƃ�
    public void EntryGoalTransition()
    {
        outGameState = OutGame.None;
        inGameState = InGame.EntryGoal;
        Debug.Log("���݂̏��(InGame)�F" + inGameState);
    }

    // �S�[���Q�[�g����蔲���鎞
    public void InGoalTransition()
    {
        outGameState = OutGame.None;
        inGameState = InGame.InGoal;
        Debug.Log("���݂̏��(InGame)�F" + inGameState);
    }

    // �S�[�����I������Ƃ�
    public void GoalCompletionTransition()
    {
        outGameState = OutGame.None;
        inGameState = InGame.GoalCompletion;
        Debug.Log("���݂̏��(InGame)�F" + inGameState);
    }
}

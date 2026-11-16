/*
**
** File              : GameProgress.cs
** Student           : Nawriz Ibrahim
** Student Number    : 301161181
** Course            : Game Programming 2 - COMP396 - Fall 2026
** Professor         : Diogo Fernandes de Queiroz
** Final Project     : Core Escape
** Date              : November 15, 2026
** Description       : Stores level selection, unlock progression,
**                     and completed-game results.
**
*/

using UnityEngine;

public static class GameProgress
{
    private const string DifficultyKey =
        "SelectedDifficulty";

    private const string MediumUnlockedKey =
        "MediumUnlocked";

    private const string HardUnlockedKey =
        "HardUnlocked";

    private const string FinalScoreKey =
        "FinalScore";

    private const string VictoryKey =
        "GameVictory";

    public static int SelectedDifficulty =>
        Mathf.Clamp(
            PlayerPrefs.GetInt(DifficultyKey, 0),
            0,
            2);

    public static string GetSelectedLevelScene()
    {
        return SelectedDifficulty switch
        {
            0 => "Level01_Warehouse",
            1 => "Level02_Laboratory",
            2 => "Level03_Reactor",
            _ => "Level01_Warehouse"
        };
    }

    public static void CompleteLevel(
        int completedDifficulty,
        int finalScore)
    {
        if (completedDifficulty == 0)
        {
            PlayerPrefs.SetInt(
                MediumUnlockedKey,
                1);
        }
        else if (completedDifficulty == 1)
        {
            PlayerPrefs.SetInt(
                HardUnlockedKey,
                1);
        }

        PlayerPrefs.SetInt(FinalScoreKey, finalScore);
        PlayerPrefs.SetInt(VictoryKey, 1);
        PlayerPrefs.Save();
    }

    public static void SaveDefeat(int finalScore)
    {
        PlayerPrefs.SetInt(FinalScoreKey, finalScore);
        PlayerPrefs.SetInt(VictoryKey, 0);
        PlayerPrefs.Save();
    }

    public static int GetFinalScore()
    {
        return PlayerPrefs.GetInt(FinalScoreKey, 0);
    }

    public static bool WasVictory()
    {
        return PlayerPrefs.GetInt(VictoryKey, 0) == 1;
    }
}
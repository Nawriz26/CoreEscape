/*
**
** File              : OptionsController.cs
** Student           : Nawriz Ibrahim
** Student Number    : 301161181
** Course            : Game Programming 2 - COMP396 - Fall 2026
** Professor         : Diogo Fernandes de Queiroz
** Final Project     : Core Escape
** Date              : November 15, 2026
** Description       : Saves sound and difficulty settings and controls
**                     the progressive unlocking of game levels.
**
*/

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsController : MonoBehaviour
{
    private const string SoundKey = "SoundEnabled";
    private const string DifficultyKey = "SelectedDifficulty";
    private const string MediumUnlockedKey = "MediumUnlocked";
    private const string HardUnlockedKey = "HardUnlocked";

    [Header("Options References")]
    [SerializeField] private Toggle soundToggle;
    [SerializeField] private Button easyButton;
    [SerializeField] private Button mediumButton;
    [SerializeField] private Button hardButton;
    [SerializeField] private TMP_Text selectedDifficultyText;

    private int selectedDifficulty;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplySavedSoundSetting()
    {
        bool soundEnabled =
            PlayerPrefs.GetInt(SoundKey, 1) == 1;

        AudioListener.volume = soundEnabled ? 1f : 0f;
    }

    private void Start()
    {
        bool soundEnabled =
            PlayerPrefs.GetInt(SoundKey, 1) == 1;

        soundToggle.SetIsOnWithoutNotify(soundEnabled);
        ApplySoundSetting(soundEnabled);

        bool mediumUnlocked =
            PlayerPrefs.GetInt(MediumUnlockedKey, 0) == 1;

        bool hardUnlocked =
            PlayerPrefs.GetInt(HardUnlockedKey, 0) == 1;

        mediumButton.interactable = mediumUnlocked;
        hardButton.interactable = hardUnlocked;

        selectedDifficulty =
            Mathf.Clamp(
                PlayerPrefs.GetInt(DifficultyKey, 0),
                0,
                2);

        if (selectedDifficulty == 1 && !mediumUnlocked)
        {
            selectedDifficulty = 0;
        }

        if (selectedDifficulty == 2 && !hardUnlocked)
        {
            selectedDifficulty = 0;
        }

        SaveDifficulty();
        UpdateDifficultyText();
    }

    public void SetSoundEnabled(bool enabled)
    {
        PlayerPrefs.SetInt(SoundKey, enabled ? 1 : 0);
        PlayerPrefs.Save();

        ApplySoundSetting(enabled);
    }

    public void SelectEasy()
    {
        SelectDifficulty(0);
    }

    public void SelectMedium()
    {
        if (mediumButton.interactable)
        {
            SelectDifficulty(1);
        }
    }

    public void SelectHard()
    {
        if (hardButton.interactable)
        {
            SelectDifficulty(2);
        }
    }

    private void SelectDifficulty(int difficulty)
    {
        selectedDifficulty = difficulty;
        SaveDifficulty();
        UpdateDifficultyText();
    }

    private void SaveDifficulty()
    {
        PlayerPrefs.SetInt(
            DifficultyKey,
            selectedDifficulty);

        PlayerPrefs.Save();
    }

    private void UpdateDifficultyText()
    {
        string difficultyName =
            selectedDifficulty switch
            {
                0 => "EASY",
                1 => "MEDIUM",
                2 => "HARD",
                _ => "EASY"
            };

        selectedDifficultyText.text =
            $"SELECTED: {difficultyName}";
    }

    private static void ApplySoundSetting(bool enabled)
    {
        AudioListener.volume = enabled ? 1f : 0f;
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    public AudioMixer audioMixer;
    public TMP_Dropdown resolutionDropdown;

    // Stocke les résolutions originales pour garder le refreshRate
    Resolution[] resolutions;

    public Slider musicSlider;
    public Slider soundSlider;

    public void Start()
    {
        // Vérification null pour éviter l'erreur
        if (audioMixer != null)
        {
            audioMixer.GetFloat("Music", out float musicValueForSlider);
            if (musicSlider != null)
                musicSlider.value = musicValueForSlider;

            audioMixer.GetFloat("Sound", out float soundValueForSlider);
            if (soundSlider != null)
                soundSlider.value = soundValueForSlider;
        }

        // Initialisation des résolutions
        resolutions = Screen.resolutions;

        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();

            List<string> options = new List<string>();

            int currentResolutionIndex = 0;

            // Filtrer les résolutions uniques en largeur/hauteur
            var filteredResolutions = resolutions
                .Select(r => new { r.width, r.height, r.refreshRate })
                .Distinct()
                .ToArray();

            for (int i = 0; i < filteredResolutions.Length; i++)
            {
                // Format d'affichage avec le refresh rate
                string option = $"{filteredResolutions[i].width} x {filteredResolutions[i].height} ({filteredResolutions[i].refreshRate} Hz)";
                options.Add(option);

                // Comparaison correcte avec la résolution actuelle
                if (filteredResolutions[i].width == Screen.currentResolution.width &&
                    filteredResolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                }
            }

            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();
        }

        Screen.fullScreen = true;
    }

    public void SetVolume(float volume)
    {
        if (audioMixer != null)
            audioMixer.SetFloat("Music", volume);
    }

    public void SetSoundVolume(float volume)
    {
        if (audioMixer != null)
            audioMixer.SetFloat("Sound", volume);
    }

    public void SetFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        if (resolutionDropdown != null && resolutions != null && resolutionIndex >= 0 && resolutionIndex < resolutions.Length)
        {
            // Pour retrouver la bonne résolution originale
            var selectedOption = resolutionDropdown.options[resolutionIndex].text;

            // Chercher la résolution correspondante dans le tableau original
            foreach (var res in resolutions)
            {
                if ($"{res.width} x {res.height} ({res.refreshRate} Hz)" == selectedOption)
                {
                    Screen.SetResolution(res.width, res.height, Screen.fullScreen, res.refreshRate);
                    break;
                }
            }
        }
    }

    public void ClearSavedData()
    {
        PlayerPrefs.DeleteAll();
    }
}
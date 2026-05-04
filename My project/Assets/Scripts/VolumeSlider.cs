using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private AudioMixer masterMixer;

    private void Start()
    {
        SetVolume(PlayerPrefs.GetFloat("SavedMasterVolume", 100));
    }

    public void SetVolume(float _value)
    {
        RefreshSlider(_value);
        masterMixer.SetFloat("MasterVolume", Mathf.Log10(_value/100)*20f);
        PlayerPrefs.SetFloat("SavedMasterVolume", _value);
    }

    public void SetVolumeFromSlider()
    {
        SetVolume(volumeSlider.value);
    }

    public void RefreshSlider(float _value)
    {
        volumeSlider.value = _value;
    }
}


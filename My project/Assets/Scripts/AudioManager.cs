using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource taliVoice;
    [SerializeField] private AudioSource declanVoice;
    [SerializeField] private AudioSource johnVoice;
    [SerializeField] private AudioSource presenterVoice;
    [SerializeField] private AudioSource garrusVoice;

    private AudioSource[] sources;

    void Start()
    {
        sources = new AudioSource[] {taliVoice, declanVoice, johnVoice, presenterVoice, garrusVoice};
    }

    private void StopOtherVoices()
    {
        foreach (AudioSource voice in sources) voice.Stop();
    }


    public void PlayVoice(string characterName)
    {   
        StopOtherVoices();
        switch (characterName)
        {
            case "Juhei Ikeet":
                taliVoice.Play();
                break;
            case "Decln'Argegh":
                declanVoice.Play();
                break;
            case "CyberJohn":
                johnVoice.Play();
                break;
            case "Presenter":
                presenterVoice.Play();
                break;
            case "Arneth Maalg":
                garrusVoice.Play();
                break;
        }
    }
}
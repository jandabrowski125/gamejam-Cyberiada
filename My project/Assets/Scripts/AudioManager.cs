using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource taliVoice;
    [SerializeField] private AudioSource declanVoice;
    [SerializeField] private AudioSource johnVoice;
    [SerializeField] private AudioSource presenterVoice;
    [SerializeField] private AudioSource garrusVoice;




    public void PlayVoice(string characterName)
    {
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
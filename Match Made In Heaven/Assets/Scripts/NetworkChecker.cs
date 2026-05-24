using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class NetworkChecker : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Ikonka na ekranie, która będzie się zmieniać")]
    public Image connectionIcon;
    public Sprite connectedSprite;
    public Sprite disconnectedSprite;
    
    [Tooltip("Tekst błędu wyświetlany na czerwono")]
    public TextMeshProUGUI errorMessageText;

    [Header("Settings")]
    [Tooltip("Co ile sekund sprawdzać połączenie?")]
    public float checkInterval = 5f;
    [Tooltip("Lekki adres do pingowania (najlepiej zostawić Google)")]
    public string pingUrl = "https://www.google.com";

    private void Start()
    {
        if (errorMessageText != null)
        {
            errorMessageText.gameObject.SetActive(false);
        }

        StartCoroutine(CheckConnectionRoutine());
    }

    private IEnumerator CheckConnectionRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(checkInterval);

        while (true)
        {
            yield return StartCoroutine(VerifyInternetAccess());
            yield return wait;
        }
    }

    private IEnumerator VerifyInternetAccess()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            SetConnectionState(false);
            yield break;
        }

        using (UnityWebRequest request = UnityWebRequest.Head(pingUrl))
        {
            request.timeout = 3;
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || 
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                SetConnectionState(false);
            }
            else
            {
                SetConnectionState(true);
            }
        }
    }

    private void SetConnectionState(bool isConnected)
    {
        if (connectionIcon != null)
        {
            connectionIcon.sprite = isConnected ? connectedSprite : disconnectedSprite;
        }

        if (errorMessageText != null)
        {
            errorMessageText.gameObject.SetActive(!isConnected);
        }
    }
}
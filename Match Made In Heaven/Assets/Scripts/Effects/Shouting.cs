using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ShoutingEffects : MonoBehaviour
{
    [Header("Animation config")]
    [SerializeField] private float _animationDuration = 0.5f;
    [SerializeField] private RectTransform _shoutStartPosition;
    [SerializeField] private AnimationCurve _slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Texts")]
    [SerializeField] private GameObject _shoutWordPrefab;
    [SerializeField] private Color _wrongWordShoutColor = Color.red;
    [SerializeField] private List<string> _textsIfWrongWord = new List<string>{"Idiot!", "This word doesn't exist!"};
    public void ShoutIfWrongWord()
    {

        var shoutObj = Instantiate(_shoutWordPrefab, _shoutStartPosition);
        RectTransform objRect = shoutObj.GetComponent<RectTransform>();
        var TMPObj = shoutObj.GetComponentInChildren<TextMeshProUGUI>();
        TMPObj.text = _textsIfWrongWord[Random.Range(0, _textsIfWrongWord.Count)];
        TMPObj.color = _wrongWordShoutColor;
        objRect.localPosition = new Vector2(Random.Range(-400, 400), 0);
        Vector2 targetPos = new Vector2(
            objRect.anchoredPosition.x + Random.Range(-300, 300),
            objRect.anchoredPosition.y + Random.Range(300, 400)
        );

        StartCoroutine(Shout(objRect, targetPos, shoutObj));
    }

    private IEnumerator Shout(RectTransform rect, Vector2 targetPos, GameObject obj)
    {
        float elapsedTime = 0;
        Vector2 startPos = rect.anchoredPosition;

        while (elapsedTime < _animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = _slideCurve.Evaluate(elapsedTime / _animationDuration);

            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);

            yield return null;
        }

        rect.anchoredPosition = startPos;
        Destroy(obj);
    }
}
using UnityEngine;
using System.Collections;

public class CharacterSceneManager : MonoBehaviour
{
    [Header("References")]
    public Transform characterTransform; 

    [Header("Positions & Scales")]
    // Dialog: Postać na środku, duża (np. skala 3)
    public Vector3 centerPosition = new Vector3(0, -3.26f, 0); 
    public Vector3 centerScale = new Vector3(0.39f, 0.39f, 0.39f);

    // Wordle: Postać na boku, jeszcze większa (np. skala 4)
    public Vector3 sidePosition = new Vector3(6f, -6f, 0);   
    public Vector3 sideScale = new Vector3(0.11f, 0.11f, 0.11f);

    [Header("Animation")]
    public float moveDuration = 0.5f;
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine movementCoroutine;

    private void OnEnable()
    {
        GameEvents.OnWordleRequired += MoveToSide;
        GameEvents.OnWordleSuccess += MoveToCenter;
    }

    private void OnDisable()
    {
        GameEvents.OnWordleRequired -= MoveToSide;
        GameEvents.OnWordleSuccess -= MoveToCenter;
    }

    private void MoveToSide(string word)
    {
        Debug.Log("Postać: Przechodzę na bok i zwiększam skalę.");
        TransitionTo(sidePosition, sideScale);
    }

    private void MoveToCenter(string solvedWord)
    {
        Debug.Log("Postać: Wracam na środek do skali dialogowej.");
        TransitionTo(centerPosition, centerScale);
    }

    private void TransitionTo(Vector3 targetPos, Vector3 targetScale)
    {
        if (characterTransform == null) return;
        if (movementCoroutine != null) StopCoroutine(movementCoroutine);
        movementCoroutine = StartCoroutine(MoveRoutine(targetPos, targetScale));
    }

    private IEnumerator MoveRoutine(Vector3 targetPos, Vector3 targetScale)
    {
        float elapsedTime = 0;
        Vector3 startPos = characterTransform.position;
        Vector3 startScale = characterTransform.localScale;

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveDuration;
            float curveT = moveCurve.Evaluate(t);

            // Interpolacja pozycji
            characterTransform.position = Vector3.Lerp(startPos, targetPos, curveT);
            
            // Interpolacja skali
            characterTransform.localScale = Vector3.Lerp(startScale, targetScale, curveT);
            
            yield return null;
        }

        characterTransform.position = targetPos;
        characterTransform.localScale = targetScale;
    }
}
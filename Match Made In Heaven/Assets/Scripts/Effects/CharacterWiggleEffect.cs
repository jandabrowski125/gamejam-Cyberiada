using System.Collections;
using UnityEngine;

public class CharacterWiggleEffect : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform _target;

    [Header("Wiggle")]
    [SerializeField] private float _duration = 0.45f;
    [SerializeField] private float _amplitude = 0.08f;
    [SerializeField] private AnimationCurve _wiggleCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.2f, 1f),
        new Keyframe(0.4f, -0.8f),
        new Keyframe(0.6f, 0.6f),
        new Keyframe(0.8f, -0.3f),
        new Keyframe(1f, 0f)
    );

    private Vector3 _baseLocalPosition;
    private Coroutine _wiggleCoroutine;

    private void Awake()
    {
        if (_target == null) _target = transform;
        _baseLocalPosition = _target.localPosition;
    }

    private void OnDisable()
    {
        StopWiggle();
    }

    public void PlayWiggle()
    {
        if (_target == null) return;

        if (_wiggleCoroutine != null)
        {
            StopCoroutine(_wiggleCoroutine);
            _target.localPosition = _baseLocalPosition;
        }

        _wiggleCoroutine = StartCoroutine(WiggleRoutine());
    }

    private void StopWiggle()
    {
        if (_wiggleCoroutine != null)
        {
            StopCoroutine(_wiggleCoroutine);
            _wiggleCoroutine = null;
        }

        if (_target != null) _target.localPosition = _baseLocalPosition;
    }

    private IEnumerator WiggleRoutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < _duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / _duration);
            float verticalOffset = _wiggleCurve.Evaluate(t) * _amplitude;

            _target.localPosition = _baseLocalPosition + new Vector3(0f, verticalOffset, 0f);
            yield return null;
        }

        _target.localPosition = _baseLocalPosition;
        _wiggleCoroutine = null;
    }
}

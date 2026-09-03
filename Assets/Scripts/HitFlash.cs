using System.Collections;
using UnityEngine;

public class HitFlash : MonoBehaviour
{
    [Header("타격 플레시")]
    private Renderer _renderer;
    private Color _originalColor;
    private Coroutine _flashRoutine;
    [SerializeField] private float _duration = 0.1f;
    [SerializeField] private Color _hitColor = Color.red;

    private void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>();
        _originalColor = _renderer.material.color;
    }

    public void Play()
    {
        if (_flashRoutine != null) StopCoroutine(_flashRoutine);
        _flashRoutine = StartCoroutine(Flash());
    }

    private IEnumerator Flash()
    {
        _renderer.material.color = _hitColor;
        yield return new WaitForSeconds(_duration);
        _renderer.material.color = _originalColor;
    }
}

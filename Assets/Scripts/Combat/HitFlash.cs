using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitFlash : MonoBehaviour
{
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    // 콘스트는 완전상수고 리드온리는 null이다가 들어오는 뭐랄까 2번째 값?

    [Header("타격 플레시")]
    [SerializeField] private float _duration = 0.1f;
    [SerializeField] private Color _hitColor = Color.red;

    [Header("예고 깜빡임")]
    [Tooltip("공격 직전 번쩍이는 색"), SerializeField] private Color _warnColor = new Color(1f, 0.95f, 0.6f);
    [Tooltip("한 번 번쩍이는 시간(초)"), SerializeField] private float _blinkOnTime = 0.04f;

    private readonly List<Material> _materials = new();
    private readonly List<Color> _originalColor = new();
    private Coroutine _flashRoutine;


    private void Awake()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach(Renderer r in renderers)
        {
            Material[] mats = r.materials;

            foreach(Material m in mats)
            {
                _materials.Add(m);
                _originalColor.Add(m.GetColor(BaseColorId));
            }
        }
    }

    public void Play()
    {
        if (_flashRoutine != null) StopCoroutine(_flashRoutine);
        _flashRoutine = StartCoroutine(Flash());
    }

    // 공격 직전 예고: 선딜(totalTime) 동안 점점 빠르게 번쩍임. 중간에 맞으면 Play()가 끊고 원래 색으로 복구
    public void Blink(float totalTime)
    {
        if (_flashRoutine != null) StopCoroutine(_flashRoutine);
        for (int i = 0; i < _materials.Count; i++) _materials[i].SetColor(BaseColorId, _originalColor[i]);
        _flashRoutine = StartCoroutine(BlinkRoutine(totalTime));
    }

    private IEnumerator BlinkRoutine(float totalTime)
    {
        float elapsed = 0f;
        for (int i = 0; i < 4; i++)
        {
            // 0.35, 0.6, 0.75, 0.85 지점: 간격이 짧아져 마지막에 가장 빠름
            float next = totalTime * (1f - 0.65f * Mathf.Pow(0.62f, i));
            // 선딜이 짧으면 들어가지 않는 깜빡임은 생략 (선딜이 끝난 뒤까지 번쩍이지 않도록)
            if (next + _blinkOnTime > totalTime + 0.02f) break;
            yield return new WaitForSeconds(Mathf.Max(0f, next - elapsed));
            foreach (Material m in _materials) m.SetColor(BaseColorId, _warnColor);
            yield return new WaitForSeconds(_blinkOnTime);
            for (int j = 0; j < _materials.Count; j++) _materials[j].SetColor(BaseColorId, _originalColor[j]);
            elapsed = next + _blinkOnTime;
        }
    }

    private IEnumerator Flash()
    {
        foreach (Material m in _materials)
        {
            m.SetColor(BaseColorId, _hitColor);
        }
        yield return new WaitForSeconds(_duration);
        for (int i = 0; i < _materials.Count; i++)
        {
            _materials[i].SetColor(BaseColorId, _originalColor[i]);
        }
    }
}

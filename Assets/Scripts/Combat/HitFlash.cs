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

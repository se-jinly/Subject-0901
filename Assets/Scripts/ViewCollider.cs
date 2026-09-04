using UnityEngine;

public class ColliderGizmos : MonoBehaviour
{
    private Mesh _capsuleMesh;
    private Mesh _sphereMesh;

    private void OnDrawGizmos()
    {
        if (_capsuleMesh == null)
        {
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            _capsuleMesh = temp.GetComponent<MeshFilter>().sharedMesh;
            DestroyImmediate(temp);
        }
        if (_sphereMesh == null)
        {
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _sphereMesh = temp.GetComponent<MeshFilter>().sharedMesh;
            DestroyImmediate(temp);
        }

        foreach (Collider col in FindObjectsByType<Collider>(FindObjectsSortMode.None))
        {
            Gizmos.color = col.isTrigger ? Color.yellow : Color.green;

            if (col is CapsuleCollider capsule)
            {
                Vector3 center = capsule.transform.TransformPoint(capsule.center);
                float r = capsule.radius * 2f;
                Gizmos.matrix = Matrix4x4.TRS(center, capsule.transform.rotation,
                    new Vector3(r, capsule.height * 0.5f, r));
                Gizmos.DrawWireMesh(_capsuleMesh);
            }
            else if (col is SphereCollider sphere)
            {
                Vector3 center = sphere.transform.TransformPoint(sphere.center);
                float d = sphere.radius * 2f;
                Gizmos.matrix = Matrix4x4.TRS(center, sphere.transform.rotation, Vector3.one * d);
                Gizmos.DrawWireMesh(_sphereMesh);
            }
            else if (col is BoxCollider box)
            {
                Gizmos.matrix = box.transform.localToWorldMatrix;
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else if (col is CharacterController cc)
            {
                Vector3 center = cc.transform.TransformPoint(cc.center);
                float r = cc.radius * 2f;
                Gizmos.matrix = Matrix4x4.TRS(center, cc.transform.rotation,
                    new Vector3(r, cc.height * 0.5f, r));
                Gizmos.DrawWireMesh(_capsuleMesh);
            }

            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
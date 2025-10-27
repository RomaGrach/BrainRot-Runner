using UnityEngine;

[ExecuteAlways]    // чтобы срабатывало и в редакторе
public class EnableInstancingOnAwake : MonoBehaviour
{
    void Awake()
    {
        var rend = GetComponent<Renderer>();
        if (rend != null && rend.sharedMaterial != null)
            rend.sharedMaterial.enableInstancing = true;
    }
}

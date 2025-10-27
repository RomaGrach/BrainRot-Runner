using UnityEngine;

[ExecuteInEditMode, RequireComponent(typeof(Camera))]
public class CurvedImageEffect : MonoBehaviour
{
    public Shader warpShader;          // простой full-screen шейдер
    [Range(0, 1)] public float amount = 0.3f;

    private Material _mat;
    void Start()
    {
        _mat = new Material(warpShader);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        _mat.SetFloat("_Amount", amount);
        Graphics.Blit(src, dst, _mat);
    }
}
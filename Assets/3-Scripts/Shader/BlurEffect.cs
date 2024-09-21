using UnityEngine;

[RequireComponent(typeof(Camera))]
public class BlurEffect : MonoBehaviour
{
    [SerializeField]
    private Shader blurShader;
    private Material blurMaterial;

    private void Start()
    {
        // Create a material using the blur shader
        blurMaterial = new Material(blurShader);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (blurMaterial != null)
        {
            // Set the texel size for the shader
            blurMaterial.SetVector("_MainTex_TexelSize", new Vector2(1.0f / source.width, 1.0f / source.height));
            // Apply the blur effect
            Graphics.Blit(source, destination, blurMaterial);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
}

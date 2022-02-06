using UnityEngine;

[RequireComponent(typeof(Camera))]
public class NormalsReplacementShader : MonoBehaviour
{
    [SerializeField] private Shader normalsShader = null;

    private RenderTexture renderTexture;
    private new Camera camera;

    private void Start()
    {
        Camera thisCamera = GetComponent<Camera>();

        renderTexture = new RenderTexture(thisCamera.pixelWidth, thisCamera.pixelHeight, 24);

        Shader.SetGlobalTexture("_CameraNormalsTexture", renderTexture);

        GameObject copy = new GameObject("Normals camera");
        camera = copy.AddComponent<Camera>();
        camera.CopyFrom(thisCamera);
        camera.transform.SetParent(transform);
        camera.targetTexture = renderTexture;
        camera.SetReplacementShader(normalsShader, "RenderType");
        camera.depth = thisCamera.depth - 1;
    }
}
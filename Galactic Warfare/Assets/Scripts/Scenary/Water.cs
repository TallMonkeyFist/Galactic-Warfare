using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    [SerializeField] private Color shallowWaterColor = new Color(0.325f, 0.807f, 0.971f, 0.725f);
    [SerializeField] private Color deepWaterColor = new Color(0.086f, 0.407f, 1.0f, 0.749f);
    [SerializeField] private float maxDepthDistance = 3;

    [Range(0, 1)] [SerializeField] private float noiseCutoff = 0.777f;
    [SerializeField] private Vector4 surfaceScrollAmount = new Vector4(0.03f, 0.03f, 0.0f, 0.0f);
    [SerializeField] private float maxFoamDistance = 0.4f;
    [SerializeField] private float minFoamDistance = 0.04f;

    [Range(0, 1)] [SerializeField] private float distortionStrength = 0.27f;
    [SerializeField] private Color foamColor = new Color(1, 1, 1, 1);
    [Range(0, 1)] [SerializeField] private float smoothStep = 0.1f;

    [SerializeField] private float waveStrength = 0.1f;
    [Range(1, 100)] [SerializeField] private float waveSpeed = 1.0f;

    public bool autoUpdate = false;

    private void Awake()
    {
        SetMaterial();
    }

    public void SetMaterial()
    {
        Material mat = new Material(GetComponent<Renderer>().sharedMaterial);
        GetComponent<Renderer>().material = mat;
        mat.SetColor("_DepthGradientShallow", shallowWaterColor);
        mat.SetColor("_DepthGradientDeep", deepWaterColor);
        mat.SetFloat("_DepthMaxDistance", maxDepthDistance);

        mat.SetFloat("_SurfaceNoiseCutoff", noiseCutoff);
        mat.SetVector("_SurfaceNoiseScroll", surfaceScrollAmount);
        mat.SetFloat("_FoamMaxDistance", maxFoamDistance);
        mat.SetFloat("_FoamMinDistance", minFoamDistance);

        mat.SetFloat("_SurfaceDistortionStrength", distortionStrength);
        mat.SetColor("_FoamColor", foamColor);
        mat.SetFloat("_SmoothStep", smoothStep);

        mat.SetFloat("_Displacement", waveStrength);
        mat.SetFloat("_DisplacementSpeed", waveSpeed);
    }
}

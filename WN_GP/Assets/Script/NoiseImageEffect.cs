using UnityEngine;
using System.Collections;
 
[ExecuteInEditMode]
public class NoiseImageEffect : MonoBehaviour
{
    public Texture noiseTexture;
    public float noiseXSpeed = 100f;
    public float noiseYSpeed = 100f;
    [Range(0, 1.0f)]
    public float cutoff = 0.35f;
     
    private string m_noiseTexPropertyName = "_NoiseTex";
    private string m_noiseXSpeedPropertyName = "_NoiseXSpeed";
    private string m_noiseYSpeedPropertyName = "_NoiseYSpeed";
    private string m_cutoffPropertyName = "_Cutoff";
 
    private int m_noiseTexID;
    private int m_noiseXSpeedID;
    private int m_noiseYSpeedID;
    private int m_cutoffID;
     
    private Material m_material;
     
    void Awake ()
    {
        InitPropertyIDs();
        OnValidate();
    }
     
     
    private void InitPropertyIDs()
    {
        if(m_material == null)
            m_material = new Material( Shader.Find("Unlit/Noise Effect") );
         
        m_noiseTexID = Shader.PropertyToID(m_noiseTexPropertyName);
        m_noiseXSpeedID = Shader.PropertyToID(m_noiseXSpeedPropertyName);
        m_noiseYSpeedID = Shader.PropertyToID(m_noiseYSpeedPropertyName);
        m_cutoffID = Shader.PropertyToID(m_cutoffPropertyName);
    }
     
     
    private void OnValidate()
    {
        if(m_material == null)
            m_material = new Material( Shader.Find("Unlit/Noise Effect") );
         
        m_material.SetTexture(m_noiseTexID, noiseTexture);
        m_material.SetFloat(m_noiseXSpeedID, noiseXSpeed);
        m_material.SetFloat(m_noiseYSpeedID, noiseYSpeed);
        m_material.SetFloat(m_cutoffID, cutoff);
    }
     
     
    void OnRenderImage (RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit (source, destination, m_material);
    }
}
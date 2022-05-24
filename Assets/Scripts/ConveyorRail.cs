using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ConveyorRail : MonoBehaviour
{
    [SerializeField]
    Color _color;

    public Color Color
    {
        get => _color;
        set
        {
            _color = value;
            UpdateColor(value);
        }
    }


    [field: SerializeField]
    public float Speed { get; set; } = 10.0f;

    [field: SerializeField]
    public float BobIntensity { get; set; } = 0.1f / 5f;

    [field: SerializeField]
    public float MaxBobIntensity { get; set; } = 0.1f;

    [field: SerializeField]
    public float BobFrequency { get; set; } = 10.0f;

    [field: SerializeField]
    public float PixelsPerUnit { get; private set; } = 64f;

    public Vector3 BasePosition { get; set; }
    public float BobOffset { get; private set; }
    public MeshRenderer Renderer { get; private set; }

    float bobTimer = 0f;
    float movementTimer = 0f;

    Material conveyorBeltMaterial;
    MaterialPropertyBlock materialSetter;

    int offsetXID;
    int offsetYID;
    int scaleXID;
    int scaleYID;
    int beltColorID;

    private void Start()
    {
        materialSetter = new MaterialPropertyBlock();
        BasePosition = transform.position;
        Renderer = GetComponent<MeshRenderer>();

        conveyorBeltMaterial = Renderer.sharedMaterial;
        offsetXID = Shader.PropertyToID("_ConveyorOffsetX");
        offsetYID = Shader.PropertyToID("_ConveyorOffsetY");

        scaleXID = Shader.PropertyToID("_ConveyorScaleX");
        scaleYID = Shader.PropertyToID("_ConveyorScaleY");
        beltColorID = Shader.PropertyToID("_RendererColor");

        Renderer.GetPropertyBlock(materialSetter);
        UpdatePPUScale();
    }

    private void OnValidate()
    {
        UpdateColor(_color);
    }

    void UpdateColor(Color color)
    {
        if (materialSetter == null)
        {
            Start();
        }
        Renderer.GetPropertyBlock(materialSetter);
        materialSetter.SetColor(beltColorID, _color);
        Renderer.SetPropertyBlock(materialSetter);
    }

    private void Update()
    {
        if (materialSetter == null)
        {
            Start();
        }
        Renderer.GetPropertyBlock(materialSetter);
        if (Application.isPlaying)
        {
            bobTimer += Time.deltaTime * BobFrequency;
            bobTimer %= 1.0f;

            movementTimer += Time.deltaTime * Speed;
            BobOffset = Mathf.Clamp(BobIntensity * Mathf.Abs(Speed),0f, MaxBobIntensity) * ((1f + Mathf.Sin(bobTimer * 2.0f * Mathf.PI)) / 2f);
            transform.position = BasePosition + new Vector3(0f, BobOffset);
            SetMaterialOffset(unitsToOffset(-movementTimer), 0f);
        }
        UpdatePPUScale();
        Renderer.SetPropertyBlock(materialSetter);
    }

    void UpdatePPUScale()
    {
        var width = conveyorBeltMaterial.mainTexture.width;
        var height = conveyorBeltMaterial.mainTexture.height;
        SetMaterialScale((transform.localScale.x / width) * PixelsPerUnit, (transform.localScale.y / height) * PixelsPerUnit);
    }

    void SetMaterialOffset(float x, float y)
    {
        materialSetter.SetFloat(offsetXID, x);
        materialSetter.SetFloat(offsetYID, y);
    }

    void SetMaterialScale(float x, float y)
    {
        materialSetter.SetFloat(scaleXID, x);
        materialSetter.SetFloat(scaleYID, y);
    }

    float offsetToUnits(float offset)
    {
        return (offset * conveyorBeltMaterial.mainTexture.width) / 64.0f;
    }

    float unitsToOffset(float units)
    {
        return (units * conveyorBeltMaterial.mainTextureScale.x / transform.localScale.x);
    }
}

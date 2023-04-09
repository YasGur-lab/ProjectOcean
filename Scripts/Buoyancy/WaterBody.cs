using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;
using Color = UnityEngine.Color;

public class WaterBody : MonoBehaviour
{
    public float Density = 1.0f;
    public float BuoyancyDamping = 1.0f;

    [SerializeField] private GameObject m_Ship;
    [SerializeField] private Vector2 m_Direction;
    [SerializeField] private float m_Steepness;
    [SerializeField] private float m_WaveFrequency;
    [SerializeField] private float m_WaveAmplitude;
    [SerializeField] private float m_WaveSpeed;

    [SerializeField] private float m_SecondaryAmplitude;
    [SerializeField] private Vector2 m_SecondaryDirection;
    [SerializeField] private float m_SecondarySpeed;
    [SerializeField] private float m_SecondaryFrequency;

    [SerializeField] private float m_ThirdAmplitude;
    [SerializeField] private Vector2 m_ThirdDirection;
    [SerializeField] private float m_ThirdSpeed;
    [SerializeField] private float m_ThirdFrequency;

    [SerializeField] private float m_FourthAmplitude;
    [SerializeField] private Vector2 m_FourthDirection;
    [SerializeField] private float m_FourthSpeed;
    [SerializeField] private float m_FourthFrequency;

    private Material m_OceanMat;
    private Vector3 m_TestDummyPotentialPos;
    void Start()
    {
        SetVariables();
    }

    void SetVariables()
    {
        m_OceanMat = GetComponent<Renderer>().sharedMaterial;
    }

    void OnValidate()
    {
        if (m_OceanMat)
            UpdateMaterial();
    }

    void Update()
    {
        if (m_Ship)
        {
            Vector3 tempPos = transform.position;
            tempPos.x = m_Ship.transform.position.x;
            tempPos.z = m_Ship.transform.position.z;
            transform.position = tempPos;
        }
    }

    public float VertexHeight(Vector3 pos, float time)
    {
        Vector2 uv = new Vector2(pos.x, pos.z);

        Vector2 uvStep = uv;

        float distanceTraveled = m_WaveSpeed * time;
        Vector2 posOffset = m_Direction * distanceTraveled;

        Vector2 heightSample0 = new Vector2(GerstnerWaveHeight(uvStep, posOffset, time).x, GerstnerWaveHeight(uvStep, posOffset, time).z);

        uvStep.x = uv.x - (heightSample0.x * m_WaveFrequency);

        uvStep.y = uv.y - (heightSample0.y * m_WaveFrequency);

        Vector2 heightSample1 = new Vector2(GerstnerWaveHeight(uvStep, posOffset, time).x, GerstnerWaveHeight(uvStep, posOffset, time).z);

        uvStep.x = uv.x - (heightSample1.x * m_WaveFrequency);

        uvStep.y = uv.y - (heightSample1.y * m_WaveFrequency);

        Vector2 heightSample2 = new Vector2(GerstnerWaveHeight(uvStep, posOffset, time).x, GerstnerWaveHeight(uvStep, posOffset, time).z);
        
        uvStep.x = uv.x - (heightSample2.x * m_WaveFrequency);
        uvStep.y = uv.y - (heightSample2.y * m_WaveFrequency);

        float finalHeight = GerstnerWaveHeight(uvStep, posOffset, time).y;
        m_TestDummyPotentialPos = GerstnerWaveHeight(uvStep, posOffset, time);
        return finalHeight;
    }

    Vector3 GerstnerWaveHeight(Vector2 worldPos, Vector2 positionOffset, float time)
    {
        Vector3 GerstnerWave;

        float SteepTimesAmp = m_WaveAmplitude * m_Steepness;
        float secSteepTimesAmp = m_SecondaryAmplitude * m_Steepness;
        float thirdSteepTimesAmp = m_ThirdAmplitude * m_Steepness;
        float fourthSteepTimesAmp = m_FourthAmplitude * m_Steepness;

        Vector2 normDIr = -m_Direction.normalized;
        Vector2 secondaryNormDir = -m_SecondaryDirection.normalized;
        Vector2 thirdNormDir = -m_ThirdDirection.normalized;
        Vector2 fourthNormDir = -m_FourthDirection.normalized;

        Vector2 dirTimesFrequency = normDIr * m_WaveFrequency;
        Vector2 secDirTimesFrequency = secondaryNormDir * m_SecondaryFrequency;
        Vector2 thirdDirTimesFrequency = thirdNormDir * m_ThirdFrequency;
        Vector2 fourthDirTimesFrequency = fourthNormDir * m_FourthFrequency;

        float velo = time * m_WaveSpeed;
        float SecondaryVelo = time * m_SecondarySpeed;
        float thirdVelo = time * m_ThirdSpeed;
        float fourthVelo = time * m_FourthSpeed;

        float SteepTimesAmpTimesDirX = SteepTimesAmp * normDIr.x;
        float SteepTimesAmpTimesDirZ = SteepTimesAmp * normDIr.y;

        float secSteepTimesAmpTimesDirX = secSteepTimesAmp * secondaryNormDir.x;
        float secSteepTimesAmpTimesDirZ = secSteepTimesAmp * secondaryNormDir.y;

        float thirdSteepTimesAmpTimesDirX = thirdSteepTimesAmp * thirdNormDir.x;
        float thirdSteepTimesAmpTimesDirZ = thirdSteepTimesAmp * thirdNormDir.y;

        float fourthSteepTimesAmpTimesDirX = fourthSteepTimesAmp * fourthNormDir.x;
        float fourthSteepTimesAmpTimesDirZ = fourthSteepTimesAmp * fourthNormDir.y;

        //WAVE_ONE
        float dot1 = Vector2.Dot(worldPos, dirTimesFrequency);
        dot1 += velo;

        float cosDot1 = Mathf.Cos(dot1);
        float cosDot1TimesDirX = cosDot1 * SteepTimesAmpTimesDirX;
        float cosDot1TimesDirZ = cosDot1 * SteepTimesAmpTimesDirZ;

        float sinDot1 = Mathf.Sin(dot1);
        sinDot1 *= m_WaveAmplitude;

        //WAVE_TWO
        float dot2 = Vector2.Dot(worldPos, secDirTimesFrequency);
        dot2 += SecondaryVelo;

        float cosDot2 = Mathf.Cos(dot2);
        float cosDot2TimesDirX = cosDot2 * secSteepTimesAmpTimesDirX;
        float cosDot2TimesDirZ = cosDot2 * secSteepTimesAmpTimesDirZ;
        
        float sinDot2 = Mathf.Sin(dot2);
        sinDot2 *= m_SecondaryAmplitude;

        //WAVE_THREE
        float dot3 = Vector2.Dot(worldPos, thirdDirTimesFrequency);
        dot3 += thirdVelo;

        float cosDot3 = Mathf.Cos(dot3);
        float cosDot3TimesDirX = cosDot3 * thirdSteepTimesAmpTimesDirX;
        float cosDot3TimesDirZ = cosDot3 * thirdSteepTimesAmpTimesDirZ;

        float sinDot3 = Mathf.Sin(dot3);
        sinDot3 *= m_ThirdAmplitude;

        //WAVE_FOUR
        float dot4 = Vector2.Dot(worldPos, fourthDirTimesFrequency);
        dot4 += fourthVelo;

        float cosDot4 = Mathf.Cos(dot4);
        float cosDot4TimesDirX = cosDot4 * fourthSteepTimesAmpTimesDirX;
        float cosDot4TimesDirZ = cosDot4 * fourthSteepTimesAmpTimesDirZ;

        float sinDot4 = Mathf.Sin(dot4);
        sinDot4 *= m_FourthAmplitude;

        //COMBINE WAVES
        float cosDotDirXCombine = cosDot1TimesDirX + cosDot2TimesDirX + cosDot3TimesDirX + cosDot4TimesDirX;
        float cosDotDirZCombine = cosDot1TimesDirZ + cosDot2TimesDirZ + cosDot3TimesDirZ + cosDot4TimesDirZ;
        float SinDotsCombined = sinDot1 + sinDot2 + sinDot3 + sinDot4;

        GerstnerWave = new Vector3(cosDotDirXCombine, SinDotsCombined, cosDotDirZCombine);
        //Debug.Log(GerstnerWave);
        return GerstnerWave;
    }

    void UpdateMaterial()
    {
        if (m_OceanMat)
        {
            Debug.Log("Updated");
            m_OceanMat.SetVector("_HDDirection", m_Direction);
            m_OceanMat.SetFloat("_HDFrequency", m_WaveFrequency);
            m_OceanMat.SetFloat("_HDSteepness", m_Steepness);
            m_OceanMat.SetFloat("_HDAmplitude", m_WaveAmplitude);
            m_OceanMat.SetFloat("_HDSpeed", m_WaveSpeed);

            m_OceanMat.SetFloat("_HDSecondary_Amplitude", m_SecondaryAmplitude);
            m_OceanMat.SetVector("_HDSecondary_Direction", m_SecondaryDirection);
            m_OceanMat.SetFloat("_HDSecondary_Speed", m_SecondarySpeed);
            m_OceanMat.SetFloat("_HDSecondary_Frequency", m_SecondaryFrequency);

            m_OceanMat.SetFloat("_HDThird_Amplitude", m_ThirdAmplitude);
            m_OceanMat.SetVector("_HDThird_Direction", m_ThirdDirection);
            m_OceanMat.SetFloat("_HDThird_Speed", m_ThirdSpeed);
            m_OceanMat.SetFloat("_HDThird_Frequency", m_ThirdFrequency);

            m_OceanMat.SetFloat("_HDFourth_Amplitude", m_FourthAmplitude);
            m_OceanMat.SetVector("_HDFourth_Direction", m_FourthDirection);
            m_OceanMat.SetFloat("_HDFourth_Speed", m_FourthSpeed);
            m_OceanMat.SetFloat("_HDFourth_Frequency", m_FourthFrequency);
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        WaterBuoyancy buoyancyHandler = collider.GetComponent<WaterBuoyancy>();
        if (buoyancyHandler != null)
        {
            buoyancyHandler.WaterBody = this;
        }

    }

    void OnTriggerExit(Collider collider)
    {
        WaterBuoyancy buoyancyHandler = collider.GetComponent<WaterBuoyancy>();
        if (buoyancyHandler != null)
        {
            buoyancyHandler.WaterBody = null;
        }
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        if (m_TestDummyPotentialPos != Vector3.zero)
            Gizmos.DrawSphere(m_TestDummyPotentialPos, 0.5f);
    }

    public Vector3 GetSurfaceNormal(Vector3 pos, float time)
    {
        Vector3 normal;
        Vector2 uv = new Vector2(pos.x, pos.z);

        Vector2 uvStep = uv;

        float distanceTraveled = m_WaveSpeed * time;
        Vector2 posOffset = m_Direction * distanceTraveled;

        Vector2 heightSample0 = new Vector2(GerstnerWaveHeight(uvStep, posOffset, time).x, GerstnerWaveHeight(uvStep, posOffset, time).z);

        uvStep.x = uv.x - (heightSample0.x * m_WaveFrequency);

        uvStep.y = uv.y - (heightSample0.y * m_WaveFrequency);

        Vector2 heightSample1 = new Vector2(GerstnerWaveHeight(uvStep, posOffset, time).x, GerstnerWaveHeight(uvStep, posOffset, time).z);

        uvStep.x = uv.x - (heightSample1.x * m_WaveFrequency);

        uvStep.y = uv.y - (heightSample1.y * m_WaveFrequency);

        Vector2 heightSample2 = new Vector2(GerstnerWaveHeight(uvStep, posOffset, time).x, GerstnerWaveHeight(uvStep, posOffset, time).z);
        uvStep.x = uv.x - (heightSample2.x * m_WaveFrequency);
        uvStep.y = uv.y - (heightSample2.y * m_WaveFrequency);

        float finalHeight = GerstnerWaveHeight(uvStep, posOffset, time).y;

        normal.x = heightSample0.y - heightSample1.y;
        normal.y = 2f * finalHeight * m_WaveFrequency;
        normal.z = heightSample0.x - heightSample1.x;

        Vector3 direction = new Vector3(m_Direction.x, 0f, m_Direction.y);
        normal = Vector3.Cross(direction, normal);
        normal = Vector3.Cross(normal, direction);

        return normal;

        //Vector3 normal;
        //Vector2 uv = new Vector2(pos.x, pos.z);

        //Vector2 uvStep = uv;

        //float distanceTraveled = m_WaveSpeed * time;
        //Vector2 posOffset = m_Direction * distanceTraveled;

        //Vector2 heightSample0 = new Vector2(GerstnerWaveHeight(uvStep, posOffset, time).x, GerstnerWaveHeight(uvStep, posOffset, time).z);

        //uvStep.x = uv.x - (heightSample0.x * m_WaveFrequency);

        //uvStep.y = uv.y - (heightSample0.y * m_WaveFrequency);

        //Vector2 heightSample1 = new Vector2(GerstnerWaveHeight(uvStep, posOffset, time).x, GerstnerWaveHeight(uvStep, posOffset, time).z);

        //uvStep.x = uv.x - (heightSample1.x * m_WaveFrequency);

        //uvStep.y = uv.y - (heightSample1.y * m_WaveFrequency);

        //Vector2 heightSample2 = new Vector2(GerstnerWaveHeight(uvStep, posOffset, time).x, GerstnerWaveHeight(uvStep, posOffset, time).z);
        //uvStep.x = uv.x - (heightSample2.x * m_WaveFrequency);
        //uvStep.y = uv.y - (heightSample2.y * m_WaveFrequency);

        //float finalHeight = GerstnerWaveHeight(uvStep, posOffset, time).y;

        //normal.x = heightSample0.y - heightSample1.y;
        //normal.y = 2f * finalHeight * m_WaveFrequency;
        //normal.z = heightSample0.x - heightSample1.x;

        //Vector3 temp = new Vector3(m_Direction.x, 0f, m_Direction.y);
        //Vector3.OrthoNormalize(ref normal, ref temp);

        ////normal.x = heightSample0.y - heightSample1.y;
        ////normal.y = 2f * finalHeight * m_WaveFrequency;
        ////normal.z = heightSample0.x - heightSample1.x;

        ////Vector3 temp = new Vector3(m_Direction.x, 0f, m_Direction.y);
        ////Vector3.OrthoNormalize(ref normal, ref temp);

        //return normal;
    }
}
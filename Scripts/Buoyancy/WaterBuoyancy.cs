using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json.Converters;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using Vector3 = UnityEngine.Vector3;

public class WaterBuoyancy : MonoBehaviour
{
    float m_Force;
    float ApproxBoxSphereRadius = 0.5f;
    Vector3 CenterOfMassOverride;

    public WaterBody WaterBody { get; set; }
    public float Density { get; private set; }

    public Rigidbody Body { get; private set; }

    void Start()
    {
        //Init components
        Body = GetComponent<Rigidbody>();

        if (CenterOfMassOverride.sqrMagnitude > 0.0f)
        {
            Body.centerOfMass = CenterOfMassOverride;
        }

        //Create list of buoyancy spheres
        m_BuoyancySpheres = new List<BuoyancySpheree>();

        //Make buoyancy Spheres based on colliders
        Collider[] colliderList = GetComponentsInChildren<Collider>();

        foreach (Collider collider in colliderList)
        {
            System.Type colliderType = collider.GetType();

            if (colliderType == typeof(BoxCollider))
            {
                SetupApproxBoxSpheres((BoxCollider)collider);
            }
            else if (colliderType == typeof(SphereCollider))
            {
                SetupApproxSpheres((SphereCollider)collider);
            }
            else if (colliderType == typeof(CapsuleCollider))
            {
                SetupApproxCapsuleSpheres((CapsuleCollider)collider);
            }
            else
            {
                DebugUtils.LogError("Unsupported collider type: {0}", colliderType.ToString());
            }

        }

    }

    //Doing the buoyancy physics in FixedUpdate.  This makes things more stable and consistant than using Update()
    //since the time step will always be the same.
    void FixedUpdate()
    {
        if (WaterBody == null)
        {
            return;
        }

        foreach (BuoyancySpheree sphere in m_BuoyancySpheres)
        {
            sphere.ApplyForce(this);
        }
    }

    //This will draw the buoyancy spheres.  This really came in handy to debug problems with setting up the spheres
    void OnDrawGizmosSelected()
    {
        if (m_BuoyancySpheres == null)
        {
            return;
        }
        foreach (BuoyancySpheree sphere in m_BuoyancySpheres)
        {
            sphere.OnDrawGizmosSelected();
        }
    }
    void Update()
    {

    }
    void SetupApproxSpheres(SphereCollider sphereCollider)
    {
        Vector3 localcenter = sphereCollider.center;
        float volumeAdjusment = 1.0f;

        BuoyancySpheree sphereZ = new BuoyancySpheree(sphereCollider.gameObject, ApproxBoxSphereRadius, localcenter, volumeAdjusment, m_Force);
        m_BuoyancySpheres.Add(sphereZ);
    }

    void SetupApproxBoxSpheres(BoxCollider boxCollider)
    {
        float sphereDiameter = ApproxBoxSphereRadius * 2;

        Vector3 boxDimension = boxCollider.size;

        boxDimension.x *= boxCollider.transform.lossyScale.x;
        boxDimension.y *= boxCollider.transform.lossyScale.y;
        boxDimension.z *= boxCollider.transform.lossyScale.z;

        float numberofsphereonX = Mathf.Max(((int)boxDimension.x / sphereDiameter), 1);
        float numberofsphereonY = Mathf.Max(((int)boxDimension.y / sphereDiameter), 1);
        float numberofsphereonZ = Mathf.Max(((int)boxDimension.z / sphereDiameter), 1);

        Vector3 localMinCorner = boxCollider.center - 0.5f * boxDimension;

        localMinCorner.x += ApproxBoxSphereRadius;
        localMinCorner.y += ApproxBoxSphereRadius;
        localMinCorner.z += ApproxBoxSphereRadius;
        int numOfSpheres = (int)numberofsphereonY * (int)numberofsphereonY;

        for (int x = 0; x < numberofsphereonX; x++)
        {
            for (int y = 0; y < numberofsphereonY; y++)
            {
                for (int z = 0; z < numberofsphereonZ; z++)
                {
                    Vector3 offset = new Vector3(sphereDiameter * x, sphereDiameter * y, sphereDiameter * z);

                    Vector3 localCenter = localMinCorner + offset;

                    localCenter.x /= boxCollider.transform.lossyScale.x;
                    localCenter.y /= boxCollider.transform.lossyScale.y;
                    localCenter.z /= boxCollider.transform.lossyScale.z;

                    float cubeVolume = sphereDiameter * sphereDiameter * sphereDiameter;

                    cubeVolume /= numOfSpheres;

                    float sphereVolume = MathUtils.CalcSphereVolume(ApproxBoxSphereRadius);
                    float volumeAdjusment = cubeVolume / sphereVolume;

                    BuoyancySpheree sphereZ = new BuoyancySpheree(boxCollider.gameObject, ApproxBoxSphereRadius, localCenter, volumeAdjusment, m_Force);
                    m_BuoyancySpheres.Add(sphereZ);
                }
            }
        }
    }

    private void SetupApproxCapsuleSpheres(CapsuleCollider capsuleCollider)
    {
        float sphereDiameter = ApproxBoxSphereRadius * 2;

        Vector3 capsuleDimension = new Vector3();
        capsuleDimension.x = capsuleCollider.radius * 2;
        capsuleDimension.y = capsuleCollider.height;
        capsuleDimension.z = capsuleCollider.radius * 2;

        capsuleDimension.x *= capsuleCollider.transform.lossyScale.x;
        capsuleDimension.y *= capsuleCollider.transform.lossyScale.y;
        capsuleDimension.z *= capsuleCollider.transform.lossyScale.z;

        float numberofsphereonX = Mathf.Max(((int)capsuleDimension.x / sphereDiameter), 1);
        float numberofsphereonY = Mathf.Max(((int)capsuleDimension.y / sphereDiameter), 1);
        float numberofsphereonZ = Mathf.Max(((int)capsuleDimension.z / sphereDiameter), 1);

        Vector3 localMinCorner = capsuleCollider.center - 0.5f * capsuleDimension;

        localMinCorner.x += ApproxBoxSphereRadius;
        localMinCorner.y += ApproxBoxSphereRadius;
        localMinCorner.z += ApproxBoxSphereRadius;

        for (int x = 0; x < numberofsphereonX; x++)
        {
            for (int y = 0; y < numberofsphereonY; y++)
            {
                for (int z = 0; z < numberofsphereonZ; z++)
                {
                    Vector3 offset = new Vector3(sphereDiameter * x, sphereDiameter * y, sphereDiameter * z);

                    Vector3 localCenter = localMinCorner + offset;

                    localCenter.x /= capsuleCollider.transform.lossyScale.x;
                    localCenter.y /= capsuleCollider.transform.lossyScale.y;
                    localCenter.z /= capsuleCollider.transform.lossyScale.z;

                    float cylinderVolume = (float)(Mathf.PI * capsuleCollider.radius * capsuleCollider.radius) * (capsuleCollider.height / numberofsphereonY);

                    //Debug.Log("SetupApproxCapsuleSpheres: " + y);

                    float sphereVolume = MathUtils.CalcSphereVolume(ApproxBoxSphereRadius);
                    float volumeAdjusment = cylinderVolume / sphereVolume;
                    BuoyancySpheree sphereZ = new BuoyancySpheree(capsuleCollider.gameObject, ApproxBoxSphereRadius, localCenter, volumeAdjusment, m_Force);
                    m_BuoyancySpheres.Add(sphereZ);
                }
            }
        }
    }

    public void ApplyForce(float mass, Vector3 applyPt)
    {
        Vector3 normalForce = new Vector3(
            0.0f,
            -9.8f * mass,
            0.0f
            );

        Body.AddForceAtPosition(normalForce, applyPt);
    }

    public List<BuoyancySpheree> GetBuoyancyList()
    {
        return m_BuoyancySpheres;
    }

    List<BuoyancySpheree> m_BuoyancySpheres;
}

public class BuoyancySpheree
{
    public BuoyancySpheree(GameObject parent, float radius, Vector3 localCenter, float volumeAdjustment, float force)
    {
        m_ParentObj = parent;

        m_Radius = radius;

        m_LocalPos = localCenter;

        m_VolumeAdjustment = volumeAdjustment;

        m_ForceAmount = force;
    }


    //public void ApplyForce(WaterBuoyancy handler)
    //{
    //    Vector3 worldPos = m_ParentObj.transform.TransformPoint(m_LocalPos);

    //    float sphereBottom = worldPos.y - m_Radius;

    //    float height = (handler.WaterBody.VertexHeight(worldPos, Time.time)) - sphereBottom;
    //    pos = new Vector3(worldPos.x, handler.WaterBody.VertexHeight(worldPos, Time.time), worldPos.z);

    //    height = Mathf.Clamp(height, 0.0f, m_Radius * 2);

    //    float volumeUnderWater = MathUtils.CalcSphereCapVolume(m_Radius, height);

    //    volumeUnderWater *= m_VolumeAdjustment;

    //    float fluidWeight = volumeUnderWater * handler.WaterBody.Density * Physics.gravity.y;

    //    float forceAmount = -fluidWeight;

    //    Vector3 velocityAtPos = handler.Body.GetPointVelocity(worldPos);

    //    float dampingForce = 0.0f;

    //    if (velocityAtPos.y > 0.0f)
    //    {
    //        dampingForce = handler.WaterBody.BuoyancyDamping * velocityAtPos.y * velocityAtPos.y;
    //    }

    //    forceAmount = Mathf.Max(0.0f, forceAmount - dampingForce);

    //    if (forceAmount > 0.0f)
    //    {
    //        if (!m_IsUnderWater) m_IsUnderWater = true;

    //        // Get the normal vector of the water surface at the boat's position
    //       // Vector3 normal = handler.WaterBody.GetSurfaceNormal(worldPos, Time.time);
    //        //Debug.DrawRay(new Vector3(worldPos.x, height, worldPos.z), normal * 10.0f, Color.yellow);
    //        // Apply a force in the direction of the normal vector
    //        //Vector3 force = forceAmount * normal;
    //        Vector3 force = new Vector3(0.0f, forceAmount, 0.0f);
    //        handler.Body.AddForceAtPosition(force, worldPos); ;
    //    }
    //    else
    //    {
    //        if (m_IsUnderWater) m_IsUnderWater = false;
    //    }
    //}

    public void ApplyForce(WaterBuoyancy handler)
    {
        Vector3 worldPos = m_ParentObj.transform.TransformPoint(m_LocalPos);

        float sphereBottom = worldPos.y - m_Radius;

        float height = (handler.WaterBody.VertexHeight(worldPos, Time.time)) - sphereBottom;
        pos = new Vector3(worldPos.x, handler.WaterBody.VertexHeight(worldPos, Time.time), worldPos.z);

        height = Mathf.Clamp(height, 0.0f, m_Radius * 2);

        float volumeUnderWater = MathUtils.CalcSphereCapVolume(m_Radius, height);

        volumeUnderWater *= m_VolumeAdjustment;

        float fluidWeight = volumeUnderWater * handler.WaterBody.Density * Physics.gravity.y;

        float forceAmount = -fluidWeight;

        Vector3 velocityAtPos = handler.Body.GetPointVelocity(worldPos);

        float dampingForce = 0.0f;

        if (velocityAtPos.y > 0.0f)
        {
            dampingForce = handler.WaterBody.BuoyancyDamping * velocityAtPos.y * velocityAtPos.y;
        }

        forceAmount = Mathf.Max(0.0f, forceAmount - dampingForce);

        //Vector3 normal = handler.WaterBody.GetSurfaceNormal(worldPos, Time.time);
        //if (pos.y < 0.0f)
        //    normal *= -1;
        
        //Debug.DrawRay(pos, normal * 10.0f, Color.yellow);

        if (forceAmount > 0.0f)
        {
            if (!m_IsUnderWater) m_IsUnderWater = true;

            Vector3 force = new Vector3(0.0f, forceAmount, 0.0f);
            handler.Body.AddForceAtPosition(force, worldPos);
        }
        else
        {
            if (m_IsUnderWater) m_IsUnderWater = false;
        }

    }

    public bool IsUnderWater() => m_IsUnderWater;

    public void OnDrawGizmosSelected()
    {
        Vector3 worldPos = m_ParentObj.transform.TransformPoint(m_LocalPos);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(worldPos, m_Radius);

        if (pos != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(pos, m_Radius);
        }
    }

    float m_Radius;
    Vector3 m_LocalPos;
    float m_VolumeAdjustment;
    public float m_ForceAmount;
    GameObject m_ParentObj;
    private Vector3 pos = Vector3.zero;
    private bool m_IsUnderWater;
}

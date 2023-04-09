using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatController : MonoBehaviour
{
    [SerializeField] float m_Speed = 30f;
    [SerializeField] float m_ReduceVelocityAmount = 0.1f;

    [SerializeField] float m_UprightSpeed = 3000f;
    [SerializeField] float m_RotationSpeed = 10f;

    [SerializeField] float m_MaxRotationAngleInX = 2.5f;
    [SerializeField] float m_RotationInXSpeed = 10f;

    [SerializeField] float m_MaxRotationAngle = 20f;
    [SerializeField] float m_StabilizationFactor = 0.3f;
    [SerializeField] float m_StabilizationThreshold = 2f;

    [SerializeField] Rigidbody m_Rididbody;

    public Quaternion m_UprightRotation;

    void Start()
    {
        m_Rididbody = GetComponent<Rigidbody>(); // Get the reference to the Rigidbody component
        m_UprightRotation = transform.rotation; // Save the upright rotation of the ship
    }

    //void FixedUpdate()
    //{
    //    float moveHorizontal = Input.GetAxis("Horizontal");
    //    float moveVertical = Input.GetAxis("Vertical");

    //    Vector3 movement = new Vector3(moveHorizontal, 0f, moveVertical);
    //    movement = transform.TransformDirection(movement);

    //    m_Rididbody.AddForce(movement * m_Speed);

    //    float rotateInput = Input.GetAxis("Horizontal");

    //    transform.Rotate(0f, rotateInput * m_RotationSpeed * Time.fixedDeltaTime, 0f);

    //    float currentRotationAngle = transform.eulerAngles.z;
    //    if (currentRotationAngle > 180f)
    //    {
    //        currentRotationAngle -= 360f;
    //    }

    //    float clampedRotationAngle = Mathf.Clamp(currentRotationAngle, -180f + m_MaxRotationAngle, 180f - m_MaxRotationAngle);

    //    Quaternion targetRotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, clampedRotationAngle);

    //    float distanceToUprightRotation = Quaternion.Angle(transform.rotation, m_UprightRotation);
    //    if (distanceToUprightRotation > m_StabilizationThreshold)
    //    {
    //        if (Mathf.Abs(rotateInput) > 0.1f)
    //        {
    //            Quaternion additionalRotation = Quaternion.Euler(0f, 0f, -m_StabilizationFactor * rotateInput);
    //            targetRotation *= additionalRotation;
    //        }
    //    }

    //    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, m_UprightSpeed * Time.fixedDeltaTime);


    //    if (moveVertical > 0f && moveVertical > 0f)
    //    {

    //    }
    //    else if (moveVertical > 0f)
    //    {
    //        Quaternion currentRotation = transform.rotation;
    //        float rotationAngle = Mathf.Clamp(currentRotation.eulerAngles.x, 0, -m_MaxRotationAngleInX);
    //        //float rotationAngle = Mathf.Clamp(currentRotation.eulerAngles.x - m_RotationInXSpeed * Time.fixedDeltaTime, -m_MaxRotationAngleInX, m_MaxRotationAngleInX);
    //        Quaternion targetRotationInX = Quaternion.Euler(-rotationAngle, currentRotation.eulerAngles.y, currentRotation.eulerAngles.z);
    //        transform.rotation = Quaternion.Lerp(currentRotation, targetRotationInX, m_RotationInXSpeed * Time.fixedDeltaTime); ;

    //    }

    //    if (m_Rididbody.velocity.magnitude < 1f && moveVertical == 0f)
    //    {
    //        m_Rididbody.velocity = Vector3.zero;
    //    }
    //    else if (moveVertical == 0f)
    //    {
    //        m_Rididbody.velocity *= m_ReduceVelocityAmount;
    //    }
    //}

    void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0f, moveVertical);
        movement = transform.TransformDirection(movement);

        m_Rididbody.AddForce(movement * m_Speed);

        float rotateInput = Input.GetAxis("Horizontal");

        transform.Rotate(0f, rotateInput * m_RotationSpeed * Time.fixedDeltaTime, 0f);

        // Clamp the rotation of the ship around the z-axis to a certain range
        float currentRotationAngle = transform.eulerAngles.z;
        if (currentRotationAngle > 180f)
        {
            currentRotationAngle -= 360f;
        }

        float clampedRotationAngle = Mathf.Clamp(currentRotationAngle, -m_MaxRotationAngle, m_MaxRotationAngle);
        Quaternion targetRotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, clampedRotationAngle);

        // Check if the ship is close to the upright rotation, and apply additional stabilization only if needed
        float distanceToUprightRotation = Quaternion.Angle(transform.rotation, m_UprightRotation);
        if (distanceToUprightRotation > m_StabilizationThreshold)
        {
            // Apply additional stabilization to the rotation when turning
            if (Mathf.Abs(rotateInput) > 0.1f)
            {
                Quaternion additionalRotation = Quaternion.Euler(0f, 0f, -m_StabilizationFactor * rotateInput);
                targetRotation *= additionalRotation;
                
            }
        }
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, m_UprightSpeed * Time.fixedDeltaTime);
        if (moveVertical > 0f)
        {
            Quaternion currentRotation = transform.rotation;
            float rotationAngle = Mathf.Clamp(currentRotation.eulerAngles.x, 0, -m_MaxRotationAngleInX);
            //float rotationAngle = Mathf.Clamp(currentRotation.eulerAngles.x - m_RotationInXSpeed * Time.fixedDeltaTime, -m_MaxRotationAngleInX, m_MaxRotationAngleInX);
            Quaternion targetRotationInX = Quaternion.Euler(-rotationAngle, currentRotation.eulerAngles.y, currentRotation.eulerAngles.z);
            transform.rotation = Quaternion.Lerp(currentRotation, targetRotationInX, m_RotationInXSpeed * Time.fixedDeltaTime); ;

        }

        if (moveVertical == 0f)
        {
            //m_Rididbody.velocity *= m_ReduceVelocityAmount;
        }
    }

}

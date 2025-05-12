using UnityEngine.InputSystem;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClawController : MonoBehaviour
{
    private const float maxAnchorSnapDistance = 1.5f;
    [Header("General Configuration")]
    [Space(10)]
    [SerializeField] private bool rightClaw;
    [SerializeField] private float forceAmount = 4.5f;
    [SerializeField] private float airMass = 47;
    [SerializeField] private float grabMass = 1;
    [SerializeField] private float grabBufferTime = 0.3f;  // Tiempo de buffer para realizar el agarre

    [Header("Crab Parts")]
    [Space(10)]
    private Rigidbody rb;
    [SerializeField] private Rigidbody clawAnchorRB;
    [SerializeField] private Animator clawAnim;
    [SerializeField] private Transform grabPoint;
    [SerializeField] private GameObject arrowGuide;
    private GameObject parentCrab;

    [Header("State Variables")]
    [Space(10)]
    [HideInInspector] public List<GameObject> touchedObjects;
    [HideInInspector] public Vector2 moveDirection;
    private bool grabbing;
    private bool grabPressed;
    public static bool someClawGrabbing = false;
    private Coroutine grabBufferCoroutine; 

    [Header("References")]
    [Space(10)]
    [SerializeField] private ClawController oppositeClaw;
    private ClawEffects clawEffects;
    [SerializeField] private CrabManager crabManager;


    void Awake()
    {
        clawEffects = GetComponent<ClawEffects>();
        rb = GetComponent<Rigidbody>();
        parentCrab = transform.parent.gameObject;
    }

    public void Move(InputAction.CallbackContext callBack) //CALLED ON INPUT SYSTEM
    {
        if (GameManager.state != GameState.Play || grabbing)
            return;

        if (callBack.performed)
        {
            Vector2 direction = callBack.ReadValue<Vector2>();
            moveDirection = new Vector2(direction.x, direction.y);
            if (GameManager.saveData.showArrow)
            {
                arrowGuide.SetActive(true); 
            }
        }
        else if (callBack.canceled)
        {
            moveDirection = Vector2.zero;
            arrowGuide.SetActive(false);
        }
    }

    public void Grab(InputAction.CallbackContext callBack) //CALLED ON INPUT SYSTEM
    {
        if (callBack.performed && GameManager.state == GameState.Play)
        {
            TryToGrab();
        }
        else if (callBack.canceled)
        {
            DropClaw();
            if (grabBufferCoroutine != null)
            {
                ResetGrabBuffer();
            }
        }
    }

    private void Update()
    {
        UpdateArrowGuideVisibility();
    }

    void FixedUpdate()
    {
        CheckGrabbingState();
        MoveClaw();
    }



    // METODS vvv


    private void TryToGrab()
    {
        clawEffects.PlayClackEffect();
        clawAnim.SetBool("Close", true);
        CheckObjectGrabbed();
    }

    

    public void DropClaw()
    {
        clawAnim.SetBool("Close", false);
        grabPressed = false;
        if (grabbing)
        {
            clawAnchorRB.isKinematic = false;
            grabbing = false;
            
            if (oppositeClaw.grabbing == false)
            {
                crabManager.rb.mass = airMass;
                someClawGrabbing = false;

                if (touchedObjects.Count > 0) 
                {
                    crabManager.fallingScript.SaveDropInfo(touchedObjects[0]);
                }
                
                if (!crabManager.respawnScript.movingFloor)
                {
                    crabManager.respawnScript.SetParents(parentCrab);
                }
            }
        }
    }

   

    private void CheckGrabbingState()
    {
        if (!grabbing && grabPressed)
        {
            if (IsTouchingLayer("Agarrable"))
            {
                StartGrabbing();
            }
            else if (IsTouchingLayer("Cristal"))
            {
                clawEffects.PlayMetalEffect();
                if (touchedObjects[0].CompareTag("Weapon"))
                {
                    crabManager.dialogScript.ShowWeaponDialog();
                }
            }
        }
    }

    private void MoveClaw()
    {
        if (!grabbing && moveDirection != Vector2.zero)
        {
            float adjustedForce = forceAmount * Time.timeScale;
            rb.AddForce(moveDirection * adjustedForce, ForceMode.Impulse);
            if (GameManager.saveData.showArrow)
            {
                float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
                arrowGuide.transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }

    
    
    public void DropBothClaws()
    {
        DropClaw();
        oppositeClaw.DropClaw();
    }

    void StartGrabbing()
    {
        StopMovementAndSlowMotion();
        MoveAnchorToClosestPoint();
        clawEffects.PlayGrabEffect();
        crabManager.wingsScript.ResetAllWingsObjects();
        crabManager.jetpackScript.ResetAllJetpackObjects();
        grabbing = true;
        someClawGrabbing = true;
        crabManager.rb.mass = grabMass;
        SetAnchorParent();
        crabManager.dialogScript.HandleDialogsOnGrab(touchedObjects[0]);
        CheckCameraChangeOnGrab();
    }

    private void CheckCameraChangeOnGrab()
    {
        CameraChangeOnGrab cameraChange;
        if (touchedObjects[0].TryGetComponent<CameraChangeOnGrab>(out cameraChange))
        {
            cameraChange.ChangeCamera();
        }
    }

    private void SetAnchorParent()
    {
        if (!touchedObjects[0].CompareTag("MovingObject"))
        {
            crabManager.respawnScript.SetParents(touchedObjects[0]);
        }
        else
        {
            clawAnchorRB.transform.SetParent(touchedObjects[0].transform);
        }
    }

    private void StopMovementAndSlowMotion()
    {
        moveDirection = Vector2.zero;
        if (crabManager.slowMotionScript.isSlowMotionActive)
        {
            crabManager.slowMotionScript.StopSlowMotion();
        }
        crabManager.wingsScript.wingsImage.color = Color.white;
        crabManager.wingsScript.wingsReady = true;
    }
    //COSO

    private void MoveAnchorToClosestPoint()
    {
        Collider touchedCollider = touchedObjects[0]?.GetComponent<Collider>();
        Vector3 closestPoint;

        if (touchedCollider != null)
        {
            if (touchedCollider is MeshCollider meshCollider && !meshCollider.convex)
            {
                closestPoint = CalculateClosestPointWithRaycast(touchedCollider, grabPoint.position);
            }
            else
            {
                closestPoint = touchedCollider.ClosestPoint(grabPoint.position);
            }

            clawAnchorRB.isKinematic = true;
            if (Vector3.Distance(closestPoint, clawAnchorRB.transform.position) < maxAnchorSnapDistance)
            {
                clawAnchorRB.transform.position = new Vector3(closestPoint.x, closestPoint.y, 0);
            }
        }
    }

    private void UpdateArrowGuideVisibility()
    {
        if (GameManager.saveData.showArrow)
        {
            arrowGuide.SetActive(!grabbing && moveDirection != Vector2.zero);
        }
    }

    Vector3 CalculateClosestPointWithRaycast(Collider targetCollider, Vector3 originPoint)
    {
        int rayCount = 16;
        Vector3 closestPoint = originPoint;
        float closestDistance = 100;

        for (int i = 0; i < rayCount; i++)
        {
            // Set the angle for each ray to be evenly distributed in a circle
            float angle = i * Mathf.PI * 2 / rayCount; 
            Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);

            if (Physics.Raycast(originPoint, direction, out RaycastHit hit, Mathf.Infinity))
            {
                if (hit.collider == targetCollider)
                {
                    float distance = Vector3.Distance(originPoint, hit.point);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestPoint = hit.point;
                    }
                }
            }
        }
        return closestPoint;
    }

    
    private bool IsTouchingLayer(string layerName)
    {
        if (HasValidTouchedObject())
        {
            return touchedObjects[0].layer == LayerMask.NameToLayer(layerName);
        }
        return false;
    }
    private bool HasValidTouchedObject()
    {
        return touchedObjects != null && touchedObjects.Count > 0 && touchedObjects[0] != null;
    }
    private void CheckObjectGrabbed()
    {
        if (!IsTouchingLayer("Agarrable") && !IsTouchingLayer("Cristal") && grabBufferCoroutine == null)
        {
            grabBufferCoroutine = StartCoroutine(GrabBufferCoroutine());
        }
        else if (IsTouchingLayer("Agarrable"))
        {
            StartGrabbing();
        }
        else if (IsTouchingLayer("Cristal"))
        {
            clawEffects.PlayMetalEffect();
            if (touchedObjects[0].CompareTag("Weapon"))
            {
                crabManager.dialogScript.ShowWeaponDialog();
            }
        }
    }

    private IEnumerator GrabBufferCoroutine()
    {
        grabPressed = true;
        yield return new WaitForSeconds(grabBufferTime);
        grabPressed = false;
        grabBufferCoroutine = null;
    }
    private void ResetGrabBuffer()
    {
        StopCoroutine(grabBufferCoroutine);
        grabPressed = false;
        grabBufferCoroutine = null;
    }
}

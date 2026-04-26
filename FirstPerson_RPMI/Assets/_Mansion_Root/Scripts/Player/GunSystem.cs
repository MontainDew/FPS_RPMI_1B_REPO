using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.UI.Image;

public class GunSystem : MonoBehaviour
{

    #region General Variables
    [Header("General References")]
    [SerializeField] Camera fpsCam;
    [SerializeField] Transform shootPoint;
    [SerializeField] LayerMask impactLayer;
    [SerializeField] LayerMask interactLayer;
    RaycastHit hit;

    [Header("Weapon Parameters")]
    [SerializeField] float range  = 2f;
    [SerializeField] float spread = 0f;
    [SerializeField] float flashCooldown = 2f;

    [Header("Flash Box Settings")]
    [SerializeField] Vector3 flashBoxSize = new Vector3(5f, 5f, 1f);

    [Header("Feedback References")]
    [SerializeField] GameObject impactEffect;

    [Header("Dev - Gun State Bools")]
    [SerializeField] bool shooting;
    [SerializeField] bool canShoot;

    [Header("Various References")]
    [SerializeField] GameObject camLight;
    [SerializeField] GameObject camParticles;

    [Header("Scripts References")]
    public PuzzleManager puzzleManager;
    public SensorsDetection sensorsDetection1;
    public SensorsDetection sensorsDetection2;
    public SensorsDetection sensorsDetection3;
    public SensorsDetection sensorsDetection4;
    public SensorsDetection sensorsDetection5;
    #endregion

    private void Awake()
    {
        canShoot = true;
    }

    void Update()
    {
        if (canShoot && shooting)
        {
            canShoot = false;
            StartCoroutine(FlashRoutine());
        }
        else if (!canShoot && shooting) Debug.Log("Cam is recharging...");
    }

    IEnumerator FlashRoutine()
    {
        camParticles.SetActive(true);
        yield return new WaitForSeconds(0.6f);
        AudioManager.Instance.Playsfx(0);
        camLight.SetActive(true);
        Shoot();
        yield return new WaitForSeconds(1.5f);
        AudioManager.Instance.Playsfx(1);
        yield return new WaitForSeconds(flashCooldown);
        camParticles.SetActive(false);
        camLight.SetActive(false);
        canShoot = true;
    }

    void Shoot()
    {
        Vector3 direction = fpsCam.transform.forward;
        Vector3 origin = fpsCam.transform.position;

        RaycastHit[] hits = Physics.BoxCastAll(origin, flashBoxSize * 0.5f, direction, fpsCam.transform.rotation, range, impactLayer);

        foreach (RaycastHit h in hits)
        {
            Debug.Log("Flash impacto: " + h.collider.name);

            if (h.collider.CompareTag("LightPannel1"))
            {
                puzzleManager.fusible1.SetActive(true);
            }

            if (h.collider.CompareTag("LightPannel2"))
            {
                puzzleManager.fusible2.SetActive(true);
            }

            if (h.collider.CompareTag("LightPannel3"))
            {
                puzzleManager.fusible3.SetActive(true);
            }

            if (h.collider.CompareTag("Sensor1"))
            {
                sensorsDetection1.isDeactivated = true;
            }

            if (h.collider.CompareTag("Sensor2"))
            {
                sensorsDetection2.isDeactivated = true;
            }

            if (h.collider.CompareTag("Sensor3"))
            {
                sensorsDetection3.isDeactivated = true;
            }

            if (h.collider.CompareTag("Sensor4"))
            {
                sensorsDetection4.isDeactivated = true;
            }

            if (h.collider.CompareTag("Sensor5"))
            {
                sensorsDetection5.isDeactivated = true;
            }

            if (impactEffect != null)
            {
                Instantiate(impactEffect, h.point, Quaternion.identity);
            }
        }
    }


    private void OnDrawGizmos()
    {
        if (fpsCam == null) return;

        Vector3 origin = fpsCam.transform.position;
        Vector3 direction = fpsCam.transform.forward;

        Gizmos.color = Color.yellow;

        int steps = 1; // cantidad de cubos para visualizar el recorrido
        float stepDistance = range / steps;

        for (int i = 0; i <= steps; i++)
        {
            Vector3 center = origin + direction * (stepDistance * i);
            Gizmos.DrawWireCube(center, flashBoxSize);
        }

        // Cubo final (impacto máximo)
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(origin + direction * range, flashBoxSize);
    }

    #region Input Methods
    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.performed) shooting = true;
        if (context.canceled) shooting = false;
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        
        Vector3 origin = fpsCam.transform.position;
        Vector3 direction = fpsCam.transform.forward;

        if (Physics.Raycast(origin, direction, out hit, range, interactLayer))
        {

        }
    }
    #endregion
}

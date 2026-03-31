using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunSystem : MonoBehaviour
{

    #region General Variables
    [Header("General References")]
    [SerializeField] Camera fpsCam;
    [SerializeField] Transform shootPoint;
    [SerializeField] LayerMask impactLayer;
    RaycastHit hit;

    [Header("Weapon Parameters")]
    [SerializeField] float range = 6f;
    [SerializeField] float spread = 0f;
    [SerializeField] float flashCooldown = 2f;

    [Header("Flash Box Settings")]
    [SerializeField] Vector3 flashBoxSize = new Vector3(2f, 2f, 2f);

    [Header("Feedback References")]
    [SerializeField] GameObject impactEffect;

    [Header("Dev - Gun State Bools")]
    [SerializeField] bool shooting;
    [SerializeField] bool canShoot;

    [Header("Various References")]
    [SerializeField] GameObject camLight;
    [SerializeField] GameObject camParticles;

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

            if (h.collider.CompareTag("LightPannel"))
            {
                //HACER QUE APAREZCA EL FUSIBLE
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

        // Línea principal del "raycast"
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, origin + direction * range);

        // Punto final del raycast
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(origin + direction * range, 0.2f);
    }

    #region Input Methods
    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.performed) shooting = true;
        if (context.canceled) shooting = false;
    }
    #endregion
}

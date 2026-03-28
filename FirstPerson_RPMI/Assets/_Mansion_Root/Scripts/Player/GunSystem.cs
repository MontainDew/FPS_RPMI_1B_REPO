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
    [SerializeField] float range = 100f;
    [SerializeField] float spread = 0f;
    [SerializeField] float flashCooldown = 0.2f;

    [Header("Flash Box Settings")]
    [SerializeField] Vector3 flashBoxSize = new Vector3(2f, 2f, 2f);

    [Header("Feedback References")]
    [SerializeField] GameObject impactEffect;

    [Header("Dev - Gun State Bools")]
    [SerializeField] bool shooting;
    [SerializeField] bool canShoot;

    #endregion

    private void Awake()
    {
        canShoot = true;
    }

    void Update()
    {
        if (canShoot && shooting)
        {
            StartCoroutine(FlashRoutine());
        }
    }

    IEnumerator FlashRoutine()
    {
        canShoot = false;

        //Sonido flash pequeño
        //Particulas flash
        //Sonido flash grande
        //Animacion Luz con Flash

        Shoot(); // Ahora usa BoxCast

        //Sonido recarga flash

        yield return new WaitForSeconds(flashCooldown);
        canShoot = true;
    }

    void Shoot()
    {
        Vector3 direction = fpsCam.transform.forward;

        // Origen del BoxCast
        Vector3 origin = fpsCam.transform.position;

        // BoxCastAll para detectar múltiples objetos
        RaycastHit[] hits = Physics.BoxCastAll(origin, flashBoxSize * 0.5f, direction, fpsCam.transform.rotation, range, impactLayer);

        foreach (RaycastHit h in hits)
        {
            Debug.Log("Flash impactó: " + h.collider.name);

            // Si es enemigo
            if (h.collider.CompareTag("Enemy"))
            {
                EnemyHealth enemyhealth = h.collider.GetComponent<EnemyHealth>();
                // Aquí puedes hacer que reaccione al flash
            }

            // Efecto visual opcional
            if (impactEffect != null)
            {
                Instantiate(impactEffect, h.point, Quaternion.identity);
            }
        }
    }

    #region Input Methods
    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.performed) shooting = true;
        if (context.canceled) shooting = false;
    }
    #endregion
}

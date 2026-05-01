using System.Collections;
using Unity.VisualScripting;
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
    [SerializeField] int stuningTime = 5;

    [Header("Weapon Parameters")]
    [SerializeField] float range = 2f;
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
    [SerializeField] GameObject CPuzzleInterface;
    [SerializeField] GameObject DPuzzleInterface;
    [SerializeField] EnemyAiBase Enemy;

    [Header("Scripts References")]
    public PuzzleManager puzzleManager;
    public Inv_Logic inventory;
    public GardenDoor fuseBox;
    public Cofre cofre;
    public SensorDetection sensorsDetection1;
    public SensorDetection sensorsDetection2;
    public SensorDetection sensorsDetection3;
    public SensorDetection sensorsDetection4;
    public SensorDetection sensorsDetection5;
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
                if (puzzleManager.fusible1 == null) return;

                puzzleManager.fusible1.SetActive(true);
            }

            if (h.collider.CompareTag("LightPannel2"))
            {
                if (puzzleManager.fusible2 == null) return;
                puzzleManager.fusible2.SetActive(true);
            }

            if (h.collider.CompareTag("LightPannel3"))
            {
                if (puzzleManager.fusible3 == null) return;
                puzzleManager.fusible3.SetActive(true);
            }

            if (h.collider.CompareTag("Enemy"))
            {
                StartCoroutine(FlashedEnemy());
            }

            if (h.collider.CompareTag("Sensor1"))
            {
                AudioManager.Instance.Playsfx(5);
                sensorsDetection1.isDeactivated = true;
            }

            if (h.collider.CompareTag("Sensor2"))
            {
                AudioManager.Instance.Playsfx(5);
                sensorsDetection2.isDeactivated = true;
            }

            if (h.collider.CompareTag("Sensor3"))
            {
                AudioManager.Instance.Playsfx(5);
                sensorsDetection3.isDeactivated = true;
            }

            if (h.collider.CompareTag("Sensor4"))
            {
                AudioManager.Instance.Playsfx(5);
                sensorsDetection4.isDeactivated = true;
            }

            if (h.collider.CompareTag("Sensor5"))
            {
                AudioManager.Instance.Playsfx(5);
                sensorsDetection5.isDeactivated = true;
            }

            if (impactEffect != null)
            {
                Instantiate(impactEffect, h.point, Quaternion.identity);
            }
        }
    }

    IEnumerator FlashedEnemy()
    {
        Enemy.animator.SetTrigger("Stun");
        Enemy.enabled = false;
        yield return new WaitForSeconds(stuningTime);
        Enemy.enabled = true;
        
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

        // Cubo final (impacto m?ximo)
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
        if (!context.performed) return;

        Vector3 origin = fpsCam.transform.position;
        Vector3 direction = fpsCam.transform.forward;

        if (Physics.Raycast(origin, direction, out hit, range, interactLayer))
        {
            Debug.Log("Golpea primero: " + hit.collider.name);
        
            Debug.Log("Interactuando");

            if (hit.collider.CompareTag("Button"))
            {
                AudioManager.Instance.Playsfx(4);
                sensorsDetection1.ResetSensors();
            }

            if (hit.collider.CompareTag("Fuse1"))
            {
                AudioManager.Instance.Playsfx(2);
                Destroy(puzzleManager.fusible1);
                inventory.FuseCuantity++;
            }

            if (hit.collider.CompareTag("Fuse2"))
            {
                AudioManager.Instance.Playsfx(2);
                Destroy(puzzleManager.fusible2);
                inventory.FuseCuantity++;
            }

            if (hit.collider.CompareTag("Fuse3"))
            {
                AudioManager.Instance.Playsfx(2);
                Destroy(puzzleManager.fusible3);
                inventory.FuseCuantity++;
            }

            if (hit.collider.CompareTag("FuseBox"))
            {
                if (inventory.FuseCuantity >= 3)
                {
                    AudioManager.Instance.Playsfx(3);
                    Debug.Log("Fusibles colocados");
                    fuseBox.greenFuse.SetActive(true);
                    fuseBox.blueFuse.SetActive(true);
                    fuseBox.redFuse.SetActive(true);
                    fuseBox.enemy.SetActive(true);

                    GameObject[] puertas = GameObject.FindGameObjectsWithTag("PuertaPatio");

                     foreach (GameObject puerta in puertas)
                    {   
                         puerta.transform.Rotate(0f, 90f, 0f);
                         AudioManager.Instance.Playsfx(16);
                    }
                }
            }
            if (hit.collider.CompareTag("Clock"))
            {
                if (inventory.ColorCode)
                {
                    CPuzzleInterface.SetActive(true);
                    AudioManager.Instance.PlayMusic(3);
                    Cursor.lockState = CursorLockMode.Confined;
                    Cursor.visible = true;
                    inventory.Pause();
                }
            }
            if (hit.collider.CompareTag("DoorPanel"))
            {
                if (inventory.DoorCode)
                {
                    DPuzzleInterface.SetActive(true);
                    Cursor.lockState = CursorLockMode.Confined;
                    Cursor.visible = true;
                    inventory.Pause();
                }
            }
             if (hit.collider.CompareTag("Key"))
            {
                Debug.Log("llavero epico");
                AudioManager.Instance.Playsfx(8);
                Destroy(hit.collider.gameObject);
                inventory.Key = true;
            }
             if (hit.collider.CompareTag("ColorCode1"))
            {
                Debug.Log("cacamod minecraft");
                AudioManager.Instance.Playsfx(9);
                Destroy(hit.collider.gameObject);
                inventory.HColorCode = true;
            }

            if (hit.collider.CompareTag("FullColorCode"))
            {
                Debug.Log("cacamod minecraft");
                AudioManager.Instance.Playsfx(9);
                Destroy(hit.collider.gameObject);
                inventory.ColorCode = true;
            }
        }
    }
    #endregion
}
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunSystem : MonoBehaviour
{

    #region General Variables
    [Header("General References")]
    [SerializeField] Camera fpsCam; //Ref si disparamos desde el centro de la camara
    [SerializeField] Transform shootPoint; //Ref si disparamos desde la punta del cañon
    [SerializeField] LayerMask impactLayer; //Layer con la que interactua el raycast
    RaycastHit hit; //Almacén de la información de los objetos ocn los que el raycast puede chocar

    [Header("Weapon Parameters")]
    [SerializeField] int damage = 10;
    [SerializeField] float range = 100f;
    [SerializeField] float spread = 0f; //Radio de dispersión
    [SerializeField] float shootingCooldown = 0.2f;
    [SerializeField] float reloadTime = 1.5f;
    [SerializeField] bool allowButtonHold = false; // El disparo se puede mantener o no 

    [Header("Bullet Management")]
    [SerializeField] int ammoSize = 30;
    [SerializeField] int bulletsPerTap = 1; //Cantidad de balas por disparo
    int bulletsLeft;


    [Header("Feedback References")]
    [SerializeField] GameObject impactEffect;//Impacto de bala visual

    [Header("Dev - Gun State Bools")]
    [SerializeField] bool shooting; //Pone si estamos disparando
    [SerializeField] bool canShoot;  //Pone si podemos disparar
    [SerializeField] bool reloading;

    #endregion

    private void Awake()
    {
        bulletsLeft = ammoSize;
        canShoot = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Shoot()
    {
        //!!!!!!!!!!!! pium pium raycast wow disparo y toco cosas
        Vector3 direction = fpsCam.transform.forward;

        //Dispersion aleatoria
        direction.x += Random.Range(-spread, spread);
        direction.y += Random.Range(-spread, spread);

        //rayo
        //Physics.Raycast(Origen del rayo, dirección, almacén de la info del impacto, longitud del rayo, layer con la que impacta el rayo)
        if (Physics.Raycast(fpsCam.transform.position, direction, out hit, range, impactLayer))
        {
            Debug.Log(hit.collider.name);
        }
    }
    #region
    public void OnShoot(InputAction.CallbackContext context)
    {
        Shoot();
    }

    public void OnReload(InputAction.CallbackContext context)
    {

    }
    #endregion
}

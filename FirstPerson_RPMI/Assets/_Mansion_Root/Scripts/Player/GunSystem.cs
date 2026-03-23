using System.Collections;
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
    [SerializeField] int bulletsLeft;


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
       if (canShoot && shooting && !reloading && bulletsLeft > 0)
        {
            StartCoroutine(ShootRoutine());
        }
    }

    IEnumerator ShootRoutine()
    {
        canShoot = false;//Primera capa de seguridad que evita que apilemos disparos
        if (!allowButtonHold) shooting = false; //Disparación por click
        for(int i = 0; i < bulletsPerTap; i++)
        {
            if(bulletsLeft <= 0) break;//Cuando no hay balas no dispara
            Shoot();
            bulletsLeft--;
        }
        yield return new WaitForSeconds(shootingCooldown);//tiempo entre disparos
        canShoot = true;
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
            if (hit.collider.CompareTag("Enemy"))
            {
                EnemyHealth enemyhealth = hit.collider.GetComponent<EnemyHealth>();
                enemyhealth.TakeDamage(damage);
            }
        }
    }
    IEnumerator ReloadRoutine()
    {
        reloading = true; //No se estaquea la recarga
        //aqui iria Animacion de recarga
        yield return new WaitForSeconds(reloadTime);
        bulletsLeft = ammoSize;
        reloading = false;
    }

    void Reload()
    {
        if (bulletsLeft < ammoSize && !reloading)
        {
            StartCoroutine(ReloadRoutine());
        }
    }

    #region Input Methods
    public void OnShoot(InputAction.CallbackContext context)
    {
        //Comprobar que el disparo se puede mantener o no
        if (allowButtonHold)
        {
            shooting = context.ReadValueAsButton();
        }
        else
        {
            if (context.performed) shooting = true;
        }
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.performed) Reload();
    }
    #endregion
}

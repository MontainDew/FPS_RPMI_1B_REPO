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
    [SerializeField] float range = 100f;
    [SerializeField] float spread = 0f; //Radio de dispersión
    [SerializeField] float flashCooldown = 0.2f;

    [Header("Feedback References")]
    [SerializeField] GameObject impactEffect;//Impacto de bala visual

    [Header("Dev - Gun State Bools")]
    [SerializeField] bool shooting; //Pone si estamos disparando
    [SerializeField] bool canShoot;  //Pone si podemos disparar

    #endregion

    private void Awake()
    {
        canShoot = true;
    }

    // Update is called once per frame
    void Update()
    {
       if (canShoot && shooting)
        {
            StartCoroutine(FlashRoutine());
        }
    }

    IEnumerator FlashRoutine()
    {
        canShoot = false;//Primera capa de seguridad que evita que apilemos disparos
            //Sonido flash pequeño
            //Particulas flash
            //Sonido flash grande
            //Animacion Luz con Flash
            Shoot();
            //Sonido recarga flash
        yield return new WaitForSeconds(flashCooldown);//tiempo entre fotos
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
            }
        }
    }

    #region Input Methods
    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.performed) shooting = true;
    }
    #endregion
}

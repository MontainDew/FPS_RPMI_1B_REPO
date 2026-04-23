using System.Collections;
using UnityEngine;

public class SensorsDetection : MonoBehaviour
{

    [Header("Sensors Detection Variables")]
    [SerializeField] Material detectionArea;
    [SerializeField] bool playerDetected;
    public bool isDeactivated;
    [SerializeField] float deactivationTime;

    [Header("Puzzle Reward Objects")]
    [SerializeField] GameObject rewardCage;
    [SerializeField] bool rewardPickable;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerDetected = false;
        isDeactivated = false;
        rewardPickable = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerDetected == false && isDeactivated == true)
        {
            StartCoroutine(FlashedRoutine);
        }
    }

    public void ResetSensors()
    {
        //SFX Reset
        playerDetected = false;
        rewardPickable = true;
        //Animacion abrir baul llave
        //Cambiar material a no detectado
        //Reactivar todos los collider
    }

    IEnumerator FlashedRoutine()
    {
        //Sonido desactivado
        //Desactivar collider
        //Animacion desactivado
        yield return new WaitForSeconds(deactivationTime);
        //Sonido reactivado
        //Reactivar collider
        //Animacion reactivado
        isDeactivated = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerDetected = true;
            rewardPickable = false;
            //ANIMACION CERRAR BAUL LLAVE
            //Sonido detectado
            detectionArea = new Material(); //Material detectado 
            //Desactivar colliders para que no repita el sonido con otros sensores si ya ha sido detectado con uno
        }
    }

}

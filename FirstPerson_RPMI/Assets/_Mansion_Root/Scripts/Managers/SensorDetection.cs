using System.Collections;
using UnityEngine;

public class SensorDetection : MonoBehaviour
{

    [Header("Sensors Detection Variables")]
    public MeshCollider detectionCollider;
    public MeshRenderer materialRenderer;
    [SerializeField] bool playerDetected;
    public bool isDeactivated;
    [SerializeField] float deactivationTime;

    [Header("Material Varibles")]
    public Material normalMat;
    public Material detectedMat;
    public Material deactivatedMat;

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

    private void Awake()
    {
        detectionCollider = GetComponent<MeshCollider>();
        materialRenderer = GetComponent<MeshRenderer>();
    }

    void Update()
    {
        if (playerDetected == false && isDeactivated == true)
        {
            Debug.Log("LISTO PARA DESCTIVAR");
            StartCoroutine(FlashedRoutine());
        }
    }

    public void ResetSensors()
    {
        //SFX Reset
        playerDetected = false;
        rewardPickable = true;
        //Animacion abrir baul llave
        materialRenderer.material = normalMat; //A todos los sensores a la vez
        detectionCollider.enabled = false; //A todos los sensores a la vez
    }

    IEnumerator FlashedRoutine()
    {
        Debug.Log("CAMBIO DE ESTADO");

        //Sonido desactivado
        detectionCollider.enabled = false;
        materialRenderer.material = deactivatedMat;
        //Animacion desactivado
        yield return new WaitForSeconds(deactivationTime);
        //Sonido reactivado
        detectionCollider.enabled = true;
        materialRenderer.material = normalMat;
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
            materialRenderer.material = detectedMat;
            detectionCollider.enabled = false; //De todos los sensores a la vez
        }
    }

}
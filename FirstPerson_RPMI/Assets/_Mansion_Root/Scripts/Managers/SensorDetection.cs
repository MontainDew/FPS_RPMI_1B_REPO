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
        materialRenderer.material = normalMat;
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
        //Animacion reset boton
        playerDetected = false;
        rewardPickable = true;
        //Animacion abrir baul llave
        GameObject[] detectionAreas = GameObject.FindGameObjectsWithTag("DetectionArea"); //Vuelve a activar los colliders de todas las detection areas

        foreach (GameObject area in detectionAreas)
        {
            MeshCollider col = area.GetComponent<MeshCollider>();
            MeshRenderer rend = area.GetComponent<MeshRenderer>();
            if (col != null)
            {
                col.enabled = true;
            }

            if (rend != null)
            {
                rend.material = normalMat;
            }
        }
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
            GameObject[] detectionAreas = GameObject.FindGameObjectsWithTag("DetectionArea");

            foreach (GameObject area in detectionAreas)
            {
                MeshCollider col = area.GetComponent<MeshCollider>();
                if (col != null)
                {
                    col.enabled = false;
                }
            }
        }
    }

}
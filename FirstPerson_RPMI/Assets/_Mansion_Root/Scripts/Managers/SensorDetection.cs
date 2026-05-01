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
    

    private bool isFlashing = false;

    [Header("Material Variables")]
    public Material normalMat;
    public Material detectedMat;
    public Material deactivatedMat;

    [Header("Puzzle Reward Objects")]
    [SerializeField] GameObject rewardCollider;
    [SerializeField] bool rewardPickable;

    void Awake()
    {
        detectionCollider = GetComponent<MeshCollider>();
        materialRenderer = GetComponent<MeshRenderer>();
    }

    void Start()
    {
        playerDetected = false;
        isDeactivated = false;
        rewardPickable = true;
        materialRenderer.material = normalMat;
        rewardCollider.SetActive(true);
    }

    void Update()
    {
        
        if (isDeactivated && !isFlashing)
        {
            StartCoroutine(FlashedRoutine());
        }

        if (rewardPickable == false)
        {
            rewardCollider.SetActive(false);
        }
    }

    public void DeactivateSensor()
    {
        playerDetected = false;
        isDeactivated = true;
    }

    public void ResetSensors()
    {
        playerDetected = false;
        rewardPickable = true;

        rewardCollider.SetActive(true);

        GameObject[] detectionAreas = GameObject.FindGameObjectsWithTag("DetectionArea");

        AudioManager.Instance.Playsfx(7);

        foreach (GameObject area in detectionAreas)
        {
            MeshCollider col = area.GetComponent<MeshCollider>();
            MeshRenderer rend = area.GetComponent<MeshRenderer>();

            SensorDetection sensor = area.GetComponent<SensorDetection>();

            if (sensor != null)
            {
                sensor.rewardPickable = true;
            }

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
        isFlashing = true;

        Debug.Log("CAMBIO DE ESTADO");

        detectionCollider.enabled = false;
        materialRenderer.material = deactivatedMat;

        yield return new WaitForSeconds(deactivationTime);

        detectionCollider.enabled = true;
        materialRenderer.material = normalMat;
        AudioManager.Instance.Playsfx(7);
        isDeactivated = false;
        isFlashing = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerDetected = true;
            rewardPickable = false;

            rewardCollider.SetActive(false);

            AudioManager.Instance.Playsfx(6);

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
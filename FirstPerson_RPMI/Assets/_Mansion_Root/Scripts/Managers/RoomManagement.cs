using UnityEngine;

public class RoomManagement : MonoBehaviour
{
    [Header("Puzzle Rooms Management Check")]
    [SerializeField] bool fusible1Room;
    [SerializeField] bool fusible2Room;
    [SerializeField] bool fusible3Room;
    [SerializeField] bool sensorRoom;

    [Header("Puzzle Rooms Objects")]
    [SerializeField] GameObject[] fusiblePanels;
    [SerializeField] GameObject sensors;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fusible1Room = false;
        fusible2Room = false;
        fusible3Room = false;
        sensorRoom = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (fusible1Room == true && fusible2Room == false && fusible3Room == false && sensors == false)
        {
            fusiblePanels[0].SetActive(true);
            fusiblePanels[1].SetActive(false);
            fusiblePanels[2].SetActive(false);
            sensors.SetActive(false);
        }

        else if (fusible2Room == true && fusible1Room == false && fusible3Room == false && sensors == false)
        {
            fusiblePanels[0].SetActive(false);
            fusiblePanels[1].SetActive(true);
            fusiblePanels[2].SetActive(false);
            sensors.SetActive(false);
        }
        else if (fusible3Room == true && fusible1Room == false && fusible2Room == false && sensors == false)
        {
            fusiblePanels[0].SetActive(false);
            fusiblePanels[1].SetActive(false);
            fusiblePanels[2].SetActive(true);
            sensors.SetActive(false);
        }
        else if (sensorRoom == true && fusible1Room == false && fusible2Room == false && fusible3Room == false)
        {
            fusiblePanels[0].SetActive(false);
            fusiblePanels[1].SetActive(false);
            fusiblePanels[2].SetActive(false);
            sensors.SetActive(true);
        }
        else
        {
            fusiblePanels[0].SetActive(false);
            fusiblePanels[1].SetActive(false);
            fusiblePanels[2].SetActive(false);
            sensors.SetActive(false);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Fusible1"))
        {
            fusible1Room = true;
            fusible2Room = false;
            fusible3Room = false;
            sensorRoom = false;

        }

        else if (other.gameObject.CompareTag("Fusible2"))
        {
            fusible1Room = false;
            fusible2Room = true;
            fusible3Room = false;
            sensorRoom = false;
        }

        else if (other.gameObject.CompareTag("Fusible3"))
        {
            fusible1Room = false;
            fusible2Room = false;
            fusible3Room = true;
            sensorRoom = false;
        }

        else if (other.gameObject.CompareTag("SensorRoom"))
        {
            fusible1Room = false;
            fusible2Room = false;
            fusible3Room = false;
            sensorRoom = true;
        }
        else 
        {
            fusible1Room = false;
            fusible2Room = false;
            fusible3Room = false;
            sensorRoom = false;
        }
    }
}

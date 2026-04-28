using UnityEngine;

public class GardenDoor : MonoBehaviour
{
    [Header("Fuse Box References")]
    [SerializeField] GameObject greenFuse;
    [SerializeField] GameObject blueFuse;
    [SerializeField] GameObject redFuse;

    [Header("Script References")]
    public Inv_Logic inventory;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        greenFuse.SetActive(false);
        blueFuse.SetActive(false);
        redFuse.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

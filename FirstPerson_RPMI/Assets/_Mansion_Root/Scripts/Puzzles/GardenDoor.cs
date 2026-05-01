using UnityEngine;

public class GardenDoor : MonoBehaviour
{
    [Header("Fuse Box References")]
    public GameObject greenFuse;
    public GameObject blueFuse;
    public GameObject redFuse;
    public GameObject enemy;

    [Header("Script References")]
    public Inv_Logic inventory;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        greenFuse.SetActive(false);
        blueFuse.SetActive(false);
        redFuse.SetActive(false);
        enemy.SetActive(false);

    }

    

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenDoor()
{
    
}
}

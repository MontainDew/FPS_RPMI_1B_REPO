using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [Header("Fusible Items References")]
    public GameObject fusible1;
    public GameObject fusible2;
    public GameObject fusible3;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fusible1.SetActive(false);
        fusible2.SetActive(false);
        fusible3.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

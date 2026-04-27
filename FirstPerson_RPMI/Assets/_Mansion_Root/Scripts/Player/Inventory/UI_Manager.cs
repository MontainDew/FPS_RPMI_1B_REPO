using TMPro;
using UnityEngine;

public class UI_Manager : MonoBehaviour
{
    [Header("Checkers")]
    public bool fuseC;
    public bool cameraLightC;
    public bool ColorCodeC;
    public bool fullColorCodeC;
    public bool doorCodeC;

    [Header("Inventory")]
    [SerializeField] private GameObject fuse;
    public int fuseNum;
    public TMP_Text NumText;
    [SerializeField] private GameObject cameraLight;
    [SerializeField] private GameObject ColorCode;
    [SerializeField] private GameObject fullColorCode;
    [SerializeField] private GameObject doorCode;

    private void Update()
    {

        if (fuseC == true)
        {
            fuse.SetActive(true);
        }
        else
        {
            fuse.SetActive(false);
        }

        NumText.text = "X" + fuseNum.ToString();

        if (cameraLightC == true)
        {
            cameraLight.SetActive(true);
        }
        else
        {
            cameraLight.SetActive(false);
        }
        if (ColorCodeC == true)
        {
            ColorCode.SetActive(true);
        }
        else
        {
            ColorCode.SetActive(false);
        }
        if (fullColorCodeC == true)
        {
            fullColorCode.SetActive(true);
        }
        else
        {
            fullColorCode.SetActive(false);
        }
        if (doorCodeC == true)
        {
            doorCode.SetActive(true);
        }
        else
        {
            doorCode.SetActive(false);
        }
    }
}

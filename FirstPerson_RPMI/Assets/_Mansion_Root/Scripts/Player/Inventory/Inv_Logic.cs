using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Inv_Logic : MonoBehaviour
{
    #region Variables
    [Header("Checker")]
     public bool Camera;
     public int FuseCuantity;
     public bool Key;
     public bool HColorCode;
     public bool ColorCode;
     public bool DoorCode;

    [Header("ItemSlots")]
    [SerializeField] private GameObject CameraSlot;
    [SerializeField] private GameObject FuseSlot;
    [SerializeField] private TMP_Text FuseCuantityText;
    [SerializeField] private GameObject KeySlot;
    [SerializeField] private GameObject HColorCodeSlot;
    [SerializeField] private GameObject ColorCodeSlot;
    [SerializeField] private GameObject DoorCodeSlot;

    [Header("Other")]
    [SerializeField] private GameObject Inventory;
    [SerializeField] private int onoff = 1;
    [SerializeField] private GunSystem Gun;
    [SerializeField] private FP_Controller player;
    #endregion


    private void Start()
    {
        AudioManager.Instance.PlayMusic(1);
    }

    private void Update()
    {
        if (FuseCuantity > 0)
        {
            FuseSlot.SetActive(true);
            FuseCuantityText.text = "X " + FuseCuantity.ToString();
        }
        else
        {
            FuseSlot.SetActive(false);
        }
        if (HColorCode)
        {
            HColorCodeSlot.SetActive(true);
        }
        else
        {
            HColorCodeSlot.SetActive(false);
        }
        if (DoorCode)
        {
            DoorCodeSlot.SetActive(true);
        }
        else
        {
            DoorCodeSlot.SetActive(false);
        }
        if (Camera)
        {
            CameraSlot.SetActive(true);
        }
        else
        {
            CameraSlot.SetActive(false);
        }
        if (Key)
        {
            KeySlot.SetActive(true);
        }
        else
        {
            KeySlot.SetActive(false);
        }
        if (ColorCode)
        {
            ColorCodeSlot.SetActive(true);
        }
        else
        {
            ColorCodeSlot.SetActive(false);
        }
    }

    public void Pause()
    {
        Time.timeScale = 0;
        Gun.enabled = false;
        player.enabled = false;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    public void Resume()
    {
        Time.timeScale = 1;
        Gun.enabled = true;
        player.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    #region inventory
    public void OnInventory(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (onoff == 1)
            {
                Debug.Log("Abre");
                Inventory.SetActive(true);
                onoff++;
                Pause();
            }
            else
            {
                Debug.Log("Cierra");
                Inventory.SetActive(false);
                onoff--;
                Resume();
            }
        }
    }
}
    #endregion
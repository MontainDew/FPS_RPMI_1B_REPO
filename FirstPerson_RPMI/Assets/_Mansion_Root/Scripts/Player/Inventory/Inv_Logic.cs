using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Inv_Logic : MonoBehaviour
{
    [Header("Checker")]
    [SerializeField] private bool Camera;
    [SerializeField] private int FuseCuantity;
    [SerializeField] private bool HColorCode;
    [SerializeField] private bool ColorCode;
    [SerializeField] private bool DoorCode;

    [Header("ItemSlots")]
    [SerializeField] private GameObject CameraSlot;
    [SerializeField] private GameObject FuseSlot;
    [SerializeField] private TMP_Text FuseCuantityText;
    [SerializeField] private GameObject HColorCodeSlot;
    [SerializeField] private GameObject ColorCodeSlot;
    [SerializeField] private GameObject DoorCodeSlot;

    [Header("Other")]
    [SerializeField]private GameObject Inventory;
    [SerializeField]private int onoff = 1;

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
        if (Camera)
        {
            CameraSlot.SetActive(true);
        }
        if (ColorCode)
        {
            ColorCodeSlot.SetActive(true);
        }
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
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
                onoff++;
            }
            else
            {
                Debug.Log("Cierra");
                Inventory.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                onoff--;
            }
        }
    }
    #endregion
}

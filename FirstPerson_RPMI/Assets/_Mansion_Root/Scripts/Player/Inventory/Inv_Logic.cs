using UnityEngine;
using UnityEngine.InputSystem;

public class Inv_Logic : MonoBehaviour
{
    [SerializeField] private GameObject Inventory;
    [SerializeField]private int onoff = 1;
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

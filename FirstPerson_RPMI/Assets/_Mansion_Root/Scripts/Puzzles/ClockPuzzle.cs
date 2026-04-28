using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class ClockPuzzle : MonoBehaviour
{
    public TextMeshProUGUI displayText;
    public Inv_Logic Inventario;

    [SerializeField] string currentInput = "";
    [SerializeField] string correctCode = "1120";

    public void AddNumber(string number)
    {
        currentInput += number;
        UpdateDisplay();
        
    }

   public void ClearInput()
    {
        currentInput = "";
        UpdateDisplay();
    }

    public void SubmitCode()
    {
        if (currentInput == correctCode)
        {
            Debug.Log("Codigo correcto");
            Inventario.DoorCode = true;
        }
        else
        {
            Debug.Log("Codigo incorrecto");
            Fail();
        }
    }
    void UpdateDisplay()
    {
        displayText.text = currentInput;
    }
    void Fail()
    {
        currentInput = "";
        UpdateDisplay();
    }
}

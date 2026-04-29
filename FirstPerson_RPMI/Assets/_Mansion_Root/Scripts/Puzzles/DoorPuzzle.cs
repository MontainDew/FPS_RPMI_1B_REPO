using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class DoorPuzzle : MonoBehaviour
{
    public TextMeshProUGUI displayText;

    [SerializeField] string currentInput = "";
    [SerializeField] string correctCode = "2389";

    public void AddNumber(string number)
    {
        if (currentInput.Length < 4)
        {
            currentInput += number;
            UpdateDisplay();
        }
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
            Debug.Log("Pli Open de dor");
        }
        else
        {
            Debug.Log("Fua que liada colega");
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
    private void Update()
    {
        foreach (char c in Input.inputString)
        {
            if (char.IsDigit(c))
            {
                AddNumber(c.ToString());
            }
            if (c == '\b' && currentInput.Length >= 4)
            {
                ClearInput();
            }
            if (c == '\n' || c == '\r')
            {
                SubmitCode();
            }
        }
    }
}

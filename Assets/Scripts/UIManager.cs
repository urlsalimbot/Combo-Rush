using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreDisplay;
    [SerializeField] private TextMeshProUGUI comboDisplay;
     
    public void SetScoreDisplay(int display){
     scoreDisplay.text = display.ToString();   
    } 

    public void SetComboDisplay(int display){
     comboDisplay.text = display.ToString();   
    } 

}

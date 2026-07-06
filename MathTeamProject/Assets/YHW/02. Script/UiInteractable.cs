using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiInteractable : MonoBehaviour
{
    [Header("Slider")]
    public Slider amplitudeSlider;
    public Slider frequencySlider;
    [Header("DropDown")]
    public TMP_Dropdown dropdown;
    
    public void SetUiInteractable(bool interactable)
    {
        amplitudeSlider.interactable = interactable;
        frequencySlider.interactable = interactable;
        dropdown.interactable = interactable;
    }
}

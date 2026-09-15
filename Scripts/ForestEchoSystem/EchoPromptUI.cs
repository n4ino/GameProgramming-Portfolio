using TMPro;
using UnityEngine;

/*
* EchoPromptUI
*
* Manages the interaction prompt displayed when
* the player is near an Echo and can recover
* lost currency.
*/

public class EchoPromptUI : MonoBehaviour
{
    public static EchoPromptUI Instance;

    [SerializeField] private TextMeshProUGUI echoText;

    private void Awake()
    {
        Instance = this;
        
        // Hide the prompt by default when the scene loads.
        Hide();
    }

    public void Show()
    {
        if (echoText != null)
            echoText.gameObject.SetActive(true);
    }

    public void Hide()
    { 
        if (echoText != null)
            echoText.gameObject.SetActive(false);
    }
}

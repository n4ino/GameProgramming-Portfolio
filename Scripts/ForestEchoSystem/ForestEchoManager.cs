using Unity.VisualScripting;
using UnityEngine;

/*
* ForestEchoManager
*
* Manages Echo spawning and recovery.
* Stores the player's lost currency on death,
* handles currency recovery, and ensures that
* only one active Echo exists at a time.
*/

public class ForestEchoManager : MonoBehaviour
{
   public static ForestEchoManager Instance { get; private set; }   

   [SerializeField] private GameObject echoPrefab;
    private Echo activeEcho = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
    }

    public void SpawnEcho(Vector3 position)
    {
        Debug.Log("[ECHO SPAWN] Spawning echo at " + position);

        // Remove the previous Echo if the player died before recovering it.
        if (activeEcho != null)     
            Destroy(activeEcho.gameObject);

        // Store all currency currently carried by the player.
        int lostAmount = PlayerCurrency.Instance.TakeAll();

        // Create a new Echo at the player's death location.
        GameObject echoObj = Instantiate(echoPrefab, position, Quaternion.identity);
        activeEcho = echoObj.GetComponent<Echo>();

        // Save the lost currency amount inside the Echo.
        activeEcho.StoreCurrency(lostAmount);

        Debug.Log("[Echo] spawned at " + position + " with " + lostAmount + " berries");
    }

    public void RecoverEcho()
    {
        Debug.Log("[MANAGER] RecoverEcho CALLED, activeEcho = " + activeEcho);
        if (activeEcho == null) return;

        // Return stored currency to the player and remove the Echo.
        activeEcho.Recover();
        Destroy(activeEcho.gameObject);
        activeEcho = null;

        // Hide the prompt after recovering the Echo.
        EchoPromptUI.Instance?.Hide();
    }

    public void ClearEcho()
    {
        if (activeEcho != null)
        {
            // Remove the currently active Echo.
            Destroy(activeEcho.gameObject); 
            activeEcho = null;
        }
    }
}

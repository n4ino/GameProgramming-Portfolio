using UnityEngine;
using UnityEngine.Video;
using System.Collections;

/*
* IntroCutsceneStarter
*
* Automatically starts an intro cutscene when the scene loads.
* Supports an optional delay before playback begins.
*/

public class IntroCutsceneStarter : MonoBehaviour
{
    [SerializeField] private VideoClip introClip;

    [Tooltip("Delay before the intro cutscene starts.")]
    [SerializeField] private float delayBeforeCutscene = 0.0f; 

    private IEnumerator Start()
    {
        // Wait before starting the intro cutscene.
        yield return new WaitForSeconds(delayBeforeCutscene);

        if (introClip != null && CutsceneManager.Instance != null)
        {
            CutsceneManager.Instance.PlayCutscene(introClip);
        }
        else
        {
            Debug.LogWarning("CutsceneManager or intro video clip is missing.");
        }
    }
}

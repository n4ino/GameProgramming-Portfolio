using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

/*
* CutsceneManager
*
* Manages video cutscene playback across scenes.
* Handles player control locking, UI visibility,
* audio muting, scene transitions, and cutscene
* queueing between scenes.
*/

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance;
    private static VideoClip queuedCutscene;
    private static GameObject queuedCutsceneBlackout;

    [Header("Components")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private AudioSource audioSource;

    [Header("UI")]
    [SerializeField] private GameObject uiRoot;
    [SerializeField] private bool autoFindUiRoot = true;

    [Header("Optional player lock")]
    [SerializeField] private PlayerController playerController;

#if UNITY_EDITOR
    [Header("DEBUG (Editor only)")]
    [SerializeField] private VideoClip testClip;
#endif

    private bool cutscenePlaying;
    private Action onFinishedCallback;
    private bool hiddenUiRoot;
    private bool uiRootWasActive;
    private bool hideBlackoutOnFirstFrame;
    private readonly List<AudioSourceMuteState> mutedAudioSources = new List<AudioSourceMuteState>();

    // Queue a cutscene to be played automatically after a scene transition.
    public static void QueueCutsceneForNextManager(VideoClip clip)
    {
        queuedCutscene = clip;

        if (clip != null)
            ShowQueuedCutsceneBlackout();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.loopPointReached += HandleCutsceneFinished;
            videoPlayer.frameReady += HandleFirstCutsceneFrameReady;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void Start()
    {
        if (queuedCutscene != null)
            StartCoroutine(PlayQueuedCutsceneRoutine());
    }

    public void PlayCutscene(VideoClip clip, Action onFinished = null)
    {
        if (cutscenePlaying || clip == null || videoPlayer == null)
            return;

        ResolveSceneReferences();

        cutscenePlaying = true;
        onFinishedCallback = onFinished;

        // Pause gameplay systems while the cutscene is active.
        GameTime.SetCutscene(true);

        if (playerController != null)
            playerController.movementLocked = true;

        HideUi();
        MuteGameAudio();

        videoPlayer.clip = clip;
        videoPlayer.sendFrameReadyEvents = true;
        hideBlackoutOnFirstFrame = queuedCutsceneBlackout != null;

        if (audioSource != null)
        {
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.SetTargetAudioSource(0, audioSource);
            audioSource.Play();
        }

        videoPlayer.Play();
    }

    private void HandleCutsceneFinished(VideoPlayer vp)
    {
        StopCutscene();
    }

    public void StopCutscene()
    {
        if (videoPlayer != null)
            videoPlayer.Stop();

        if (audioSource != null)
            audioSource.Stop();

        RestoreGameAudio();

        GameTime.SetCutscene(false);

        if (playerController != null)
            playerController.movementLocked = false;

        RestoreUi();

        cutscenePlaying = false;
        HideQueuedCutsceneBlackout();

        onFinishedCallback?.Invoke();
        onFinishedCallback = null;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        uiRoot = null;
        playerController = null;

        if (videoPlayer != null &&
            (videoPlayer.renderMode == VideoRenderMode.CameraNearPlane ||
             videoPlayer.renderMode == VideoRenderMode.CameraFarPlane))
        {
            videoPlayer.targetCamera = null;
        }

        if (queuedCutscene != null)
            StartCoroutine(PlayQueuedCutsceneRoutine());
    }

    // Hide the loading blackout once the first cutscene frame is visible.
    private void HandleFirstCutsceneFrameReady(VideoPlayer vp, long frameIdx)
    {
        if (!hideBlackoutOnFirstFrame)
            return;

        hideBlackoutOnFirstFrame = false;
        HideQueuedCutsceneBlackout();
    }

    private void ResolveSceneReferences()
    {
        if (playerController == null)
            playerController = UnityEngine.Object.FindFirstObjectByType<PlayerController>();

        if (videoPlayer != null &&
            (videoPlayer.renderMode == VideoRenderMode.CameraNearPlane ||
             videoPlayer.renderMode == VideoRenderMode.CameraFarPlane) &&
            videoPlayer.targetCamera == null)
        {
            videoPlayer.targetCamera = Camera.main;
        }

        if (!autoFindUiRoot || uiRoot != null)
            return;

        GameObject uiObject = GameObject.Find("UI");
        if (uiObject != null)
            uiRoot = uiObject;
    }

    // Wait one frame to allow scene initialization before playback.
    private IEnumerator PlayQueuedCutsceneRoutine()
    {
        yield return null;

        if (queuedCutscene == null)
            yield break;

        VideoClip clip = queuedCutscene;
        queuedCutscene = null;

        PlayCutscene(clip);
    }

    // Create a fullscreen black overlay to hide scene loading before the cutscene starts.
    private static void ShowQueuedCutsceneBlackout()
    {
        if (queuedCutsceneBlackout != null)
            return;

        queuedCutsceneBlackout = new GameObject("QueuedCutsceneBlackout", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(Image));
        UnityEngine.Object.DontDestroyOnLoad(queuedCutsceneBlackout);

        Canvas canvas = queuedCutsceneBlackout.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        CanvasScaler scaler = queuedCutsceneBlackout.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        Image image = queuedCutsceneBlackout.GetComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = true;

        RectTransform rect = queuedCutsceneBlackout.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void HideQueuedCutsceneBlackout()
    {
        if (queuedCutsceneBlackout == null)
            return;

        UnityEngine.Object.Destroy(queuedCutsceneBlackout);
        queuedCutsceneBlackout = null;
    }

    private void HideUi()
    {
        if (uiRoot == null)
            return;

        hiddenUiRoot = true;
        uiRootWasActive = uiRoot.activeSelf;
        uiRoot.SetActive(false);
    }

    private void RestoreUi()
    {
        if (!hiddenUiRoot || uiRoot == null)
            return;

        uiRoot.SetActive(uiRootWasActive);
        hiddenUiRoot = false;
    }

    // Temporarily mute all other AudioSources while the cutscene is playing.
    private void MuteGameAudio()
    {
        mutedAudioSources.Clear();

        AudioSource[] sources = UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource source in sources)
        {
            if (source == null || source == audioSource)
                continue;

            mutedAudioSources.Add(new AudioSourceMuteState(source, source.mute));
            source.mute = true;
        }
    }

    // Restore each AudioSource to its previous mute state.
    private void RestoreGameAudio()
    {
        foreach (AudioSourceMuteState state in mutedAudioSources)
        {
            if (state.Source != null)
                state.Source.mute = state.WasMuted;
        }

        mutedAudioSources.Clear();
    }

    private readonly struct AudioSourceMuteState
    {
        public readonly AudioSource Source;
        public readonly bool WasMuted;

        public AudioSourceMuteState(AudioSource source, bool wasMuted)
        {
            Source = source;
            WasMuted = wasMuted;
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (!cutscenePlaying && testClip != null && Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
            PlayCutscene(testClip);
    }
#endif
}

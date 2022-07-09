using OWML.ModHelper;
using UnityEngine.SceneManagement;
namespace AutoResume
{
    class AutoResume : ModBehaviour
    {
        private bool _isAwake = false;

        private void Start()
        {
            // Skip flash screen.
            var titleScreenAnimation = FindObjectOfType<TitleScreenAnimation>();
            titleScreenAnimation._fadeDuration = 0;
            titleScreenAnimation._gamepadSplash = false;
            titleScreenAnimation._introPan = false;

            var titleScreenManager = FindObjectOfType<TitleScreenManager>();
            titleScreenManager.FadeInTitleLogo();

            // Skip menu fade.
            var titleAnimationController = FindObjectOfType<TitleAnimationController>();
            titleAnimationController._logoFadeDelay = 0.001f;
            titleAnimationController._logoFadeDuration = 0.001f;
            titleAnimationController._optionsFadeDelay = 0.001f;
            titleAnimationController._optionsFadeDuration = 0.001f;
            titleAnimationController._optionsFadeSpacing = 0.001f;

            // Need to wait a little bit for some reason.
            ModHelper.Events.Unity.FireOnNextUpdate(Resume);

            GlobalMessenger.AddListener("WakeUp", OnWakeUp);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            GlobalMessenger.RemoveListener("WakeUp", OnWakeUp);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Resume()
        {
            // Simulate "resume game" button press.
            FindObjectOfType<TitleScreenManager>()._resumeGameAction.ConfirmSubmit();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "SolarSystem") ModHelper.Events.Unity.FireOnNextUpdate(SkipWakeUp);

            _isAwake = false;
        }

        private void SkipWakeUp()
        {
            if (_isAwake) return;

            // Skip wake up animation.
            var cameraEffectController = FindObjectOfType<PlayerCameraEffectController>();
            cameraEffectController.OpenEyes(0, true);
            cameraEffectController._wakeLength = 0f;
            cameraEffectController._waitForWakeInput = false;

            // Skip wake up prompt.
            LateInitializerManager.pauseOnInitialization = false;
            Locator.GetPauseCommandListener().RemovePauseCommandLock();
            Locator.GetPromptManager().RemoveScreenPrompt(cameraEffectController._wakePrompt);
            OWTime.Unpause(OWTime.PauseType.Sleeping);
            cameraEffectController.WakeUp();

            // Enable all inputs immediately.
            OWInput.ChangeInputMode(InputMode.Character);
            (OWInput.SharedInputManager as InputManager)._inputFadeFraction = 0f;
            GlobalMessenger.FireEvent("TakeFirstFlashbackSnapshot");
        }

        private void OnWakeUp()
        {
            _isAwake = true;
        }
    }
}


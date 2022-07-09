using System;
using OWML;
using OWML.ModHelper;
using OWML.ModHelper.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace AutoResume
{
    class AutoResume : ModBehaviour
    {
        bool _isOpenEyesSkipped = false;
        bool _isSolarSystemLoaded = false;

        private void Start()
        {
            // Skip flash screen.
            var titleScreenAnimation = FindObjectOfType<TitleScreenAnimation>();
            titleScreenAnimation.SetValue("_fadeDuration", 0);
            titleScreenAnimation.SetValue("_gamepadSplash", false);
            titleScreenAnimation.SetValue("_introPan", false);
            titleScreenAnimation.Invoke("FadeInTitleLogo");

            // Skip menu fade.
            var titleAnimationController = FindObjectOfType<TitleAnimationController>();
            titleAnimationController.SetValue("_logoFadeDelay", 0.001f);
            titleAnimationController.SetValue("_logoFadeDuration", 0.001f);
            titleAnimationController.SetValue("_optionsFadeDelay", 0.001f);
            titleAnimationController.SetValue("_optionsFadeDuration", 0.001f);
            titleAnimationController.SetValue("_optionsFadeSpacing", 0.001f);

            // Need to wait a little bit for some reason.
            Invoke("Resume", 0.5f);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Resume()
        {
            // Simulate "resume game" button press.
            var resume = FindObjectOfType<TitleScreenManager>().GetValue<SubmitActionLoadScene>("_resumeGameAction");
            resume.Invoke("ConfirmSubmit");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "SolarSystem")
            {
                _isSolarSystemLoaded = true;
                _isOpenEyesSkipped = false;
            }
        }

        private void LateUpdate()
        {
            if (!_isOpenEyesSkipped && _isSolarSystemLoaded)
            {
                _isOpenEyesSkipped = true;

                // Skip wake up animation.
                var cameraEffectController = FindObjectOfType<PlayerCameraEffectController>();
                cameraEffectController.OpenEyes(0, true);
                cameraEffectController.SetValue("_wakeLength", 0f);
                cameraEffectController.SetValue("_waitForWakeInput", false);

                // Skip wake up prompt.
                LateInitializerManager.pauseOnInitialization = false;
                Locator.GetPauseCommandListener().RemovePauseCommandLock();
                Locator.GetPromptManager().RemoveScreenPrompt(cameraEffectController.GetValue<ScreenPrompt>("_wakePrompt"));
                OWTime.Unpause(OWTime.PauseType.Sleeping);
                cameraEffectController.Invoke("WakeUp");

                // Enable all inputs immedeately.
                OWInput.ChangeInputMode(InputMode.Character);
                typeof(OWInput).SetValue("_inputFadeFraction", 0f);
                GlobalMessenger.FireEvent("TakeFirstFlashbackSnapshot");
            }
        }
    }

}


using System;
using OWML;
using OWML.ModHelper;
using OWML.ModHelper.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace AutoResume
{
    class MainBehaviour : ModBehaviour
    {
        //private void Start()
        //{
        //    ModHelper.Console.WriteLine("Skipping splash screen...");
        //    var titleScreenAnimation = FindObjectOfType<TitleScreenAnimation>();
        //    titleScreenAnimation.SetValue("_fadeDuration", 0);
        //    titleScreenAnimation.SetValue("_gamepadSplash", false);
        //    titleScreenAnimation.SetValue("_introPan", false);
        //    titleScreenAnimation.Invoke("FadeInTitleLogo");
        //    ModHelper.Console.WriteLine("Done!");


        //    foreach (var item in FindObjectsOfType<Selectable>())
        //    {
        //        if (item.name.ToLower().Contains("resume"))
        //        {
        //            item.GetComponent<SubmitActionLoadScene>().Invoke("ConfirmSubmit");

        //        }
        //    }

        //    //FindObjectOfType<SubmitActionLoadScene>().Invoke("ConfirmSubmit");

        //    ModHelper.Events.Scenes.OnCompleteSceneChange += OnCompleteSceneChange;
        //}

        //void LateStuff()
        //{
        //    //ModHelper.Console.WriteLine("Finding _resumeGameAction...");
        //    //var loadAction = typeof(TitleScreenManager).GetField("_resumeGameAction");
        //    //ModHelper.Console.WriteLine("Done!");

        //    //ModHelper.Console.WriteLine("Finding TitleScreenManager...");
        //    //loadAction.GetValue(FindObjectOfType<TitleScreenManager>()).Invoke("ConfirmSubmit");
        //    //ModHelper.Console.WriteLine("Done!");

        //}

        //private void OnCompleteSceneChange(OWScene oldScene, OWScene newScene)
        //{
        //   if(newScene == OWScene.SolarSystem)
        //    {
        //        FindObjectOfType<PlayerCameraEffectController>().OpenEyes(0.5f, true);
        //    }
        //}

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


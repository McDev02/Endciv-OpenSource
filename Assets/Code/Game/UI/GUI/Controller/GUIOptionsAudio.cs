using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CanvasGroup))]
    [DisallowMultipleComponent]
    public class GUIOptionsAudio : GUIAnimatedPanel
    {
        AudioManager audioManager;       

        [SerializeField] Slider TotalVolume;
        [SerializeField] Slider MusicVolume;
        [SerializeField] Slider SoundVolume;
        [SerializeField] Slider UIVolume;

        private bool canUpdateAudio = true;

        public void Setup(AudioManager audioManager)
        {
            this.audioManager = audioManager;
            canUpdateAudio = false;
            UpdateUI();
            canUpdateAudio = true;
        }

        private void OnEnable()
        {
            canUpdateAudio = false;
            UpdateUI();
            canUpdateAudio = true;
            OnUpdateSettings();
        }

        public void OnUpdateSettings()
        {
            if (!canUpdateAudio)
                return;
            audioManager.MasterVolume = TotalVolume.value;
            audioManager.MusicVolume = MusicVolume.value;
            audioManager.SoundVolume = SoundVolume.value;
            audioManager.SpeechVolume = UIVolume.value;
        }

        public void OnPresetChanged()
        {            
            OnUpdateSettings();
        }

        public void DiscardValues()
        {
            //Set everything back to before changes were made
            Main.Instance.audioManager.DiscardTemporaryValues();
            OnUpdateSettings();
            UpdateUI();
        }

        public void ApplyValues()
        {
            //Apply temporary changes and write to disk.
            UpdateTempSettings();
            Main.Instance.audioManager.ApplyTemporaryValues(false);
            OnUpdateSettings();
        }

        void UpdateTempSettings()
        {            
            audioManager.TmpSettings.totalVolume = TotalVolume.value;
            audioManager.TmpSettings.musicVolume = MusicVolume.value;
            audioManager.TmpSettings.soundVolume = SoundVolume.value;
            audioManager.TmpSettings.uiVolume = UIVolume.value;
        }

        void UpdateUI()
        {
            if (audioManager.TmpSettings == null)
                return;
            TotalVolume.value = audioManager.TmpSettings.totalVolume;
            MusicVolume.value = audioManager.TmpSettings.musicVolume;
            SoundVolume.value = audioManager.TmpSettings.soundVolume;
            UIVolume.value = audioManager.TmpSettings.uiVolume;
        }
    }
}
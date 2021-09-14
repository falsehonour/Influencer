using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HashtagChampion
{
    public class MatchSettingsMenuManager : MenuManager
    {
        private MatchSettings settings;

        [SerializeField] private UISettingsController maxPlayerCountController;
        [SerializeField] private UISettingsController countdownController;
        [SerializeField] private UISettingsController initialPickupsController;
        [SerializeField] private UISettingsController taggerSpeedController;

        private void Start()
        {
            Initialise();
        }

        public override void Activate()
        {
            Debug.Log("MatchSettings Activate");
            base.Activate();
            ResetToDefault();
        }

        private void Initialise()
        {
            settings = new MatchSettings();
            //TODO: There should be a wat to automate this

            maxPlayerCountController.Initialise(
                (delegate (sbyte value)
                {
                    settings.maxPlayerCount = (MaxPlayerCount)value;
                })
                , typeof(MaxPlayerCount));

            countdownController.Initialise(
                (delegate (sbyte value)
                {
                    settings.countdown = (CountdownValues)value;
                })
               , typeof(CountdownValues));

            initialPickupsController.Initialise(
               (delegate (sbyte value)
               {
                   settings.initialPickups = (InitialPickups)value;
               })
              , typeof(InitialPickups));

            taggerSpeedController.Initialise(
               (delegate (sbyte value)
               {
                   settings.taggerSpeed = (TaggerSpeedValues)value;
               })
              , typeof(TaggerSpeedValues));

        }

        /*private System.Action<sbyte> CreateControllerAction(ref sbyte modified)
        {
            return
            (delegate (sbyte value)
            {
                modified = value;
            });
        }*/

        private void SetSettingsControllersValues()
        {
            Debug.Log("SetSettingsControllersValues");
            maxPlayerCountController.SetValue((sbyte)settings.maxPlayerCount, false);
            countdownController.SetValue((sbyte)settings.countdown, false);
            initialPickupsController.SetValue((sbyte)settings.initialPickups, false);
            taggerSpeedController.SetValue((sbyte)settings.taggerSpeed, false);

        }

        public void Host()
        {
            Player.localPlayer.HostMatch(settings);
        }

        public void ResetToDefault()
        {
            settings.SetDefaultValues();
            SetSettingsControllersValues();
        }
    }
}


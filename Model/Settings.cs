namespace Model.Properties {
    
    
    // This class allows you to handle specific events on the settings class:
    //  The SettingChanging event is raised before a setting's value is changed.
    //  The PropertyChanged event is raised after a setting's value is changed.
    //  The SettingsLoaded event is raised after the setting values are loaded.
    //  The SettingsSaving event is raised before the setting values are saved.
    internal sealed partial class Settings {
        
        public Settings() {
      // // To add event handlers for saving and changing settings, uncomment the lines below:
      //
      // this.SettingChanging += this.SettingChangingEventHandler;
      //
      // this.SettingsSaving += this.SettingsSavingEventHandler;
      //

      SettingsLoaded += Settings_SettingsLoaded;
        }

    private void Settings_SettingsLoaded(object sender, System.Configuration.SettingsLoadedEventArgs e)
    {
#if (DEBUG)
      //this["ServerURL"] = "http://hfq/api/v1/";
      this["ServerURL"] = "https://hfqserver.com/api/v1/";
#else
      this["ServerURL"] = "https://hfqserver.com/api/v1/";
#endif
    }

    private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e) {
            // Add code to handle the SettingChangingEvent event here.
        }
        
        private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e) {
            // Add code to handle the SettingsSaving event here.
        }
    }
}
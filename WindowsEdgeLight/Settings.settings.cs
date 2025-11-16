namespace WindowsEdgeLight.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("6500")]
        public int LightTemperature {
            get {
                return ((int)(this["LightTemperature"]));
            }
            set {
                this["LightTemperature"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public double LightBrightness {
            get {
                return ((double)(this["LightBrightness"]));
            }
            set {
                this["LightBrightness"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("6500")]
        public int Preset1Temperature {
            get {
                return ((int)(this["Preset1Temperature"]));
            }
            set {
                this["Preset1Temperature"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public double Preset1Brightness {
            get {
                return ((double)(this["Preset1Brightness"]));
            }
            set {
                this["Preset1Brightness"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("4500")]
        public int Preset2Temperature {
            get {
                return ((int)(this["Preset2Temperature"]));
            }
            set {
                this["Preset2Temperature"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0.7")]
        public double Preset2Brightness {
            get {
                return ((double)(this["Preset2Brightness"]));
            }
            set {
                this["Preset2Brightness"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2700")]
        public int Preset3Temperature {
            get {
                return ((int)(this["Preset3Temperature"]));
            }
            set {
                this["Preset3Temperature"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0.5")]
        public double Preset3Brightness {
            get {
                return ((double)(this["Preset3Brightness"]));
            }
            set {
                this["Preset3Brightness"] = value;
            }
        }
    }
}

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PokemonBag.Utils;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PokemonBag.Logic
{
    public class ApplicationSettings
    {
        [JsonIgnore]
        internal DeviceSettings Device = new DeviceSettings();

        [JsonIgnore]
        public string ProfileConfigPath;

        [JsonIgnore]
        public string ConfigFile;

        public AuthType AuthType;
        public string GoogleUsername;
        public string GooglePassword;
        public string PtcUsername;
        public string PtcPassword;
        public bool IsAutoLogin;
        public bool UseProxy;
        public string UseProxyHost;
        public string UseProxyPort;
        public bool UseProxyAuthentication;
        public string UseProxyUsername;
        public string UseProxyPassword;

        public ApplicationSettings()
        {
            InitializePropertyDefaultValues(this);
        }

        public void InitializePropertyDefaultValues(object obj)
        {
            FieldInfo[] fields = obj.GetType().GetFields();

            foreach (FieldInfo field in fields)
            {
                var d = field.GetCustomAttribute<DefaultValueAttribute>();

                if (d != null)
                    field.SetValue(obj, d.Value);
            }
        }

        public static ApplicationSettings Load()
        {
            ApplicationSettings settings = null;
            var profileConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "config");
            var configFile = Path.Combine(profileConfigPath, "config.json");


            if (File.Exists(configFile))
            {
                try
                {
                    //if the file exists, load the settings
                    var input = File.ReadAllText(configFile);

                    var jsonSettings = new JsonSerializerSettings();
                    jsonSettings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
                    jsonSettings.ObjectCreationHandling = ObjectCreationHandling.Replace;
                    jsonSettings.DefaultValueHandling = DefaultValueHandling.Populate;

                    settings = JsonConvert.DeserializeObject<ApplicationSettings>(input, jsonSettings);

                }
                catch (JsonReaderException exception)
                {
                    return null;
                }
            }
            else
            {
                settings = new ApplicationSettings();
            }

            settings.ProfileConfigPath = profileConfigPath;
            settings.ConfigFile = configFile; 

            settings.Save(configFile);
            settings.Device.Load(Path.Combine(profileConfigPath, "device.json"));

            return settings;
        }

        public void Save(string fullPath)
        {
            var jsonSerializeSettings = new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Include,
                Formatting = Formatting.Indented,
                Converters = new JsonConverter[] { new StringEnumConverter { CamelCaseText = true } }
            };

            var output = JsonConvert.SerializeObject(this, jsonSerializeSettings);

            var folder = Path.GetDirectoryName(fullPath);
            if (folder != null && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllText(fullPath, output);
        }

    }

    internal class DeviceSettings
    {
        [JsonIgnore]
        private string _filePath;

        [DefaultValue("random")]
        public string DevicePackageName;

        [DefaultValue("8525f5d8201f78b5")]
        public string DeviceId;

        [DefaultValue("msm8996")]
        public string AndroidBoardName;

        [DefaultValue("1.0.0.0000")]
        public string AndroidBootloader;

        [DefaultValue("HTC")]
        public string DeviceBrand;

        [DefaultValue("HTC 10")]
        public string DeviceModel;

        [DefaultValue("pmewl_00531")]
        public string DeviceModelIdentifier;

        [DefaultValue("qcom")]
        public string DeviceModelBoot;

        [DefaultValue("HTC")]
        public string HardwareManufacturer;

        [DefaultValue("HTC 10")]
        public string HardwareModel;

        [DefaultValue("pmewl_00531")]
        public string FirmwareBrand;

        [DefaultValue("release-keys")]
        public string FirmwareTags;

        [DefaultValue("user")]
        public string FirmwareType;

        [DefaultValue("htc/pmewl_00531/htc_pmewl:6.0.1/MMB29M/770927.1:user/release-keys")]
        public string FirmwareFingerprint;


        public DeviceSettings()
        {
            InitializePropertyDefaultValues(this);
        }

        public void InitializePropertyDefaultValues(object obj)
        {
            FieldInfo[] fields = obj.GetType().GetFields();

            foreach (FieldInfo field in fields)
            {
                var d = field.GetCustomAttribute<DefaultValueAttribute>();

                if (d != null)
                    field.SetValue(obj, d.Value);
            }
        }

        public void Load(string path)
        {
            try
            {
                _filePath = path;

                if (File.Exists(_filePath))
                {
                    //if the file exists, load the settings
                    var input = File.ReadAllText(_filePath);

                    var settings = new JsonSerializerSettings();
                    settings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
                    JsonConvert.PopulateObject(input, this, settings);
                }
                // Do some post-load logic to determine what device info to be using - if 'custom' is set we just take what's in the file without question
                if (!this.DevicePackageName.Equals("random", StringComparison.InvariantCultureIgnoreCase) && !this.DevicePackageName.Equals("custom", StringComparison.InvariantCultureIgnoreCase))
                {
                    // User requested a specific device package, check to see if it exists and if so, set it up - otherwise fall-back to random package
                    string keepDevId = this.DeviceId;
                    SetDevInfoByKey(this.DevicePackageName);
                    this.DeviceId = keepDevId;
                }
                if (this.DevicePackageName.Equals("random", StringComparison.InvariantCultureIgnoreCase))
                {
                    // Random is set, so pick a random device package and set it up - it will get saved to disk below and re-used in subsequent sessions
                    Random rnd = new Random();
                    int rndIdx = rnd.Next(0, DeviceInfoHelper.DeviceInfoSets.Keys.Count - 1);
                    this.DevicePackageName = DeviceInfoHelper.DeviceInfoSets.Keys.ToArray()[rndIdx];
                    SetDevInfoByKey(this.DevicePackageName);
                }
                if (string.IsNullOrEmpty(this.DeviceId) || this.DeviceId == "8525f5d8201f78b5")
                    this.DeviceId = this.RandomString(16, "0123456789abcdef"); // changed to random hex as full alphabet letters could have been flagged

                // Jurann: Note that some device IDs I saw when adding devices had smaller numbers, only 12 or 14 chars instead of 16 - probably not important but noted here anyway

                Save(_filePath);
            }
            catch (JsonReaderException exception)
            {
            }
        }

        public void Save(string fullPath)
        {
            var jsonSerializeSettings = new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Include,
                Formatting = Formatting.Indented,
                Converters = new JsonConverter[] { new StringEnumConverter { CamelCaseText = true } }
            };

            var output = JsonConvert.SerializeObject(this, jsonSerializeSettings);

            var folder = Path.GetDirectoryName(fullPath);
            if (folder != null && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllText(fullPath, output);
        }

        public void Save()
        {
            if (!string.IsNullOrEmpty(_filePath))
            {
                Save(_filePath);
            }
        }

        private string RandomString(int length, string alphabet = "abcdefghijklmnopqrstuvwxyz0123456789")
        {
            var outOfRange = Byte.MaxValue + 1 - (Byte.MaxValue + 1) % alphabet.Length;

            return string.Concat(
                Enumerable
                    .Repeat(0, Int32.MaxValue)
                    .Select(e => this.RandomByte())
                    .Where(randomByte => randomByte < outOfRange)
                    .Take(length)
                    .Select(randomByte => alphabet[randomByte % alphabet.Length])
            );
        }

        private byte RandomByte()
        {
            using (var randomizationProvider = new RNGCryptoServiceProvider())
            {
                var randomBytes = new byte[1];
                randomizationProvider.GetBytes(randomBytes);
                return randomBytes.Single();
            }
        }

        private void SetDevInfoByKey(string devKey)
        {
            if (DeviceInfoHelper.DeviceInfoSets.ContainsKey(this.DevicePackageName))
            {
                this.AndroidBoardName = DeviceInfoHelper.DeviceInfoSets[this.DevicePackageName]["AndroidBoardName"];
                this.AndroidBootloader = DeviceInfoHelper.DeviceInfoSets[this.DevicePackageName]["AndroidBootloader"];
                this.DeviceBrand = DeviceInfoHelper.DeviceInfoSets[this.DevicePackageName]["DeviceBrand"];
                this.DeviceId = DeviceInfoHelper.DeviceInfoSets[this.DevicePackageName]["DeviceId"];
                this.DeviceModel = DeviceInfoHelper.DeviceInfoSets[this.DevicePackageName]["DeviceModel"];
                this.DeviceModelBoot = DeviceInfoHelper.DeviceInfoSets[this.DevicePackageName]["DeviceModelBoot"];
                this.DeviceModelIdentifier = DeviceInfoHelper.DeviceInfoSets[this.DevicePackageName]["DeviceModelIdentifier"];
                this.FirmwareBrand = DeviceInfoHelper.DeviceInfoSets[this.DevicePackageName]["FirmwareBrand"];
                this.FirmwareFingerprint = DeviceInfoHelper.DeviceInfoSets[this.DevicePackageName]["FirmwareFingerprint"];
                this.FirmwareTags = DeviceInfoHelper.DeviceInfoSets[this.DevicePackageName]["FirmwareTags"];
                this.FirmwareType = DeviceInfoHelper.DeviceInfoSets[this.DevicePackageName]["FirmwareType"];
                this.HardwareManufacturer = DeviceInfoHelper.DeviceInfoSets[this.DevicePackageName]["HardwareManufacturer"];
                this.HardwareModel = DeviceInfoHelper.DeviceInfoSets[this.DevicePackageName]["HardwareModel"];
            }
            else
            {
                throw new ArgumentException("Invalid device info package! Check your device.config file and make sure a valid DevicePackageName is set. For simple use set it to 'random'. If you have a custom device, then set it to 'custom'.");
            }
        }
    }

    public class ClientSettings : ISettings
    {
        private readonly Random _rand = new Random();
        private readonly ApplicationSettings _settings;

        public ClientSettings(ApplicationSettings settings)
        {
            _settings = settings;
        }

        public string GoogleUsername => _settings.GoogleUsername;
        public string GooglePassword => _settings.GooglePassword;

        #region Auth Config Values

        public bool UseProxy
        {
            get { return _settings.UseProxy; }
            set { _settings.UseProxy = value; }
        }

        public string UseProxyHost
        {
            get { return _settings.UseProxyHost; }
            set { _settings.UseProxyHost = value; }
        }

        public string UseProxyPort
        {
            get { return _settings.UseProxyPort; }
            set { _settings.UseProxyPort = value; }
        }

        public bool UseProxyAuthentication
        {
            get { return _settings.UseProxyAuthentication; }
            set { _settings.UseProxyAuthentication = value; }
        }

        public string UseProxyUsername
        {
            get { return _settings.UseProxyUsername; }
            set { _settings.UseProxyUsername = value; }
        }

        public string UseProxyPassword
        {
            get { return _settings.UseProxyPassword; }
            set { _settings.UseProxyPassword = value; }
        }

        public string GoogleRefreshToken
        {
            get { return null; }
            set { GoogleRefreshToken = null; }
        }
        AuthType ISettings.AuthType
        {
            get { return _settings.AuthType; }

            set { _settings.AuthType = value; }
        }

        string ISettings.GoogleUsername
        {
            get { return _settings.GoogleUsername; }

            set { _settings.GoogleUsername = value; }
        }

        string ISettings.GooglePassword
        {
            get { return _settings.GooglePassword; }

            set { _settings.GooglePassword = value; }
        }

        string ISettings.PtcUsername
        {
            get { return _settings.PtcUsername; }

            set { _settings.PtcUsername = value; }
        }

        string ISettings.PtcPassword
        {
            get { return _settings.PtcPassword; }

            set { _settings.PtcPassword = value; }
        }

        #endregion Auth Config Values

        #region Device Config Values

        string DevicePackageName
        {
            get { return _settings.Device.DevicePackageName; }
            set { _settings.Device.DevicePackageName = value; }
        }
        string ISettings.DeviceId
        {
            get { return _settings.Device.DeviceId; }
            set { _settings.Device.DeviceId = value; }
        }
        string ISettings.AndroidBoardName
        {
            get { return _settings.Device.AndroidBoardName; }
            set { _settings.Device.AndroidBoardName = value; }
        }
        string ISettings.AndroidBootloader
        {
            get { return _settings.Device.AndroidBootloader; }
            set { _settings.Device.AndroidBootloader = value; }
        }
        string ISettings.DeviceBrand
        {
            get { return _settings.Device.DeviceBrand; }
            set { _settings.Device.DeviceBrand = value; }
        }
        string ISettings.DeviceModel
        {
            get { return _settings.Device.DeviceModel; }
            set { _settings.Device.DeviceModel = value; }
        }
        string ISettings.DeviceModelIdentifier
        {
            get { return _settings.Device.DeviceModelIdentifier; }
            set { _settings.Device.DeviceModelIdentifier = value; }
        }
        string ISettings.DeviceModelBoot
        {
            get { return _settings.Device.DeviceModelBoot; }
            set { _settings.Device.DeviceModelBoot = value; }
        }
        string ISettings.HardwareManufacturer
        {
            get { return _settings.Device.HardwareManufacturer; }
            set { _settings.Device.HardwareManufacturer = value; }
        }
        string ISettings.HardwareModel
        {
            get { return _settings.Device.HardwareModel; }
            set { _settings.Device.HardwareModel = value; }
        }
        string ISettings.FirmwareBrand
        {
            get { return _settings.Device.FirmwareBrand; }
            set { _settings.Device.FirmwareBrand = value; }
        }
        string ISettings.FirmwareTags
        {
            get { return _settings.Device.FirmwareTags; }
            set { _settings.Device.FirmwareTags = value; }
        }
        string ISettings.FirmwareType
        {
            get { return _settings.Device.FirmwareType; }
            set { _settings.Device.FirmwareType = value; }
        }
        string ISettings.FirmwareFingerprint
        {
            get { return _settings.Device.FirmwareFingerprint; }
            set { _settings.Device.FirmwareFingerprint = value; }
        }

        #endregion Device Config Values

        double ISettings.DefaultLatitude
        {
            get
            {
                return 40.778915;
            }

            set { }
        }

        double ISettings.DefaultLongitude
        {
            get
            {
                return -73.962277;
            }

            set { }
        }

        double ISettings.DefaultAltitude
        {
            get
            {
                return 10;
            }
            set { }
        }

        public void SaveSetting()
        {
            _settings.Save(_settings.ConfigFile);
        }
    }
}

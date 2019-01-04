using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Chromebook_Firmware_Update_Tool
{
    public partial class MainWindow : Window
    {
        private string manufacturer = "";
        public string model = "";
        private string trackpadType = "";
        private string humanReadableModel = "";
        private string chipset = "";
        private string executableDir = "";
        private int deviceType;
        private bool macAddressInjectionRequired;

        private string sha1OfFile(string path)
        {
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                using (BufferedStream bs = new BufferedStream((Stream)fs))
                {
                    using (SHA1Managed sha1 = new SHA1Managed())
                    {
                        byte[] hash = sha1.ComputeHash(bs);
                        StringBuilder formatted = new StringBuilder(2 * hash.Length);
                        foreach (byte b in hash)
                        {
                            formatted.AppendFormat("{0:X2}", b);
                        }
                        return formatted.ToString().ToLower();
                    }
                }
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        private bool checkHashes(bool validJSON)
        {
            if (this.executableDir == "")
                this.executableDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            bool flag = true;
            if (this.sha1OfFile(this.executableDir + "/FPT-HSWBDW/fparts.txt") != "d0740e8f6de1734e65f475b21d31f40461eb7a33")
                flag = false;
            if (this.sha1OfFile(this.executableDir + "/FPT-HSWBDW/fptw64.exe") != "f78e5e922a0c0e8e877e7627298981275a380dd7")
                flag = false;
            if (this.sha1OfFile(this.executableDir + "/FPT-HSWBDW/idrvdll32e.DLL") != "0ac170fff553339e9b1779e70e7eb05a43eced07")
                flag = false;
            if (this.sha1OfFile(this.executableDir + "/FPT-HSWBDW/pmxdll32e.DLL") != "dc009ce0a6ba77f19790c0b667fa4db5d742eda4")
                flag = false;
            if (this.sha1OfFile(this.executableDir + "/FPT-BYTBSW/fparts.txt") != "f73ba677e5e8debc6e0db8931e974e22653ac3d4")
                flag = false;
            if (this.sha1OfFile(this.executableDir + "/FPT-BYTBSW/fptw64.exe") != "cd5a8c0cdd83f450221611db0586680ee479823e")
                flag = false;
            if (this.sha1OfFile(this.executableDir + "/FPT-BYTBSW/idrvdll32e.DLL") != "0ac170fff553339e9b1779e70e7eb05a43eced07")
                flag = false;
            if (this.sha1OfFile(this.executableDir + "/FPT-BYTBSW/pmxdll32e.DLL") != "dc009ce0a6ba77f19790c0b667fa4db5d742eda4")
                flag = false;
            if (this.sha1OfFile(this.executableDir + "/cbtools/cbfstool.exe") != "a9aaba341eaeb1061585ffa1b1b264ef95dad096")
                flag = false;
            if (this.sha1OfFile(this.executableDir + "/cbtools/ifdtool.exe") != "745f05f821e4fc60b6c9a20cbd8bb9752e8ed428")
                flag = false;
            if (this.sha1OfFile(this.executableDir + "/cbtools/cygwin1.dll") != "2ff758b6387ced3e33c6c0bb77a9584dcc6b577f")
                flag = false;
            if (validJSON && this.sha1OfFile(this.executableDir + "/Newtonsoft.Json.dll") != "26c78dad612aff904f216f19f49089f84cc77eb8")
                flag = false;
            if (!flag)
            {
                this.modelSupportedLabel.Content = (object)"Internal Error: Checksum Mismatch";
                this.supportedImg.Source = (ImageSource)new BitmapImage(new Uri("unsupported.png", UriKind.Relative));
                this.updateBtn.IsEnabled = false;
            }
            return flag;
        }

        private bool isModelSupported()
        {
            string lower = this.model.ToLower();
            if (lower == "peppy")
            {
                this.humanReadableModel = "Acer C720(P)";
                this.chipset = "Intel Haswell";
                this.deviceType = 1;
                return true;
            }
            if (lower == "falco")
            {
                this.humanReadableModel = "HP Chromebook 14";
                this.chipset = "Intel Haswell";
                this.deviceType = 1;
                return true;
            }
            if (lower == "wolf")
            {
                this.humanReadableModel = "Dell Chromebook 11";
                this.chipset = "Intel Haswell";
                this.deviceType = 1;
                return true;
            }
            if (lower == "leon")
            {
                this.humanReadableModel = "Toshiba Chromebook 1";
                this.chipset = "Intel Haswell";
                this.deviceType = 1;
                return true;
            }
            if (lower == "mccloud")
            {
                this.humanReadableModel = "Acer Chromebox CXI";
                this.chipset = "Intel Haswell";
                this.deviceType = 2;
                this.macAddressInjectionRequired = true;
                return true;
            }
            if (lower == "monroe")
            {
                this.humanReadableModel = "LG Chromebase 22";
                this.chipset = "Intel Haswell";
                this.macAddressInjectionRequired = true;
                this.deviceType = 2;
                return true;
            }
            if (lower == "panther")
            {
                this.humanReadableModel = "Asus Chromebox CN60";
                this.chipset = "Intel Haswell";
                this.macAddressInjectionRequired = true;
                this.deviceType = 2;
                return true;
            }
            if (lower == "tricky")
            {
                this.humanReadableModel = "Dell Chromebox 3010";
                this.chipset = "Intel Haswell";
                this.macAddressInjectionRequired = true;
                this.deviceType = 2;
                return true;
            }
            if (lower == "zako")
            {
                this.humanReadableModel = "HP Chromebox CB1";
                this.chipset = "Intel Haswell";
                this.deviceType = 2;
                return true;
            }
            if (lower == "auron")
            {
                new AuronConfirm() { mainWindow = this }.ShowDialog();
                return this.isModelSupported();
            }
            if (lower == "auron_paine")
            {
                this.humanReadableModel = "Acer C740";
                this.chipset = "Intel Broadwell";
                this.deviceType = 1;
                return true;
            }
            if (lower == "auron_yuna")
            {
                this.humanReadableModel = "Acer C910/CB5-571";
                this.chipset = "Intel Broadwell";
                this.deviceType = 1;
                return true;
            }
            if (lower == "lulu")
            {
                this.humanReadableModel = "Dell Chromebook 13";
                this.chipset = "Intel Broadwell";
                this.deviceType = 1;
                return true;
            }
            if (lower == "gandof")
            {
                this.humanReadableModel = "Toshiba Chromebook 2 [2015]";
                this.chipset = "Intel Broadwell";
                this.deviceType = 1;
                return true;
            }
            if (lower == "samus")
            {
                this.humanReadableModel = "Google Pixel 2";
                this.chipset = "Intel Broadwell";
                this.deviceType = 1;
                return true;
            }
            if (lower == "buddy")
            {
                this.humanReadableModel = "Acer Chromebase 24";
                this.chipset = "Intel Broadwell";
                this.macAddressInjectionRequired = true;
                this.deviceType = 2;
                return true;
            }
            if (lower == "guado")
            {
                this.humanReadableModel = "Acer Chromebase CN62";
                this.chipset = "Intel Broadwell";
                this.macAddressInjectionRequired = true;
                this.deviceType = 2;
                return true;
            }
            if (lower == "rikku")
            {
                this.humanReadableModel = "Acer Chromebox CXI2";
                this.chipset = "Intel Broadwell";
                this.macAddressInjectionRequired = true;
                this.deviceType = 2;
                return true;
            }
            if (lower == "tidus")
            {
                this.humanReadableModel = "Lenovo ThinkCentre Chromebox";
                this.chipset = "Intel Broadwell";
                this.macAddressInjectionRequired = true;
                this.deviceType = 2;
                return true;
            }
            if (lower == "parrot")
            {
                this.humanReadableModel = "Acer C7/C710";
                this.chipset = "Intel Sandy/Ivy Bridge";
                this.deviceType = 1;
                return false;
            }
            if (lower == "butterfly")
            {
                this.humanReadableModel = "HP Pavilion 14";
                this.chipset = "Intel Sandy Bridge";
                this.deviceType = 1;
                return false;
            }
            if (lower == "lumpy")
            {
                this.humanReadableModel = "Samsung Series 5 550";
                this.chipset = "Intel Sandy Bridge";
                this.deviceType = 1;
                return false;
            }
            if (lower == "stout")
            {
                this.humanReadableModel = "Lenovo Thinkpad X131e";
                this.chipset = "Intel Ivy Bridge";
                this.deviceType = 1;
                return false;
            }
            if (lower == "link")
            {
                this.humanReadableModel = "Google Pixel 1";
                this.chipset = "Intel Ivy Bridge";
                this.deviceType = 1;
                return false;
            }
            if (lower == "stumpy")
            {
                this.humanReadableModel = "Samsung Chromebox Series 3";
                this.chipset = "Intel Sandy/Ivy Bridge";
                this.deviceType = 1;
                return false;
            }
            if (lower == "banjo")
            {
                this.humanReadableModel = "Acer Chromebook 15";
                this.chipset = "Intel Bay Trail";
                this.deviceType = 1;
                return true;
            }
            if (lower == "candy")
            {
                this.humanReadableModel = "Dell Chromebook 11";
                this.chipset = "Intel Bay Trail";
                this.deviceType = 1;
                return true;
            }
            if (lower == "clapper")
            {
                this.humanReadableModel = "Lenovo N20(P)";
                this.chipset = "Intel Bay Trail";
                this.deviceType = 1;
                return true;
            }
            if (lower == "enguarde")
            {
                this.humanReadableModel = "Lenovo N21";
                this.chipset = "Intel Bay Trail";
                this.deviceType = 1;
                return true;
            }
            if (lower == "gnawty")
            {
                this.humanReadableModel = "Acer C730/CB3-111/CB3-131";
                this.chipset = "Intel Bay Trail";
                this.deviceType = 1;
                return true;
            }
            if (lower == "heli")
            {
                this.humanReadableModel = "Haier Chromebook G2";
                this.chipset = "Intel Bay Trail";
                this.deviceType = 1;
                return true;
            }
            if (lower == "kip")
            {
                this.humanReadableModel = "HP Chromebook 11";
                this.chipset = "Intel Bay Trail";
                this.deviceType = 1;
                return true;
            }
            if (lower == "orco")
            {
                this.humanReadableModel = "Lenovo 100S";
                this.chipset = "Intel Bay Trail";
                this.deviceType = 1;
                return true;
            }
            if (lower == "quawks")
            {
                this.humanReadableModel = "Asus C300";
                this.chipset = "Intel Bay Trail";
                this.deviceType = 1;
                return true;
            }
            if (lower == "squawks")
            {
                this.humanReadableModel = "Asus C200";
                this.chipset = "Intel Bay Trail";
                this.deviceType = 1;
                return true;
            }
            if (lower == "swanky")
            {
                this.humanReadableModel = "Toshiba Chromebook 2 [2014]";
                this.chipset = "Intel Bay Trail";
                this.deviceType = 1;
                return true;
            }
            if (lower == "winky")
            {
                this.humanReadableModel = "Samsung Chromebook 2";
                this.chipset = "Intel Bay Trail";
                this.deviceType = 1;
                return true;
            }
            if (lower == "ninja")
            {
                this.humanReadableModel = "AOpen Chromebox Commercial";
                this.chipset = "Intel Bay Trail";
                this.macAddressInjectionRequired = true;
                this.deviceType = 2;
                return true;
            }
            if (lower == "sumo")
            {
                this.humanReadableModel = "AOpen Chromebase Commercial";
                this.chipset = "Intel Bay Trail";
                this.macAddressInjectionRequired = true;
                this.deviceType = 2;
                return true;
            }
            this.humanReadableModel = "Unknown: " + this.model;
            return false;
        }

        public MainWindow()
        {
            this.InitializeComponent();
            using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher((ObjectQuery)new SelectQuery("Select * from Win32_ComputerSystem")))
            {
                foreach (ManagementObject managementObject in managementObjectSearcher.Get())
                {
                    managementObject.Get();
                    this.manufacturer = (string)managementObject["Manufacturer"];
                    this.model = (string)managementObject["Model"];
                }
            }
            foreach (ManagementObject managementObject in new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity").Get())
            {
                object propertyValue1 = managementObject.GetPropertyValue("Present");
                if (propertyValue1 != null && (bool)propertyValue1)
                {
                    object propertyValue2 = managementObject.GetPropertyValue("HardwareID");
                    if (propertyValue2 != null)
                    {
                        string str = ((string[])propertyValue2)[0];
                        if (str.Equals("ACPI\\VEN_CYAP&DEV_0000"))
                            this.trackpadType = "Cypress";
                        if (str.Equals("ACPI\\VEN_ELAN&DEV_0000"))
                            this.trackpadType = "Elan";
                        if (str.Equals("ACPI\\VEN_ATML&DEV_0000"))
                            this.trackpadType = "Atmel";
                        if (str.Equals("ACPI\\VEN_SYNA&DEV_0000"))
                            this.trackpadType = "Synaptics";
                    }
                }
            }
            if (this.isModelSupported())
            {
                this.modelSupportedLabel.Content = (object)"Firmware Update Supported";
                this.supportedImg.Source = (ImageSource)new BitmapImage(new Uri("checkmark.png", UriKind.Relative));
                this.updateBtn.IsEnabled = true;
            }
            else
            {
                this.modelSupportedLabel.Content = (object)"Firmware Update Unsupported";
                this.supportedImg.Source = (ImageSource)new BitmapImage(new Uri("unsupported.png", UriKind.Relative));
            }
            this.modelLabel.Content = (object)("Model:  " + this.humanReadableModel);
            if (this.deviceType == 1)
                this.modelTypeImg.Source = (ImageSource)new BitmapImage(new Uri("laptop.png", UriKind.Relative));
            string str1 = "Chipset: " + this.chipset;
            if (this.trackpadType != "")
                str1 = str1 + "; Trackpad Type: " + this.trackpadType;
            this.modelDetails.Content = (object)str1;
            this.checkHashes(true);
        }

        private void updateBtn_Click(object sender, RoutedEventArgs e)
        {
            this.updateBtn.IsEnabled = false;
            if (!this.checkHashes(true))
                return;
            this.progressText.Content = (object)"Backing Up Current Firmware...";
            this.progressBar.Value = 10.0;
            if ((this.chipset == "Intel Haswell" || this.chipset == "Intel Broadwell") && !this.backupHSW())
                this.progressText.Content = (object)"Error Backing Up Existing Firmware!";
            else if (this.chipset == "Intel Bay Trail" && !this.backupBYT())
                this.progressText.Content = (object)"Error Backing Up Existing Firmware!";
            else
                this.downloadFWManifest();
        }

        private bool backupHSW()
        {
            if (this.executableDir == "")
                this.executableDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Process process = new Process();
            process.StartInfo.FileName = this.executableDir + "/FPT-HSWBDW/fptw64.exe";
            process.StartInfo.WorkingDirectory = this.executableDir + "/FPT-HSWBDW";
            process.StartInfo.Arguments = "-BIOS -D ../fw-backup.bin";
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            process.StartInfo.CreateNoWindow = false;
            process.Start();
            process.WaitForExit();
            return process.ExitCode == 0;
        }

        private bool backupBYT()
        {
            if (this.executableDir == "")
                this.executableDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Process process = new Process();
            process.StartInfo.FileName = this.executableDir + "/FPT-BYTBSW/fptw64.exe";
            process.StartInfo.WorkingDirectory = this.executableDir + "/FPT-BYTBSW";
            process.StartInfo.Arguments = "-BIOS -D ../fw-backup.bin";
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            process.StartInfo.CreateNoWindow = false;
            process.Start();
            process.WaitForExit();
            return process.ExitCode == 0;
        }

        private void downloadFWManifest()
        {
            this.progressText.Content = (object)"Downloading Manifest...";
            this.progressBar.Value = 30.0;
            bool gotFWURL = false;
            string fwURL = "";
            string fwSHA = "";
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += (DoWorkEventHandler)((sender2, e2) =>
            {
                try
                {
                    WebClient webClient = new WebClient();
                    webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.10136");
                    string str = webClient.DownloadString("https://coolstar.org/chromebook/cbmodels.json");
                    JObject json = (JObject)JsonConvert.DeserializeObject(str);
                    JObject modelData = null;
                    if (json.ContainsKey(this.model.ToLower()))
                    {
                        modelData = (JObject)json[this.model.ToLower()];
                    } else if (json.ContainsKey(this.model.ToLower() + "-" + this.trackpadType.ToLower()))
                    {
                        modelData = (JObject)json[this.model.ToLower() + "-" + this.trackpadType.ToLower()];
                    }
                    if (modelData != null)
                    {
                        if (modelData.ContainsKey("url") && modelData.ContainsKey("sha1"))
                        {
                            fwURL = (String)modelData["url"];
                            fwSHA = (String)modelData["sha1"];
                            gotFWURL = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    gotFWURL = false;
                }
            });
            backgroundWorker.RunWorkerCompleted += (RunWorkerCompletedEventHandler)((sender2, e2) =>
            {
                if (gotFWURL)
                    this.downloadFW(fwURL, fwSHA);
                else
                    this.progressText.Content = (object)"Error Parsing Manifest...";
            });
            backgroundWorker.RunWorkerAsync();
        }

        private void downloadFW(string fwURL, string fwSHA)
        {
            if (this.executableDir == "")
                this.executableDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            this.progressText.Content = (object)"Downloading Firmware...";
            this.progressBar.Value = 40.0;
            WebClient webClient = new WebClient();
            webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.10136");
            webClient.DownloadProgressChanged += (DownloadProgressChangedEventHandler)((sender, e) => this.progressBar.Value = 40.0 + (double)e.ProgressPercentage * 0.2);
            webClient.DownloadFileCompleted += (AsyncCompletedEventHandler)((sender, e) =>
            {
                this.progressBar.Value = 60.0;
                if (this.sha1OfFile(this.executableDir + "/fwupdate.bin") != fwSHA)
                    this.progressText.Content = (object)"Error verifying Firmware...";
                else if (this.macAddressInjectionRequired)
                {
                    if (this.injectMacAddress())
                        this.flashFW();
                    else
                        this.progressText.Content = (object)"Error Injecting MAC Address...";
                }
                else
                    this.flashFW();
            });
            Console.WriteLine(fwURL);
            webClient.DownloadFileAsync(new Uri(fwURL, UriKind.Absolute), this.executableDir + "/fwupdate.bin");
        }

        private bool injectMacAddress()
        {
            if (this.executableDir == "")
                this.executableDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (!this.rebuildOldFirmware())
                return false;
            if (!this.extractVPD())
                return false;
            return this.injectVPD();
        }

        private bool rebuildOldFirmware()
        {
            if (this.executableDir == "")
                this.executableDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            System.IO.File.Copy(this.executableDir + "/fwupdate.bin", this.executableDir + "/fwupdate-raw.bin");
            System.IO.File.Copy(this.executableDir + "/fwupdate.bin", this.executableDir + "/fw-reconstructed.bin");

            Process process = new Process();
            process.StartInfo.FileName = this.executableDir + "/cbtools/ifdtool.exe";
            process.StartInfo.WorkingDirectory = this.executableDir + "/cbtools";
            process.StartInfo.Arguments = "-i BIOS:../fw-backup.bin ../fw-reconstructed.bin";
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = false;
            process.Start();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                System.IO.File.Delete(this.executableDir + "/fw-reconstructed.bin");
                System.IO.File.Move(this.executableDir + "/fw-reconstructed.bin.new", this.executableDir + "/fw-reconstructed.bin");
                return true;
            }
            return false;
        }

        private bool extractVPD()
        {
            if (this.executableDir == "")
                this.executableDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Process process = new Process();
            process.StartInfo.FileName = this.executableDir + "/cbtools/cbfstool.exe";
            process.StartInfo.WorkingDirectory = this.executableDir + "/cbtools";
            process.StartInfo.Arguments = "../fw-reconstructed.bin extract -n vpd.bin -f ../vpd.bin";
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = false;
            process.Start();
            process.WaitForExit();
            return process.ExitCode == 0;
        }

        private bool injectVPD()
        {
            if (this.executableDir == "")
                this.executableDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Process process = new Process();
            process.StartInfo.FileName = this.executableDir + "/cbtools/cbfstool.exe";
            process.StartInfo.WorkingDirectory = this.executableDir + "/cbtools";
            process.StartInfo.Arguments = "../fwupdate.bin add -n vpd.bin -f ../vpd.bin -t raw";
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = false;
            process.Start();
            process.WaitForExit();
            return process.ExitCode == 0;
        }

        private void flashFW()
        {
            if (!this.checkHashes(false))
                return;
            this.progressText.Content = (object)"Flashing Firmware... DO NOT TURN OFF YOUR COMPUTER";
            this.progressBar.Value = 80.0;
            if ((this.chipset == "Intel Haswell" || this.chipset == "Intel Broadwell") && !this.flashHSW())
            {
                this.progressText.Content = (object)"Error Flashing Firmware! DO NOT REBOOT!";
                int num = (int)MessageBox.Show("Error Flashing Firmware! DO NOT REBOOT. Please use FPT to flash fwbackup.bin manually.");
            }
            else if (this.chipset == "Intel Bay Trail" && !this.flashBYT())
            {
                this.progressText.Content = (object)"Error Flashing Firmware! DO NOT REBOOT!";
                int num = (int)MessageBox.Show("Error Flashing Firmware! DO NOT REBOOT. Please use FPT to flash fwbackup.bin manually.");
            }
            else
            {
                this.progressText.Content = (object)"Flashing Succeeded! Please reboot!";
                this.progressBar.Value = 100.0;
            }
        }

        private bool flashHSW()
        {
            if (this.executableDir == "")
                this.executableDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Process process = new Process();
            process.StartInfo.FileName = this.executableDir + "/FPT-HSWBDW/fptw64.exe";
            process.StartInfo.WorkingDirectory = this.executableDir + "/FPT-HSWBDW";
            process.StartInfo.Arguments = "-BIOS -F ../fwupdate.bin";
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            process.StartInfo.CreateNoWindow = false;
            process.Start();
            process.WaitForExit();
            return process.ExitCode == 0;
        }

        private bool flashBYT()
        {
            if (this.executableDir == "")
                this.executableDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Process process = new Process();
            process.StartInfo.FileName = this.executableDir + "/FPT-BYTBSW/fptw64.exe";
            process.StartInfo.WorkingDirectory = this.executableDir + "/FPT-BYTBSW";
            process.StartInfo.Arguments = "-BIOS -F ../fwupdate.bin";
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            process.StartInfo.CreateNoWindow = false;
            process.Start();
            process.WaitForExit();
            return process.ExitCode == 0;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using System.Windows;

namespace InstalledAppsInfo
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DisplayInstalledAppsInfo();
        }

        private void DisplayInstalledAppsInfo()
        {
            var installedApps = new List<AppInfo>();

            // Get installed applications for Windows
            GetInstalledAppsInfoFromRegistry("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall", installedApps);

            // Get installed applications for Microsoft 365
            GetInstalledAppsInfoFromRegistry("SOFTWARE\\Microsoft\\Office\\ClickToRun\\REGISTRY\\MACHINE\\Software\\Microsoft\\Office\\", installedApps);

            // Get installed applications for Visual Studio
            GetInstalledAppsInfoFromRegistry("SOFTWARE\\Microsoft\\VisualStudio", installedApps);

            // Get installed applications for SQL Server
            GetInstalledAppsInfoFromRegistry("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Microsoft SQL Server", installedApps);

            // Get installed applications for .NET Framework
            GetDotNetFrameworkInfo(installedApps);

            // Display installed applications in the ListBox
            lstInstalledApps.ItemsSource = installedApps;
        }

        private void GetInstalledAppsInfoFromRegistry(string regKeyPath, List<AppInfo> installedApps)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(regKeyPath))
                {
                    if (key != null)
                    {
                        foreach (string subKeyName in key.GetSubKeyNames())
                        {
                            using (RegistryKey subKey = key.OpenSubKey(subKeyName))
                            {
                                var displayName = subKey.GetValue("DisplayName")?.ToString();
                                if (!string.IsNullOrEmpty(displayName))
                                {
                                    var version = subKey.GetValue("DisplayVersion")?.ToString();
                                    var servicePack = subKey.GetValue("SP")?.ToString();

                                    installedApps.Add(new AppInfo { Name = displayName, Version = version, ServicePack = servicePack });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while retrieving installed applications: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GetDotNetFrameworkInfo(List<AppInfo> installedApps)
        {
            try
            {
                // Iterate over the .NET Framework registry keys
                using (RegistryKey ndpKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP"))
                {
                    if (ndpKey != null)
                    {
                        foreach (string versionKeyName in ndpKey.GetSubKeyNames())
                        {
                            if (versionKeyName.StartsWith("v"))
                            {
                                RegistryKey versionKey = ndpKey.OpenSubKey(versionKeyName);
                                string name = (string)versionKey.GetValue("Version", "");
                                string servicePack = (string)versionKey.GetValue("SP", "");

                                installedApps.Add(new AppInfo { Name = $".NET Framework {name}", Version = name, ServicePack = servicePack });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while retrieving .NET Framework information: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class AppInfo
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string ServicePack { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Configuration;

namespace ByteSmith.WindowsAzure.Configuration
{
    public class HybridConfigurationManager
    {
        public static string GetAppSetting(string configurationSettingName)
        {
            return RoleEnvironment.IsAvailable 
                ? RoleEnvironment.GetConfigurationSettingValue(configurationSettingName) 
                : ConfigurationManager.AppSettings[configurationSettingName];
        }

        public static ConnectionStringSettings GetConnectionString(string name)
        {
            return RoleEnvironment.IsAvailable
                ? new ConnectionStringSettings(name,
                                               RoleEnvironment.GetConfigurationSettingValue(name),
                                               "System.Data.SqlClient")
                : ConfigurationManager.ConnectionStrings[name];
        }

        public static object GetSection(string sectionName)
        {
            return ConfigurationManager.GetSection(sectionName);
        }

        public static System.Configuration.Configuration OpenExeConfiguration(ConfigurationUserLevel userLevel)
        {
            return ConfigurationManager.OpenExeConfiguration(userLevel);
        }

        public static System.Configuration.Configuration OpenExeConfiguration(string exePath)
        {
            return ConfigurationManager.OpenExeConfiguration(exePath);
        }

        public static System.Configuration.Configuration OpenMachineConfiguration()
        {
            return ConfigurationManager.OpenMachineConfiguration();
        }

        public static System.Configuration.Configuration OpenMachineConfiguration(ExeConfigurationFileMap fileMap, ConfigurationUserLevel userLevel)
        {
            return ConfigurationManager.OpenMappedExeConfiguration(fileMap, userLevel);
        }

        public static System.Configuration.Configuration OpenMappedMachineConfiguration(ExeConfigurationFileMap fileMap)
        {
            return ConfigurationManager.OpenMappedMachineConfiguration(fileMap);
        }

        public static void RefreshSection(string sectionName)
        {
            ConfigurationManager.RefreshSection(sectionName);
        }
    }
}
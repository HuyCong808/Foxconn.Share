using Microsoft.Win32;
using System;
using System.Management;
using System.Net.NetworkInformation;
using System.Windows;

namespace Foxconn.Editor
{
    public class ComputerInfo
    {
        public static string GetComputerInfo()
        {
            return GetCPUInfo() + GetBaseBoardInfo() + GetBIOSInfo();
        }

        private static string GetCPUInfo()
        {
            return GetHardWareInfo("Win32_Processor", "ProcessorId");
        }

        private static string GetBIOSInfo()
        {
            return GetHardWareInfo("Win32_BIOS", "SerialNumber");
        }

        private static string GetBaseBoardInfo()
        {
            return GetHardWareInfo("Win32_BaseBoard", "SerialNumber");
        }

        private static string GetHardWareInfo(string typePath, string key)
        {
            try
            {
                ManagementClass managementClass = new ManagementClass(typePath);
                ManagementObjectCollection instances = managementClass.GetInstances();
                foreach (PropertyData property in managementClass.Properties)
                {
                    if (property.Name == key)
                    {
                        using (ManagementObjectCollection.ManagementObjectEnumerator enumerator = instances.GetEnumerator())
                        {
                            if (enumerator.MoveNext())
                                return enumerator.Current.Properties[property.Name].Value.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can not find hardware information：" + ex.ToString());
            }
            return string.Empty;
        }

        private static string GetMacAddressByNetworkInformation()
        {
            string str1 = "SYSTEM\\CurrentControlSet\\Control\\Network\\{4D36E972-E325-11CE-BFC1-08002BE10318}\\";
            string networkInformation = string.Empty;
            try
            {
                foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet && (uint)networkInterface.GetPhysicalAddress().ToString().Length > 0U)
                    {
                        string name = str1 + networkInterface.Id + "\\Connection";
                        RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(name, false);
                        if (registryKey != null)
                        {
                            string str2 = registryKey.GetValue("PnpInstanceID", "").ToString();
                            Convert.ToInt32(registryKey.GetValue("MediaSubType", 0));
                            if (str2.Length > 3 && str2.Substring(0, 3) == "PCI")
                            {
                                networkInformation = networkInterface.GetPhysicalAddress().ToString();
                                for (int index = 1; index < 6; ++index)
                                    networkInformation = networkInformation.Insert(3 * index - 1, ":");
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can not find MAC address：" + ex.ToString());
            }
            return networkInformation;
        }
    }
}

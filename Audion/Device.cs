using CSCore.CoreAudioAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Audion
{
    public class Device
    {
        public string DeviceId { get; set; }
        public string Name { get; set; }
        internal MMDevice ActualDevice { get; set; }

        public static Device GetDefaultDevice()
        {
            using (var mmdeviceEnumerator = new MMDeviceEnumerator())
            {
                using (var mmdeviceCollection = mmdeviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active))
                {
                    foreach (var device in mmdeviceCollection)
                    {
                        return new Device
                        {
                            DeviceId = device.DeviceID,
                            Name = device.FriendlyName,
                            ActualDevice = device
                        };
                    }
                }

                return null;
            }
        }

        public static IList<Device> GetDevices()
        {
            var devices = new List<Device>();

            using (var mmdeviceEnumerator = new MMDeviceEnumerator())
            {
                using (var mmdeviceCollection = mmdeviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active))
                {
                    foreach (var device in mmdeviceCollection)
                    {
                        devices.Add(new Device
                        {
                            DeviceId = device.DeviceID,
                            Name = device.FriendlyName,
                            ActualDevice = device
                        });
                    }
                }
            }

            return devices;
        }
    }
}

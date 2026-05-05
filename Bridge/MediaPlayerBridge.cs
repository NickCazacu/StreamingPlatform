using System;

namespace StreamingPlatform.Bridge
{
    /// <summary>
    /// ABSTRACTION — Player media de bază.
    /// Conține BRIDGE-ul: referința _device la IDeviceRenderer.
    ///
    /// Subclasele (refinedAbstraction) definesc logica specifică
    /// tipului de media și DELEGĂ randarea dispozitivului prin bridge.
    /// </summary>
    public abstract class MediaPlayerBase
    {
        // BRIDGE — legătura dintre abstracție și implementare
        protected IDeviceRenderer _device;

        protected MediaPlayerBase(IDeviceRenderer device)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
        }

        /// <summary>
        /// Schimbă dispozitivul de redare la RUNTIME fără a schimba tipul de media.
        /// Aceasta e flexibilitatea cheie a Bridge-ului.
        /// </summary>
        public void SwitchDevice(IDeviceRenderer newDevice)
        {
            Console.WriteLine($"      [Bridge] Comutare dispozitiv: " +
                              $"{_device.GetDeviceName()} → {newDevice.GetDeviceName()}");
            _device = newDevice;
        }

        public string GetCurrentDevice()      => _device.GetDeviceName();
        public string GetDeviceCapabilities() => _device.GetCapabilities();

        public abstract void Play(string title, string quality);
        public abstract string GetPlayerType();
    }
}

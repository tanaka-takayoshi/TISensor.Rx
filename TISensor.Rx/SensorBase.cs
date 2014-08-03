using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace TISensor.Rx
{
    public abstract class SensorBase : IDisposable
    {
        protected GattDeviceService deviceService;

        private readonly string sensorServiceUuid;
        private readonly string sensorConfigUuid;
        private readonly string sensorDataUuid;

        private GattCharacteristic dataCharacteristic;

        private bool disposed;

        public SensorStatus Status { get; protected set; }

        protected SensorBase(string sensorServiceUuid, string sensorConfigUuid, string sensorDataUuid)
        {
            Status = SensorStatus.NotInitialized;
            this.sensorServiceUuid = sensorServiceUuid;
            this.sensorConfigUuid = sensorConfigUuid;
            this.sensorDataUuid = sensorDataUuid;
        }

        public async Task<IEnumerable<DeviceInformation>> GetDevicesAsync()
        {
            var selector = GattDeviceService.GetDeviceSelectorFromUuid(new Guid(sensorServiceUuid));
            var devices = await DeviceInformation.FindAllAsync(selector);
            return devices.ToList();
        }

        private async Task<bool> Initialize(DeviceInformation deviceInfo)
        {

            if (deviceService != null)
            {
                Clean();
            }

            deviceService = await GattDeviceService.FromIdAsync(deviceInfo.Id);
            if (deviceService == null)
                return false;
            Status = SensorStatus.Initialized;
            return true;
        }

        protected virtual Task<bool> EnableSensor()
        {
            return EnableSensor(new byte[] {1});
        }

        protected async Task<bool> EnableSensor(byte[] sensorEnableData)
        {
            var configCharacteristic = deviceService.GetCharacteristics(new Guid(sensorConfigUuid)).First();

            var status = await configCharacteristic.WriteValueAsync(sensorEnableData.AsBuffer());
            //if (status == GattCommunicationStatus.Unreachable)
            //    throw new DeviceUnreachableException(DeviceUnreachableException.DEFAULT_UNREACHABLE_MESSAGE);
            Status = SensorStatus.Enabled;
            return true;
        }

        public async Task<bool> EnableNotifications(DeviceInformation deviceInfo)
        {
            await Initialize(deviceInfo);
            await EnableSensor();
            dataCharacteristic = deviceService.GetCharacteristics(new Guid(sensorDataUuid)).First();

            var status =
                    await dataCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                    GattClientCharacteristicConfigurationDescriptorValue.Notify);
            dataCharacteristic.ValueChanged += (sender, args) =>
            {
                ReadRawData(args);
            };
            if (status == GattCommunicationStatus.Unreachable)
            {
                throw new IOException("");
            }
            Status = SensorStatus.NotificationEnabled;
            return true;
        }

        public IObservable<SensorRawValue> StartMonitor()
        {
            return 
                Observable.FromEventPattern<
                    TypedEventHandler<GattCharacteristic, GattValueChangedEventArgs>,
                    GattValueChangedEventArgs>(
                        h => dataCharacteristic.ValueChanged += h,
                        h => dataCharacteristic.ValueChanged -= h)
                    .Select(p => ReadRawData(p.EventArgs));
        }

        private SensorRawValue ReadRawData(GattValueChangedEventArgs args)
        {
            var data = new byte[args.CharacteristicValue.Length];
            DataReader.FromBuffer(args.CharacteristicValue).ReadBytes(data);
            return new SensorRawValue
            {
                RawData = data,
                TimeStamp = args.Timestamp
            };
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Clean();
                }
            }

            disposed = true;
        }

        private void Clean()
        {
            if (deviceService != null)
                deviceService.Dispose();
            deviceService = null;
            //if (dataCharacteristic != null)
            //    dataCharacteristic.ValueChanged -= dataCharacteristic_ValueChanged;
        }
    }

    public enum SensorStatus
    {
        NotInitialized,
        Initialized,
        Enabled,
        NotificationEnabled,
        NotificationDisabled
    }

    public struct SensorRawValue
    {
        public byte[] RawData { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}

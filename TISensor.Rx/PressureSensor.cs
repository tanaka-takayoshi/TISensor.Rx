using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;

namespace TISensor.Rx
{
    public class PressureSensor : SensorBase
    {
        private readonly int[] calibrationData = { 0, 0, 0, 0, 0, 0, 0, 0 };

        public const string SensorServiceUuid = "f000aa40-0451-4000-b000-000000000000";
        public const string SensorDataUuid = "f000aa41-0451-4000-b000-000000000000";
        public const string SensorConfigUuid = "f000aa42-0451-4000-b000-000000000000";
        public const string SensorCaliburationUuid = "f000aa43-0451-4000-b000-000000000000";
        public const string SensorPeriodUuid = "f000aa44-0451-4000-b000-000000000000";

        public PressureSensor() :
            base(SensorServiceUuid, SensorConfigUuid, SensorDataUuid)
        { }

        public override async Task<bool> EnableSensor()
        {
            await StoreAndReadCalibrationValues();
            return await base.EnableSensor();
        }

        private async Task StoreAndReadCalibrationValues()
        {
            var tempConfig = deviceService.GetCharacteristics(new Guid(SensorConfigUuid)).First();

            var confdata = new byte[]{ 2 };
            var status = await tempConfig.WriteValueAsync(confdata.AsBuffer());
            if (status == GattCommunicationStatus.Unreachable)
                throw new Exception("");

            var calibrationCharacteristic = deviceService.GetCharacteristics(new Guid(SensorCaliburationUuid)).First();

            var res = await calibrationCharacteristic.ReadValueAsync(Windows.Devices.Bluetooth.BluetoothCacheMode.Uncached);

            if (res.Status == GattCommunicationStatus.Unreachable)
                throw new Exception("");

            var sdata = new byte[res.Value.Length];

            DataReader.FromBuffer(res.Value).ReadBytes(sdata);

            calibrationData[0] = BitConverter.ToUInt16(sdata, 0);
            calibrationData[1] = BitConverter.ToUInt16(sdata, 2);
            calibrationData[2] = BitConverter.ToUInt16(sdata, 4);
            calibrationData[3] = BitConverter.ToUInt16(sdata, 6);
            calibrationData[4] = BitConverter.ToInt16(sdata, 8);
            calibrationData[5] = BitConverter.ToInt16(sdata, 10);
            calibrationData[6] = BitConverter.ToInt16(sdata, 12);
            calibrationData[7] = BitConverter.ToInt16(sdata, 14);
        }

        public IObservable<PressureData> StartPressureMonitor()
        {
            return StartMonitor()
                .SelectMany(raw => Observable.Return(new PressureData
                {
                    PressureInHectoPascal = CalculatePressure(raw.RawData, calibrationData),
                    TimeStamp = raw.TimeStamp
                }));
        }

        private static double CalculatePressure(byte[] sensorData, int[] calibrationData)
        {
            int t_r, p_r;	// Temperature raw value, Pressure raw value from sensor
            double t_a, S, O; 	// Temperature actual value in unit centi degrees celsius, interim values in calculation

            t_r = BitConverter.ToInt16(sensorData, 0);
            p_r = BitConverter.ToUInt16(sensorData, 2);

            t_a = (100 * (calibrationData[0] * t_r / Math.Pow(2, 8) + calibrationData[1] * Math.Pow(2, 6))) / Math.Pow(2, 16);
            S = calibrationData[2] + calibrationData[3] * t_r / Math.Pow(2, 17) + ((calibrationData[4] * t_r / Math.Pow(2, 15)) * t_r) / Math.Pow(2, 19);
            O = calibrationData[5] * Math.Pow(2, 14) + calibrationData[6] * t_r / Math.Pow(2, 3) + ((calibrationData[7] * t_r / Math.Pow(2, 15)) * t_r) / Math.Pow(2, 4);
            return (S * p_r + O) / Math.Pow(2, 14);
        }
    }

    public struct PressureData
    {
        public double PressureInHectoPascal { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}

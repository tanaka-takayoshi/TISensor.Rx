using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace TISensor.Rx
{
    public class IRTemperatureSensor : SensorBase
    {
        private const string SensorServiceUuid = "f000aa00-0451-4000-b000-000000000000";
        private const string SensorDataUuid = "f000aa01-0451-4000-b000-000000000000";
        private const string SensorConfigUuid = "f000aa02-0451-4000-b000-000000000000";
        private const string SensorPeriodUuid = "f000aa03-0451-4000-b000-000000000000";

        public IRTemperatureSensor() :
            base(SensorServiceUuid, SensorConfigUuid, SensorDataUuid)
        {
            
        }

        public IObservable<IRTemperatureData> StartTemperatureMonitor()
        {
            return StartMonitor()
                .SelectMany(raw =>
                {
                    var ambient = CalculateAmbientTemperature(raw.RawData);
                    var target = CalculateTargetTemperature(raw.RawData, ambient);
                    return Observable.Return(new IRTemperatureData
                    {
                        AmbientTemperature =  ambient,
                        TargetTemperature = target,
                        TimeStamp = raw.TimeStamp
                    });
                });
        }

        private static double CalculateAmbientTemperature(byte[] sensorData)
        {
            return BitConverter.ToUInt16(sensorData, 2) / 128.0;
        }

        /// <summary>
        /// See: http://processors.wiki.ti.com/index.php/SensorTag_User_Guide#IR_Temperature_Sensor
        /// </summary>
        private static double CalculateTargetTemperature(byte[] sensorData, double ambientTemperature)
        {
            double vobj2 = BitConverter.ToInt16(sensorData, 0);
            vobj2 *= 0.00000015625;

            double Tdie = ambientTemperature + 273.15;

            double S0 = 5.593E-14;
            double a1 = 1.75E-3;
            double a2 = -1.678E-5;
            double b0 = -2.94E-5;
            double b1 = -5.7E-7;
            double b2 = 4.63E-9;
            double c2 = 13.4;
            double Tref = 298.15;
            double S = S0 * (1 + a1 * (Tdie - Tref) + a2 * Math.Pow((Tdie - Tref), 2));
            double Vos = b0 + b1 * (Tdie - Tref) + b2 * Math.Pow((Tdie - Tref), 2);
            double fObj = (vobj2 - Vos) + c2 * Math.Pow((vobj2 - Vos), 2);
            double tObj = Math.Pow(Math.Pow(Tdie, 4) + (fObj / S), .25);

            return tObj - 273.15;
        }
    }

    public struct IRTemperatureData
    {
        public double AmbientTemperature { get; set; }
        public double TargetTemperature { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}

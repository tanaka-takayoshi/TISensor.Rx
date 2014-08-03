using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace TISensor.Rx
{
    public class HumiditySensor : SensorBase
    {
        const string SensorServiceUuid = "f000aa20-0451-4000-b000-000000000000";
        const string SensorDataUuid = "f000aa21-0451-4000-b000-000000000000";
        const string SensorConfigUuid = "f000aa22-0451-4000-b000-000000000000";
        const string SensorPeriodUuid = "f000aa23-0451-4000-b000-000000000000";

        public HumiditySensor() :
            base(SensorServiceUuid, SensorConfigUuid, SensorDataUuid)
        { }

        public IObservable<HumidityData> StartHumidityMonitor()
        {
            return StartMonitor()
                .SelectMany(raw => Observable.Return(new HumidityData
                {
                    HumidityInPercent = CalculateHumidityInPercent(raw.RawData),
                    TimeStamp = raw.TimeStamp
                }));
        }

        private static double CalculateHumidityInPercent(byte[] sensorData)
        {
            var hum = (int)BitConverter.ToUInt16(sensorData, 2);
            // bits [1..0] are status bits and need to be cleared accordingto the userguide,
            // but the iOS code doesn't bother. It should have minimal impact.
            hum = hum - (hum % 4);
            return (-6f) + 125f * (hum / 65535f);
        }
    }

    public struct HumidityData
    {
        public double HumidityInPercent { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}

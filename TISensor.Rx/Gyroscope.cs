using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace TISensor.Rx
{
    public class Gyroscope : SensorBase
    {
        const string SensorServiceUuid = "f000aa50-0451-4000-b000-000000000000";
        const string SensorDataUuid = "f000aa51-0451-4000-b000-000000000000";
        const string SensorConfigUuid = "f000aa52-0451-4000-b000-000000000000";
        const string SensorPeriodUuid = "f000aa53-0451-4000-b000-000000000000";

        public Gyroscope() :
            base(SensorServiceUuid, SensorConfigUuid, SensorDataUuid)
        {}

        public override Task<bool> EnableSensor()
        {
            return EnableSensor(new byte[] { 7 });
        }

        public IObservable<GyroscopeData> StartGyroscopeMonitor()
        {
            return StartMonitor()
                .SelectMany(raw => Observable.Return(new GyroscopeData
                {
                    AngularRateX = BitConverter.ToInt16(raw.RawData, 0) * (500f / 65536f),
                    AngularRateY = BitConverter.ToInt16(raw.RawData, 2) * (500f / 65536f),
                    AngularRateZ = BitConverter.ToInt16(raw.RawData, 4) * (500f / 65536f),
                    TimeStamp = raw.TimeStamp
                }));
        }
    }

    public struct GyroscopeData
    {
        /// <summary>
        /// degree/sec X
        /// </summary>
        public float AngularRateX { get; set; }
        public float AngularRateY { get; set; }
        public float AngularRateZ { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}

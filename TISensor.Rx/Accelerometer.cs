using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace TISensor.Rx
{
    public class Accelerometer : SensorBase
    {
        public const string SensorServiceUuid = "f000aa10-0451-4000-b000-000000000000";
        public const string SensorDataUuid = "f000aa11-0451-4000-b000-000000000000";
        public const string SensorConfigUuid = "f000aa12-0451-4000-b000-000000000000";
        public const string SensorPeriodUuid = "f000aa13-0451-4000-b000-000000000000";

        public Accelerometer() :
            base(SensorServiceUuid, SensorConfigUuid, SensorDataUuid)
        { }

        public IObservable<AccelerometerData> StartAccelemeterMonitor()
        {
            return StartMonitor()
                .SelectMany(raw => Observable.Return(new AccelerometerData
                {
                    AccelerationX = raw.RawData[0] / 64d,
                    AccelerationY = raw.RawData[1] / 64d, 
                    AccelerationZ = raw.RawData[2] / 64d,
                    TimeStamp = raw.TimeStamp
                }));
        }
    }

    public struct AccelerometerData
    {
        /// <summary>
        /// g
        /// </summary>
        public double AccelerationX { get; set; }
        public double AccelerationY { get; set; }
        public double AccelerationZ { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}

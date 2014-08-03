using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace TISensor.Rx
{
    public class MagnetoMeter : SensorBase
    {
        const string SensorServiceUuid = "f000aa30-0451-4000-b000-000000000000";
        const string SensorDataUuid = "f000aa31-0451-4000-b000-000000000000";
        const string SensorConfigUuid = "f000aa32-0451-4000-b000-000000000000";
        const string SensorPeriodUuid = "f000aa33-0451-4000-b000-000000000000";

        public MagnetoMeter() :
            base(SensorServiceUuid, SensorConfigUuid, SensorDataUuid)
        { }

        public IObservable<MagenetoMeterData> StartMagnetoMeterMonitor()
        {
            return StartMonitor()
                .SelectMany(raw => Observable.Return(new MagenetoMeterData
                {
                    MagneticFieldX = BitConverter.ToInt16(raw.RawData, 0) * (2000f / 65536f),
                    MagneticFieldY = BitConverter.ToInt16(raw.RawData, 2) * (2000f / 65536f),
                    MagneticFieldZ = BitConverter.ToInt16(raw.RawData, 4) * (2000f / 65536f),
                    TimeStamp = raw.TimeStamp
                }));
        }
    }

    public struct MagenetoMeterData
    {
        /// <summary>
        /// micro teslta
        /// </summary>
        public double MagneticFieldX { get; set; }
        public double MagneticFieldY { get; set; }
        public double MagneticFieldZ { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}

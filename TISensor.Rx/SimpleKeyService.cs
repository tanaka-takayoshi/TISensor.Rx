using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace TISensor.Rx
{
    public class SimpleKeyService : SensorBase
    {
        const string SensorServiceUuid = "0000ffe0-0000-1000-8000-00805f9b34fb";
        const string SensorDataUuid = "0000ffe1-0000-1000-8000-00805f9b34fb";

        public SimpleKeyService() :
            base(SensorServiceUuid, null, SensorDataUuid)
        {}

        protected async override Task<bool> EnableSensor()
        {
            // not possible in this case
            return true;
        }

        //public async override Task<bool> DisableSensor()
        //{
        //}

        public IObservable<SimpleKeyData> StartSimpleKeyMonitor()
        {
            return StartMonitor()
                .SelectMany(raw => Observable.Return(new { raw.TimeStamp, bits = new BitArray(raw.RawData) }))
                .SelectMany(a => Observable.Return(new SimpleKeyData
                {
                    RightKeyTouch = a.bits[0],
                    LeftKeyTouch = a.bits[1],
                    SideKeyTouch = a.bits[2],
                    TimeStamps = a.TimeStamp
                }));
        }
    }

    public struct SimpleKeyData
    {
        public bool RightKeyTouch { get; set; }
        public bool LeftKeyTouch { get; set; }
        public bool SideKeyTouch { get; set; }
        public DateTimeOffset TimeStamps { get; set; }
    }
}

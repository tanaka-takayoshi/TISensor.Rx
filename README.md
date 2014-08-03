TISensor.Rx
===========

[![Build status](https://ci.appveyor.com/api/projects/status/4igsdnqu13nglk6c/branch/master)](https://ci.appveyor.com/project/tanaka-takayoshi/tisensor-rx/branch/master)

Texas Instruments Sensor Tag data to Observable.

## Download

## Usage
```csharp
var ir = new IRTemperatureSensor();
var device = d.First();
await ir.EnableNotifications(device);
ir.StartTemperatureMonitor()
    .ObserveOnDispatcher()
    .Subscribe(v =>
    {
        AmbientTemperature = v.AmbientTemperature;
        TargetTemperature = v.TargetTemperature;
    });
```
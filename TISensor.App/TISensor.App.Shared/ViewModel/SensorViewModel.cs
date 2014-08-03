using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using TISensor.Rx;

namespace TISensor.App.ViewModel
{
    public class SensorViewModel : ViewModelBase
    {

        private ObservableCollection<DeviceInformation> devices = new ObservableCollection<DeviceInformation>();
        public ObservableCollection<DeviceInformation> Devices
        {
            get
            {
                return devices;
            }

            set
            {
                if (devices == value)
                {
                    return;
                }
                devices = value;
                RaisePropertyChanged(() => Devices);
            }
        }

        private DeviceInformation selectedDevice = null;
        public DeviceInformation SelectedDevice
        {
            get
            {
                return selectedDevice;
            }

            set
            {
                if (selectedDevice == value)
                {
                    return;
                }
                selectedDevice = value;
                RaisePropertyChanged(() => SelectedDevice);
            }
        }

        private string irStatus = "Not monitoring";
        public string IrStatus
        {
            get
            {
                return irStatus;
            }

            set
            {
                if (irStatus == value)
                {
                    return;
                }
                irStatus = value;
                RaisePropertyChanged(() => IrStatus);
            }
        }

        private double ambientTemperature = 0;
        public double AmbientTemperature
        {
            get
            {
                return ambientTemperature;
            }

            set
            {
                if (ambientTemperature == value)
                {
                    return;
                }
                ambientTemperature = value;
                RaisePropertyChanged(() => AmbientTemperature);
            }
        }

        private double targetTemperature = 0d;
        public double TargetTemperature
        {
            get
            {
                return targetTemperature;
            }

            set
            {
                if (targetTemperature == value)
                {
                    return;
                }
                targetTemperature = value;
                RaisePropertyChanged(() => TargetTemperature);
            }
        }
        private RelayCommand scanDevicesCommand;

        /// <summary>
        /// Gets the ScanDevicesCommand.
        /// </summary>
        public RelayCommand ScanDevicesCommand
        {
            get
            {
                return scanDevicesCommand ?? (scanDevicesCommand = new RelayCommand(
                    ExecuteScanDevicesCommand));
            }
        }

        private async void ExecuteScanDevicesCommand()
        {
            var selector = GattDeviceService.GetDeviceSelectorFromUuid(new Guid("f000aa10-0451-4000-b000-000000000000"));
            var found = await DeviceInformation.FindAllAsync(selector);
            Devices = new ObservableCollection<DeviceInformation>(found);
        }

        private RelayCommand connectDeviceCommand;

        /// <summary>
        /// Gets the ConnectDeviceCommand.
        /// </summary>
        public RelayCommand ConnectDeviceCommand
        {
            get
            {
                return connectDeviceCommand ?? (connectDeviceCommand = new RelayCommand(
                    ExecuteConnectDeviceCommand));
            }
        }

        private void ExecuteConnectDeviceCommand()
        {

        }

        private RelayCommand startIRMonitorCommand;

        /// <summary>
        /// Gets the ConnectDeviceCommand.
        /// </summary>
        public RelayCommand StartIRMonitorCommand
        {
            get
            {
                return connectDeviceCommand ?? (connectDeviceCommand = new RelayCommand(
                    ExecuteStartIRMonitorCommand));
            }
        }

        private void ExecuteStartIRMonitorCommand()
        {
            StartIrMonitor().FireAndForget();
        }

        public async Task StartIrMonitor()
        {
            IrStatus = "Start monitoring...";
            var ir = new IRTemperatureSensor();
            var device = d.First();
            await ir.EnableNotifications(device);
            ir.StartTemperatureMonitor()
              .ObserveOnDispatcher()
              .Subscribe(v =>
              {
                  IrStatus = string.Format("{0:000} {1:000}", v.AmbientTemperature, v.TargetTemperature);
                  AmbientTemperature = v.AmbientTemperature;
                  TargetTemperature = v.TargetTemperature;
              });
            
        }

        private bool leftButton = false;
        public bool LeftButton
        {
            get
            {
                return leftButton;
            }

            set
            {
                if (leftButton == value)
                {
                    return;
                }
                leftButton = value;
                RaisePropertyChanged(() => LeftButton);
            }
        }

        private bool rightButton = false;
        public bool RightButton
        {
            get
            {
                return rightButton;
            }

            set
            {
                if (rightButton == value)
                {
                    return;
                }
                rightButton = value;
                RaisePropertyChanged(() => RightButton);
            }
        }

        private RelayCommand startTouchMonitorCommand;

        /// <summary>
        /// Gets the StartTouchMonitorCommand.
        /// </summary>
        public RelayCommand StartTouchMonitorCommand
        {
            get
            {
                return startTouchMonitorCommand ?? (startTouchMonitorCommand = new RelayCommand(
                    ExecuteStartTouchMonitorCommand));
            }
        }

        private async void ExecuteStartTouchMonitorCommand()
        {
            var key = new SimpleKeyService();
            var d = (await key.GetDevicesAsync()).First();
            await key.EnableNotifications(d);
            key.StartSimpleKeyMonitor()
                .ObserveOnDispatcher()
                .Subscribe(data =>
                {
                    LeftButton = data.LeftKeyTouch;
                    RightButton = data.RightKeyTouch;
                });
        }

    }
}

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mudules
{
    internal class PinsController
    {
        private GpioController? controller;
        public int[] ButtonPins = { 21, 20, 16, 26, 19, 13 };
        public event Action<int, bool> ButtonPressed;
        public PinsController()
        {
            controller = new GpioController();
            Task.Run(MonitorButtons);
        }
        private void MonitorButtons()
        {
            foreach (int item in ButtonPins)
            {
                controller.OpenPin(item, PinMode.InputPullUp);
            }


            while (true)
            {
                for (int i = 0; i < ButtonPins.Length; i++)
                {
                    if (controller.Read(ButtonPins[i]) == PinValue.Low)
                    {
                        ButtonPressed?.Invoke(i, true);
                    }
                    else if (controller.Read(ButtonPins[i]) == PinValue.High)
                    {
                        ButtonPressed?.Invoke(i, false);
                    }
                    Thread.Sleep(50);

                }
            }
        }
    }
}

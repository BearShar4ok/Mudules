using System.Device.Gpio;
using System.Reflection;
using System.Text;
using Mudules;
using Newtonsoft.Json;


using NAudio.Wave;


using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace Mudules
{
    public class DeviceData
    {
        public List<string> Devices { get; set; }
        public List<string> States { get; set; }
    }
    internal class Program
    {
        // Используем lock-объект для синхронизации доступа к консоли и состоянию
        private static readonly object consoleLock = new object();

        private static bool[] states = { false, false, false, false, false, false };
       
        private static int strLen;
        private static DeviceData data;
        private static int maxStatusLength;
        private static string exeDir;

        static void Main(string[] args)
        {
            Console.Clear();
            exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            string filePath = Path.Combine(exeDir, "Assets", "commands.json");

            string json = File.ReadAllText(filePath);

            

            data = JsonConvert.DeserializeObject<DeviceData>(json);

            strLen = data.Devices.Max(x => x.Length) + 4; 
            maxStatusLength = data.States.Max(x => x.Length);

            Console.CursorVisible = false;
           

            PinsController controller = new PinsController();
            controller.ButtonPressed += PressedPlus;

            TextBuilder();

            Thread.Sleep(Timeout.Infinite);
        }

        private static void PressedPlus(int n, bool state)
        {
            // Блокируем, чтобы избежать ситуации, когда два потока одновременно меняют состояние и рисуют
            lock (consoleLock)
            {
                bool changed = false;
                if (n != 5)
                {
                    if (states[n] != state)
                    {
                        states[n] = state;
                        if (state == false)
                        {
                            states[5] = false;
                        }
                        changed = true;
                    }
                }
                else if (n == 5 && state == true)
                {
                    bool allActive = states[0] && states[1] && states[2] && states[3] && states[4];
                    if (allActive)
                    {
                        states[5] = true;
                        changed = true;
                    }
                }

                if (changed)
                {
                    TextBuilder();
                }
            }
        }

        private static void TextBuilder()
        {
            var textBuilder = new StringBuilder();

            for (int j = 0; j < 5; j++)
            {
                string deviceName = data.Devices[j];
                string status = states[j] ? data.States[0] : data.States[1];

                textBuilder.Append(deviceName.PadRight(strLen));
                textBuilder.Append(status.PadRight(maxStatusLength)); // PadRight для выравнивания
                textBuilder.AppendLine();
            }

            textBuilder.AppendLine();

            string totalStatus = states[5] ? data.States[0] : data.States[1];

            textBuilder.Append(data.Devices[5].PadRight(strLen));
            textBuilder.Append(totalStatus.PadRight(maxStatusLength));
            textBuilder.AppendLine();

            lock (consoleLock)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.SetCursorPosition(0, 0);
                Console.Write(textBuilder.ToString());
                Console.ResetColor();
            }
        }
       
    }
}
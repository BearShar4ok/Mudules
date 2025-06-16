using System.Device.Gpio;
using System.Reflection;
using System.Text; // Добавили для StringBuilder
using Mudules;
using Newtonsoft.Json;

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
        //private static string[] devices = { "НАСОС", "БЛОК ПИТАНИЯ", "ОЧИСТИТЕЛЬНАЯ СТАНЦИЯ", "УСТРОЙСТВО 4", "УСТРОЙСТВО 5" };
        private static int strLen;
        private static DeviceData data;

        // Определяем самую длинную строку статуса для выравнивания
        private static int maxStatusLength;

        static void Main(string[] args)
        {
            Console.Clear();
            string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filePath = Path.Combine(exeDir, "commands.json");

            string json = File.ReadAllText(filePath);
            //DeviceData data = JsonConvert.DeserializeObject<DeviceData>(json);

            // string json = File.ReadAllText("commnads.json");

            data = JsonConvert.DeserializeObject<DeviceData>(json);

            // Считаем максимальную длину для красивого выравнивания
            strLen = data.Devices.Max(x => x.Length) + 4; // Добавим небольшой отступ
            maxStatusLength = data.States.Max(x => x.Length);

            // Прячем курсор, чтобы он не мешал
            Console.CursorVisible = false;

            PinsController controller = new PinsController();
            controller.ButtonPressed += PressedPlus;

            // Первоначальная отрисовка интерфейса
            TextBuilder();

            // Бесконечное ожидание, чтобы программа не завершилась
            Thread.Sleep(Timeout.Infinite);
        }

        private static void PressedPlus(int n, bool state)
        {
            // Блокируем, чтобы избежать ситуации, когда два потока одновременно меняют состояние и рисуют
            lock (consoleLock)
            {
                // Проверяем, изменилось ли состояние, чтобы не перерисовывать лишний раз
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
                    // Логика для кнопки 5: включаем "ОБЩЕЕ СОСТОЯНИЕ", только если все остальные активны
                    bool allActive = states[0] && states[1] && states[2] && states[3] && states[4];
                    if (allActive)
                    {
                        states[5] = true;
                        changed = true;
                    }
                }

                // Перерисовываем интерфейс только если что-то поменялось
                if (changed)
                {
                    TextBuilder();
                }
            }
        }

        private static void TextBuilder()
        {
            // StringBuilder намного эффективнее для конкатенации строк в цикле
            var textBuilder = new StringBuilder();

            for (int j = 0; j < 5; j++)
            {
                string deviceName = data.Devices[j];
                string status = states[j] ? data.States[0] : data.States[1];

                // Форматируем строку с выравниванием
                textBuilder.Append(deviceName.PadRight(strLen));
                textBuilder.Append(status.PadRight(maxStatusLength)); // PadRight для выравнивания
                textBuilder.AppendLine();
            }

            textBuilder.AppendLine(); // Пустая строка для разделения

            //string totalStatusName = "ОБЩЕЕ СОСТОЯНИЕ";
            string totalStatus = states[5] ? data.States[0] : data.States[1];

            textBuilder.Append(data.Devices[5].PadRight(strLen));
            textBuilder.Append(totalStatus.PadRight(maxStatusLength));
            textBuilder.AppendLine();

            // === Ключевые изменения здесь ===
            lock (consoleLock)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                // 1. Устанавливаем курсор в начало
                Console.SetCursorPosition(0, 0);
                // 2. Выводим весь текст за один раз
                Console.Write(textBuilder.ToString());
                // 3. Console.Clear() УБРАН!
                Console.ResetColor();
            }
        }
    }
}
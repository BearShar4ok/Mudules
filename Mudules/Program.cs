using System.Device.Gpio;
using Mudules;

namespace Mudules
{
    internal class Program
    {
        private static bool[] states = { false, false, false, false, false, false };
        private static string[] devices = { "НАСОС", "БЛОК ПИТАНИЯ", "ОЧИСТИТЕЛЬНАЯ СТАНЦИЯ", "УСТРОЙСТВО 4", "УСТРОЙСТВО 5" };
        private static int strLen;
        static void Main(string[] args)
        {
            strLen = devices.Max(x => x.Length);
            strLen += 10;
            PinsController controller = new PinsController();
            controller.ButtonPressed += PressedPlus;

            TextBuilder();

            Thread.Sleep(Timeout.Infinite);
        }
        private static void PressedPlus(int n, bool state)
        {
            if (n != 5)
            {
                states[n] = state;
                if (state == false)
                {
                    states[5] = false;
                }
            }
            else if (n == 5 && state == true)
            {
                if (states[0] && states[1] && states[2] && states[3] && states[4])
                    states[5] = true;
            }
            TextBuilder();
        }
        private static void TextBuilder()
        {
            string text = "";
            for (int j = 0; j < 5; j++)
            {
                text += devices[j];
                for (int i = 1; i < strLen - devices[j].Length; i++)
                {
                    text += " ";
                }
                if (states[j])
                    text += "АКТИВНО";
                else
                    text += "НЕ ПОДКЛЮЧЕНО";
                text += "\n";
            }
            text += "\n";
            string t = "ОБЩЕЕ СОСТОЯНИЕ";
            text += t;
            for (int i = 1; i < strLen - t.Length; i++)
            {
                text += " ";
            }
            if (states[5])
                text += "АКТИВНО";
            else
                text += "НЕ ПОДКЛЮЧЕНО";


            Console.ForegroundColor = ConsoleColor.Green;
            Console.Clear();
            Console.WriteLine(text);
            Console.ResetColor();

        }
    }
}

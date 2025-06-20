using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mudules
{
    internal static class AudioPlayerMy
    {
        public readonly static string[] audioFiles = { "device1.mp3", "device2.mp3", "device3.mp3", "device4.mp3", "device5.mp3", "mainButton.mp3" };
        public static void Init()
        {
            string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            for (int i = 0; i < audioFiles.Length; i++)
            {
                audioFiles[i] = Path.Combine(exeDir, "Assets", audioFiles[i]);
            }
        }
        public static void PlaySoundSynchronously(int audioFileNumber)
        {
            string audioFilePath = audioFiles[audioFileNumber];
            if (!File.Exists(audioFilePath))
            {
                // В режиме службы вывод в консоль не будет виден, но полезен для отладки
                Console.WriteLine($"Аудиофайл не найден: {audioFilePath}");
                return;
            }

            string arguments = $"-q \"{audioFilePath}\"";

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "mpg123",
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            try
            {
                using (var process = Process.Start(processStartInfo))
                {
                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при запуске mpg123: {ex.Message}");
            }
        }
    }
}

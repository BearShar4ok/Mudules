Финальная пошаговая инструкция: Автозапуск приложения в TTY на Raspberry Pi OS
Цель: Настроить Raspberry Pi так, чтобы при включении она автоматически загружалась в полноэкранное консольное приложение с полной поддержкой русского языка, без необходимости каких-либо действий со стороны пользователя.

Эта инструкция учитывает особенности последних версий Raspberry Pi OS, включая проблемы с инициализацией кириллицы в консоли tty1.

Часть 1: Базовая настройка системы и языка
Шаг 1.1: Настройка отображения кириллицы в консоли
Мы сделаем это "ручным" методом, так как он более надежен, чем стандартный raspi-config для консольного режима.

Запустите утилиту настройки консоли:
sudo dpkg-reconfigure console-setup
content_copy
download
Use code with caution.
Bash
В появившемся текстовом меню последовательно выберите:
Кодировка: UTF-8
Набор символов: . комбинированный - латинский; славянская кириллица; ...
Шрифт: Terminus (рекомендуется) или Fixed.
Размер шрифта: Выберите подходящий вам размер, например, 8x16 для стандартного или 16x32 для очень крупного.
Шаг 1.2: Настройка ввода кириллицы (раскладка клавиатуры)
Запустите утилиту настройки клавиатуры:
sudo dpkg-reconfigure keyboard-configuration
content_copy
download
Use code with caution.
Bash
В меню выберите:
Модель клавиатуры: Generic 105-key PC (или ваша).
Раскладка: Russian.
Способ переключения раскладок: Alt+Shift (или удобный вам).
Остальные параметры оставьте по умолчанию.
Часть 2: Настройка автоматического запуска приложения
Мы будем использовать связку "автоматический вход пользователя + скрипт в профиле". Это самый стабильный метод.

Шаг 2.1: Настройка автологина пользователя на TTY2
Мы будем использовать tty2, так как tty1 часто бывает занят системными сообщениями и имеет проблемы с инициализацией.

Создайте директорию для переопределения настроек системной службы:
sudo mkdir -p /etc/systemd/system/getty@tty2.service.d/
content_copy
download
Use code with caution.
Bash
Создайте и откройте файл конфигурации в этой директории:
sudo nano /etc/systemd/system/getty@tty2.service.d/override.conf
content_copy
download
Use code with caution.
Bash
Вставьте в этот файл следующий текст, заменив bearshark на имя вашего пользователя:
[Service]
ExecStart=
ExecStart=-/sbin/agetty --autologin bearshark --noclear %I $TERM
content_copy
download
Use code with caution.
Ini
Сохраните файл (Ctrl+O, Enter) и выйдите (Ctrl+X).
Шаг 2.2: Создание скрипта автозапуска приложения
Этот скрипт будет выполняться сразу после того, как ваш пользователь автоматически войдет в систему на tty2.

Откройте файл .bash_profile в вашей домашней директории (без sudo):
nano ~/.bash_profile
content_copy
download
Use code with caution.
Bash
Вставьте в него следующий код, заменив пути и имя исполняемого файла на свои:
# Этот скрипт будет выполняться только при входе в терминал TTY2
if [ "$(tty)" = "/dev/tty2" ]; then
  
  # ВАЖНЫЙ ШАГ: Принудительно применяем настройки шрифтов и кодировки.
  # Это решает проблему "гонки состояний", когда приложение запускается
  # раньше, чем система успевает настроить кириллицу.
  /usr/bin/setupcon

  # Переходим в рабочую директорию и запускаем приложение.
  # Замените путь и имя файла на свои.
  cd /home/bearshark/Downloads/modules/publish/ && /home/bearshark/Downloads/modules/publish/Mudules

fi
content_copy
download
Use code with caution.
Bash
Сохраните и выйдите.
Дайте вашему приложению права на исполнение (замените путь на свой):
chmod +x /home/bearshark/Downloads/modules/publish/Mudules
content_copy
download
Use code with caution.
Bash
Часть 3: Настройка загрузки и отображения
Теперь мы настроим систему так, чтобы она загружалась в консоль и автоматически показывала нам tty2.

Шаг 3.1: Переключение на консольный режим загрузки
Запустите утилиту raspi-config:
sudo raspi-config
content_copy
download
Use code with caution.
Bash
В меню выберите 1 System Options -> S5 Boot / Auto Login -> B1 Console.
Нажмите <Finish> и на вопрос о перезагрузке пока ответьте <No>.
Шаг 3.2: Создание службы для автоматического переключения на TTY2
Этот метод надежнее, чем параметр ядра vt.default_TTY, который у нас не сработал.

Создайте файл новой системной службы:
sudo nano /etc/systemd/system/switch-to-tty2.service
content_copy
download
Use code with caution.
Bash
Вставьте в этот файл следующий текст:
[Unit]
Description=Switch to TTY2 at the end of boot
After=getty.target

[Service]
Type=oneshot
ExecStart=/usr/bin/chvt 2
StandardInput=tty
StandardOutput=tty
TTYPath=/dev/tty1

[Install]
WantedBy=multi-user.target
content_copy
download
Use code with caution.
Ini
Сохраните и выйдите.
Шаг 3.3: Включение новой службы
Включите службу, чтобы она стартовала при каждой загрузке:
sudo systemctl enable switch-to-tty2.service
content_copy
download
Use code with caution.
Bash
Часть 4: Финальная перезагрузка
Теперь, когда все настроено, перезагрузите Raspberry Pi:

sudo reboot
content_copy
download
Use code with caution.
Bash
Ожидаемый результат: После перезагрузки система на несколько секунд покажет текстовые сообщения на tty1, а затем автоматически переключится на tty2, где вы сразу увидите ваше работающее приложение с корректной поддержкой русского языка и выбранным вами размером шрифта. Задача выполнена

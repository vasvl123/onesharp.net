﻿// /*----------------------------------------------------------
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v.2.0. If a copy of the MPL
// was not distributed with this file, You can obtain one
// at http://mozilla.org/MPL/2.0/.
// ----------------------------------------------------------*/

using System;
using ScriptEngine;
using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Library;

namespace starter
{

    public class ApplicationHost : IHostApplication
    {
        public string[] CommandLineArguments { get; set; } = new string[0];

        public void Echo(string text, MessageStatusEnum status = MessageStatusEnum.Ordinary)
        {
            ConsoleHostImpl.Echo(text, status);
        }

        public void ShowExceptionInfo(Exception exc)
        {
            ConsoleHostImpl.ShowExceptionInfo(exc);
        }

        public bool InputString(out string result, int maxLen)
        {
            return ConsoleHostImpl.InputString(out result, maxLen);
        }

        public string[] GetCommandLineArguments()
        {
            return CommandLineArguments;
        }
    }

    class starter : functions
    {
        string Хост;
        Соответствие Контроллеры;
        bool Локальный;
        string mono;
        int showdata;
        Массив Соединения;
        Массив мЗадачи;

        public starter(string _ИмяМодуля) :base (_ИмяМодуля)
        {
        }

        public bool ЗапуститьПроцесс(string Имя, int Порт, string Параметры = "")
        {
            try
            {
                Сообщить("Запуск " + Имя + " ...");
                // Проверка свободного порта
                TCPСервер Сервер = Новый_TCPСервер(Порт);
                Сервер.Запустить();
                Сервер.Остановить();
                ЗапуститьПриложение(mono + "useyourmind.exe " + Имя + " " + Порт + " " + Параметры, ТекущийКаталог());
                Приостановить(200); // ???
            }
            catch (Exception e)
            {
                Сообщить(ОписаниеОшибки(e));
                return Ложь;
            }
            return Истина;
        } // ЗапуститьПроцесс()

        public bool ЗапуститьПроцесс2(string Имя, int Порт, string Параметры = "")
        {
            try
            {
                Сообщить("Запуск " + Имя + " ...");
                // Проверка свободного порта
                TCPСервер Сервер = Новый_TCPСервер(Порт);
                Сервер.Запустить();
                Сервер.Остановить();
                ЗапуститьПриложение(mono + Имя + " " + Порт + " " + Параметры, ТекущийКаталог());
                Приостановить(200); // ???
            }
            catch (Exception e)
            {
                Сообщить(ОписаниеОшибки(e));
                return Ложь;
            }
            return Истина;
        } // ЗапуститьПроцесс()

        public void Main()
        {

            Хост = "127.0.0.1";

            int Порт = 8890; // Порт стартера
            int ПортЗ = 8888; // Порт запросов веб-сервера
            int ПортВ = 8889; // Порт ответов веб-сервера
            int ПортД = 8887; // Порт дата-сервера
            int ПортМ = 8886; // Порт морфологии

            var НовыйПортК = ПортМ;

            string ПараметрХост = " "; // путь для загрузки ресурсов

            Контроллеры = Новый_Соответствие();

            Соединения = Новый_Массив();
            
            Локальный = Истина;
            
            мЗадачи = Новый_Массив();
            
            mono = "";
            var си = Новый_СистемнаяИнформация();
            if (Лев(си.ВерсияОС, 4) == "Unix")
            {
                mono = "mono ";
            }

            if (АргументыКоманднойСтроки.Length != 0)
            {
                Локальный = (АргументыКоманднойСтроки[0] != "site");
                if (!Локальный)
                {
                    ПортЗ = (int)Число(АргументыКоманднойСтроки[1]);
                    Сообщить("Режим сайта, порт " + ПортЗ); // перезапуск и завершение заблокированы
                    ПараметрХост = ""; // путь для загрузки ресурсов
                }
            }

            var Таймаут = 5;
            
            var TCPСервер = Новый_TCPСервер(Порт);
            TCPСервер.ЗапуститьАсинхронно();
            Сообщить("Стартер запущен на порту: " + Порт);
            
            мЗадачи.Добавить(Новый_Структура("Запущен, ДанныеВходящие", Ложь, Новый_Структура("cmd", "startweb")));
            мЗадачи.Добавить(Новый_Структура("Запущен, ДанныеВходящие", Ложь, Новый_Структура("cmd", "showdata")));
            мЗадачи.Добавить(Новый_Структура("Запущен, ДанныеВходящие", Ложь, Новый_Структура("cmd", "startdata")));
            мЗадачи.Добавить(Новый_Структура("Запущен, ДанныеВходящие", Ложь, Новый_Структура("cmd", "startmorph")));
            
            bool ЗавершитьПроцесс = Ложь;
            bool ПерезапуститьПроцесс = Ложь;
            int ПустойЦикл = 0;

            while (!ЗавершитьПроцесс)
            {
                var НачалоЦикла = ТекущаяУниверсальнаяДатаВМиллисекундах();

                var к = мЗадачи.Количество();
                while (к > 0 && !(ТекущаяУниверсальнаяДатаВМиллисекундах() - НачалоЦикла > 50))
                {
                    к = к - 1;
                    dynamic структЗадача = мЗадачи.Получить(0) as Структура;
                    мЗадачи.Удалить(0);

                    dynamic ДанныеВходящие = структЗадача.Получить("ДанныеВходящие") as Структура;
                    var cmd = ДанныеВходящие.cmd;

                    if (cmd == "startproc")
                    {
                        if (showdata != 0)
                        {
                            if (Контроллеры.Получить(ДанныеВходящие.procid) == Неопределено)
                            {
                                var ПортК = showdata;
                                showdata = 0;

                                var стрКонтроллер = Новый_Структура("procid, Хост, Порт, ПортС, ПортВ, ПортД, ПортМ, УдаленныйУзел, Локальный, ПараметрХост, cmd",
                                                                    ДанныеВходящие.procid, Хост, ПортК, Порт, ПортВ, ПортД, ПортМ, "", Локальный, ПараметрХост, "init");
                                while (ПередатьДанные(Строка(Хост), ПортК, стрКонтроллер) == Неопределено)
                                {
                                    Сообщить("Ошибка передачи параметров процессу.");
                                    Приостановить(50);
                                }

                                Контроллеры.Вставить(ДанныеВходящие.procid, стрКонтроллер);
                                break;
                            }
                        }
                    }

                    else if (cmd == "termproc")
                    {
                        if (ДанныеВходящие.Свойство("procid"))
                        { 
                            // процесс завершен
                            var элКонтроллер = Контроллеры.Получить(ДанныеВходящие.procid);
                            if (элКонтроллер != Неопределено)
                            {
                                Контроллеры.Удалить(ДанныеВходящие.procid);
                            }
                            continue;
                        }
                    }

                    else if (cmd == "startmorph")
                    {
                        // Запуск сервера морфологии
                        if (структЗадача.Запущен == Ложь)
                        {
                            структЗадача.Запущен = ЗапуститьПроцесс("morphserver.os", ПортМ);
                        }
                        else
                        {
                            continue;
                        }
                    }

                    else if (cmd == "startdata")
                    {
                        // Запуск дата-сервера
                        if (структЗадача.Запущен == Ложь)
                        {
                            структЗадача.Запущен = Истина; //ЗапуститьПроцесс2("dataserver.exe", ПортД);
                        }
                        else
                        {
                            continue;
                        }
                    }

                    else if (cmd == "startweb")
                    {
                        // Запуск веб-сервера
                        if (структЗадача.Запущен == Ложь)
                        {
                            структЗадача.Запущен = ЗапуститьПроцесс2("webserver.exe", ПортВ, Строка(ПортЗ));
                        }
                        else
                        {
                            // Передать параметры веб-сервера
                            if (ПередатьДанные(Хост, ПортВ, Новый_Структура("Хост, Порт, ПортС, ПортД, cmd", Хост, ПортВ, Порт, ПортД, "init")) != Неопределено)
                            {
                                Приостановить(50);
                                continue;
                            }
                        }
                    }

                    else if (cmd == "showdata")
                    {
                        if (showdata == 0)
                        {
                            // запустить процесс заранее
                            if (НовыйПортК < 5555)
                            {
                                НовыйПортК = ПортМ;
                            }
                            НовыйПортК = НовыйПортК - 1;
                            while (!ЗапуститьПроцесс("showdata.os", НовыйПортК))
                            {
                                НовыйПортК = НовыйПортК - 1;
                            }

                            showdata = НовыйПортК;
                        }
                    }

                    else if (cmd == "stopserver" || cmd == "restartserver")
                    {
                        if (Локальный)
                        {
                            // Завершить процессы
                            foreach (dynamic элКонтроллер in Контроллеры)
                            {
                                ПередатьДанные(элКонтроллер.Значение.Хост, элКонтроллер.Значение.Порт, Новый_Структура("cmd, taskid", "termproc", "0"));
                            }

                            // процесс запущен заранее
                            if (showdata != 0)
                            {
                                ПередатьДанные(Хост, showdata, Новый_Структура("cmd, taskid", "termproc", "0"));
                            }
                            
                            if (cmd == "restartserver")
                            {
                                ПерезапуститьПроцесс = Истина;
                            }
                            
                            ЗавершитьПроцесс = Истина;
                        }
                        else
                        {
                            // завершаем один процесс
                            if (ДанныеВходящие.Свойство("procid"))
                            {
                                var элКонтроллер = Контроллеры.Получить(ДанныеВходящие.procid);
                                if (элКонтроллер != Неопределено)
                                {
                                    ПередатьДанные(элКонтроллер.Хост, элКонтроллер.Порт, Новый_Структура("cmd, taskid", "termproc", "0"));
                                }
                            }
                        }
                        continue;
                    }

                    мЗадачи.Добавить(структЗадача);

                }

                var Соединение = TCPСервер.ПолучитьСоединение(Таймаут);
                if (Соединение != Неопределено)
                {
                    Соединения.Вставить(0, Соединение);
                    Таймаут = 5;
                }

                к = Соединения.Количество();
                while (к > 0)
                {
                    к = к - 1;
                    Соединение = (TCPСоединение)Соединения.Получить(0);
                    Соединения.Удалить(0);

                    if (Соединение.Статус == "Данные")
                    {
                        dynamic ДанныеВходящие = null;
                        try
                        {
                            ДанныеВходящие = ДвоичныеДанныеВСтруктуру(Соединение.ПолучитьДвоичныеДанные()) as Структура;
                        }
                        catch (Exception e)
                        {
                            Сообщить("starter: " + ОписаниеОшибки(e));
                        }

                        if (ДанныеВходящие == Неопределено)
                        {
                            continue;
                        }

                        if (ДанныеВходящие.Свойство("cmd"))
                        {
                            // новая задача
                            Сообщить("starter: " + ДанныеВходящие.cmd);
                            мЗадачи.Добавить(Новый_Структура("ДанныеВходящие", ДанныеВходящие));
                        }

                        Соединение.Закрыть();
                        continue;
                    }

                    else if (Соединение.Статус == "Ошибка")
                    {
                        Соединение.Закрыть();
                        continue;
                    }

                    else
                    {
                        ПустойЦикл = ПустойЦикл + 1;
                    }

                    Соединения.Добавить(Соединение);
                }

                var ВремяЦикла = ТекущаяУниверсальнаяДатаВМиллисекундах() - НачалоЦикла;
                if (ВремяЦикла > 100)
                {
                    Сообщить("!starter ВремяЦикла=" + ВремяЦикла);
                }
                if (Таймаут < 50)
                {
                    Таймаут = Таймаут + 1;
                }

            }

            if (Контроллеры.Количество() != 0)
            {
                Сообщить("Не все контроллеры завершили работу.");
                Приостановить(200);
            }
            TCPСервер.Остановить();

            // Завершить сервер морфологии
            var с1 = ПередатьДанные(Хост, ПортМ, Новый_Структура("cmd", "stopserver"));

            // Завершить веб-сервер
            var с2 = ПередатьДанные(Хост, ПортВ, Новый_Структура("cmd", "stopserver"));

            // Завершить дата-сервер
            var с3 = ПередатьДанные(Хост, ПортД, Новый_Структура("cmd", "stopserver"));

            while (с1.Статус == "Занят" || с2.Статус == "Занят" || с3.Статус == "Занят")
            {
                Приостановить(50);
            }

            if (ПерезапуститьПроцесс)
            {
                Сообщить("Перезапуск");
                ЗапуститьПроцесс("starter.os", Порт);
            }

            Сообщить("Процесс starter завершен.");

        }

    }

    class MainClass
    {

        public static void Main(string[] args)
        {
            var hostedScript = new HostedScriptEngine();
            var app = new starter("starter");
            app._syscon = new SystemGlobalContext();
            app._syscon.ApplicationHost = new ApplicationHost();
            app.Main();

        }
    }
}

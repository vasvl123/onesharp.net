﻿using System;
using ScriptEngine;
using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Library;

namespace webserver
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

    class webserver : functions
    {
        int Порт;
        bool ОстановитьСервер;
        Соответствие СтатусыHTTP;
        Соответствие СоответствиеРасширенийТипамMIME;
        Соответствие Задачи;
        Массив мЗадачи;
        //Перем Ресурсы;
        Соответствие Контроллеры;
        string Загрузка;
        dynamic Параметры;
        Массив Сообщения;
        Массив Соединения;
        Массив СоединенияО;

        public webserver(string _ИмяМодуля) : base(_ИмяМодуля)
        {
        }

        string УдаленныйУзелАдрес(string УдаленныйУзел)
        {
            return Лев(УдаленныйУзел, Найти(УдаленныйУзел, ":") - 1);
        } // УдаленныйУзелАдрес()


        Структура РазобратьЗапросКлиента(string ТекстЗапроса, TCPСоединение Соединение)
        {
            string ИмяКонтроллера = "";
            string ИмяМетода = "";
            //Перем ПараметрыМетода;
            var Заголовок = Новый_Соответствие();
            var мТекстовыеДанные = ТекстЗапроса;
            var Разделитель = "";
            string Метод = null;
            string Путь = null;

            var GETСтруктура = Новый_Структура();

            while (Истина)
            {
                var П = Найти(мТекстовыеДанные, Символы.ПС);
                if (П == 0)
                {
                    break;
                }
                var Подстрока = Лев(мТекстовыеДанные, П);
                мТекстовыеДанные = Прав(мТекстовыеДанные, СтрДлина(мТекстовыеДанные) - П);
                if (Найти(Подстрока, "HTTP/1") > 0)
                {
                    var П1 = 0;
                    if (Лев(Подстрока, 3) == "GET")
                    {
                        Метод = "GET";
                        П1 = 3;
                    }
                    else if (Лев(Подстрока, 4) == "POST")
                    {
                        Метод = "POST";
                        П1 = 4;
                    }
                    else if (Лев(Подстрока, 3) == "PUT")
                    {
                        Метод = "PUT";
                        П1 = 3;
                    }
                    else if (Лев(Подстрока, 6) == "DELETE")
                    {
                        Метод = "DELETE";
                        П1 = 6;
                    }
                    Заголовок.Вставить("Method", Метод);
                    var П2 = Найти(Подстрока, "HTTP/1");
                    Путь = СокрЛП(Сред(Подстрока, П1 + 1, СтрДлина(Подстрока) - 10 - П1));
                    Заголовок.Вставить("Path", Путь);
                }
                else
                {
                    if (Подстрока == Символы.ВК + Символы.ПС)
                    {
                        break;
                    }
                    else if (Найти(Подстрока, ":") > 0)
                    {
                        var П3 = Найти(Подстрока, ":");
                        var Ключ = СокрЛП(Лев(Подстрока, П3 - 1));
                        var Значение = СокрЛП(Прав(Подстрока, СтрДлина(Подстрока) - П3));
                        Заголовок.Вставить(Ключ, Значение);
                        if (Ключ == "Content-Type")
                        {
                            if (Лев(Значение, 20) == "multipart/form-data;")
                            {
                                Разделитель = "--" + Сред(Значение, 31);
                            }
                        }
                    }
                    else
                    {
                        var Ключ = "unknown";
                        var Значение = СокрЛП(Подстрока);
                        if (СтрДлина(Значение) > 0)
                        {
                            Заголовок.Вставить(Ключ, Значение);
                        }
                    }
                }
            }

            if (Метод == "POST")
            {
                Заголовок.Вставить("POSTData", Новый_Структура());
                Заголовок.Вставить("Разделитель", Разделитель);
            }


            Путь = СокрЛП(Заголовок.Получить("Path") as string);
            if (!(Путь == null))
            {
                if (Лев(Путь, 1) == "/")
                {
                    Путь = Прав(Путь, СтрДлина(Путь) - 1);
                }
                if (Прав(Путь, 1) != "/")
                {
                    Путь = Путь + "/";
                }
                var Сч = 0;
                while (Найти(Путь, "/") > 0)
                {
                    var П = Найти(Путь, "/");
                    Сч = Сч + 1;
                    var ЗначениеПараметра = РаскодироватьСтроку(Лев(Путь, П - 1), СпособКодированияСтроки.КодировкаURL);
                    Путь = Прав(Путь, СтрДлина(Путь) - П);
                    if (Сч == 1)
                    {
                        ИмяКонтроллера = ЗначениеПараметра;
                    }
                    else if (Сч == 2)
                    {
                        ИмяМетода = ЗначениеПараметра;
                    }
                    else if (!(ЗначениеПараметра == ".."))
                    {
                        ИмяМетода = ОбъединитьПути(ИмяМетода, ЗначениеПараметра);
                    }
                }
                if (!(СокрЛП(ИмяМетода) == ""))
                {
                    if (Найти(ИмяМетода, "?") != 0)
                    {
                        var GETДанные = ИмяМетода;
                        ИмяМетода = Лев(GETДанные, Найти(GETДанные, "?") - 1);
                        GETДанные = СтрЗаменить(GETДанные, ИмяМетода + "?", "") + "&";
                        while (Найти(GETДанные, "&") > 0)
                        {
                            var П1 = Найти(GETДанные, "&");
                            var П2 = Найти(GETДанные, "=");
                            var Ключ = Лев(GETДанные, П2 - 1);
                            var Значение = Сред(GETДанные, П2 + 1, П1 - (П2 + 1));
                            GETДанные = Прав(GETДанные, СтрДлина(GETДанные) - П1);
                            if (!(Ключ == ""))
                            {
                                GETСтруктура.Вставить(Ключ, РаскодироватьСтроку(Значение, СпособКодированияСтроки.КодировкаURL));
                            }
                        }
                    }
                }
            }
            Заголовок.Вставить("GETData", GETСтруктура);
            var Запрос = Новый_Структура();
            Запрос.Вставить("Заголовок", Заголовок);
            Запрос.Вставить("ИмяКонтроллера", "" + ИмяКонтроллера);
            Запрос.Вставить("ИмяМетода", "" + ИмяМетода);
            return Запрос;
        } // РазобратьЗапросКлиента()


        object ОбработатьЗапросКлиента(dynamic Запрос, TCPСоединение Соединение)
        {
            var Метод = Запрос.Заголовок.Получить("Method");
            if (!(Метод == Неопределено))
            {
                Структура ПараметрыЗапроса = Запрос.Заголовок.Получить(Метод + "Data");
                if (ПараметрыЗапроса == Неопределено)
                {
                    return Неопределено;
                }
                dynamic Задача = Новый_Структура();
                var ИдЗадачи = ПолучитьИД();
                Задачи.Вставить("" + ИдЗадачи, Задача);
                мЗадачи.Добавить(Задача);
                Задача.Вставить("ИдЗадачи", "" + ИдЗадачи);
                Задача.Вставить("структКонтроллер", Неопределено);
                Задача.Вставить("ВремяНачало", ТекущаяУниверсальнаяДатаВМиллисекундах());
                Задача.Вставить("Соединение", Соединение);
                Задача.Вставить("ИдКонтроллера", Неопределено);
                Задача.Вставить("Результат", Неопределено);
                Задача.Вставить("Этап", (Метод == "POST") ? "Данные" : "Новая");
                Задача.Вставить("УдаленныйУзел", УдаленныйУзелАдрес(Соединение.УдаленныйУзел));
                Задача.Вставить("Заголовок", Запрос.Заголовок);
                ПараметрыЗапроса.Вставить("УдаленныйУзел", Задача.УдаленныйУзел);
                ПараметрыЗапроса.Вставить("ИмяМетода", Запрос.ИмяМетода);
                ПараметрыЗапроса.Вставить("ИмяКонтроллера", Запрос.ИмяКонтроллера);
                Задача.Вставить("ПараметрыЗапроса", ПараметрыЗапроса);
                if (Запрос.ИмяКонтроллера == "resource")
                {
                    Задача.Вставить("ИмяДанных", ОбъединитьПути(Запрос.ИмяКонтроллера, Запрос.ИмяМетода));
                    Задача.Вставить("Результат", "Файл");
                    Задача.Этап = "Обработка";
                }
                else if (Запрос.ИмяКонтроллера == "favicon.ico" || Запрос.ИмяКонтроллера == "robots.txt")
                {
                    Задача.Вставить("ИмяДанных", ОбъединитьПути("resource", Запрос.ИмяКонтроллера));
                    Задача.Вставить("Результат", "Файл");
                    Задача.Этап = "Обработка";
                }
                ЛогСообщить(Задача.УдаленныйУзел + " -> taskid=" + Задача.ИдЗадачи + " " + СокрЛП(Запрос.Заголовок.Получить("Method")) + " " + Запрос.Заголовок.Получить("Path"));
            }

            return Неопределено;

        } // ОбработатьЗапросКлиента()


        void РазобратьДанныеЗапроса(dynamic Задача)
        {
            Структура POSTСтруктура = Задача.ПараметрыЗапроса;
            string Содержимое = Задача.Заголовок.Получить("Content-Type");
            if (Содержимое == "text/plain;charset=UTF-8")
            {
                var POSTДанные = Задача.Соединение.ПолучитьСтроку();
                if (СтрДлина(POSTДанные) > 0)
                {
                    POSTДанные = POSTДанные + "&";
                }
                while (Найти(POSTДанные, "&") > 0)
                {
                    var П1 = Найти(POSTДанные, "&");
                    var П2 = Найти(POSTДанные, "=");
                    var Ключ = Лев(POSTДанные, П2 - 1);
                    var Значение = Сред(POSTДанные, П2 + 1, П1 - (П2 + 1));
                    POSTДанные = Прав(POSTДанные, СтрДлина(POSTДанные) - П1);
                    if (!(Ключ == ""))
                    {
                        POSTСтруктура.Вставить(Ключ, РаскодироватьСтроку(Значение, СпособКодированияСтроки.КодировкаURL));
                    }
                }
            }
            else if (Лев(Содержимое, 20) == "multipart/form-data;")
            {
                string Разделитель = Задача.Заголовок.Получить("Разделитель");
                string мТекстовыеДанные = Задача.Соединение.ПолучитьСтроку("windows-1251");
                while (Истина)
                {
                    var П = Найти(мТекстовыеДанные, Разделитель);
                    if (П == 0)
                    {
                        break;
                    }
                    var Подстрока = Лев(мТекстовыеДанные, П);
                    мТекстовыеДанные = Прав(мТекстовыеДанные, СтрДлина(мТекстовыеДанные) - П - СтрДлина(Разделитель) - 1);
                    if (Найти(Подстрока, "Content-Disposition: form-data;") != 0)
                    {
                        var П1 = Найти(Подстрока, "name=");
                        var П2 = Найти(Подстрока, Символы.ПС);
                        var П3 = Найти(Подстрока, Символы.ВК + Символы.ПС + Символы.ВК + Символы.ПС);
                        var П4 = Найти(Подстрока, "; filename");
                        string Ключ;
                        object Значение;
                        if (!(П4 == 0))
                        {
                            Значение = ПолучитьДвоичныеДанныеИзСтроки(Сред(Подстрока, П3 + 4, СтрДлина(Подстрока) - П3 - 6), "windows-1251");
                            POSTСтруктура.Вставить("filename", РаскодироватьСтроку(Сред(Подстрока, П4 + 12, П2 - П4 - 14), СпособКодированияСтроки.КодировкаURL));
                            Ключ = Сред(Подстрока, П1 + 6, П4 - П1 - 7);
                        }
                        else
                        {
                            Ключ = Сред(Подстрока, П1 + 6, П2 - П1 - 8);
                            Значение = РаскодироватьСтроку(Сред(Подстрока, П2 + 3, СтрДлина(Подстрока) - П2 - 5), СпособКодированияСтроки.КодировкаURL);
                        }
                        if (!(Ключ == ""))
                        {
                            POSTСтруктура.Вставить(Ключ, Значение);
                        }
                    }
                }
            }
            else if (Содержимое == "application/octet-stream")
            {
                POSTСтруктура.Вставить("Данные", Задача.Соединение.ПолучитьДвоичныеДанные());
            }

        } // РазобратьДанныеЗапроса()


        void ОбработатьОтветСервера(dynamic Задача)
        {
            string ИмяФайла;
            try
            {
                var СтатусОтвета = 200;
                var Заголовок = Новый_Соответствие();
                ДвоичныеДанные ДвоичныеДанныеОтвета;
                if (ТипЗнч(Задача.Результат) == Тип("ДвоичныеДанные"))
                {
                    ДвоичныеДанныеОтвета = Задача.Результат;
                    Заголовок.Вставить("Content-Length", ДвоичныеДанныеОтвета.Размер());
                    var ContentType = "";
                    if (Задача.Свойство("ContentType", ContentType))
                    {
                        Заголовок.Вставить("Content-Type", ContentType);
                    }
                }
                else
                {
                    ДвоичныеДанныеОтвета = ПолучитьДвоичныеДанныеИзСтроки(Задача.Результат);
                    Заголовок.Вставить("Content-Length", ДвоичныеДанныеОтвета.Размер());
                    Заголовок.Вставить("Content-Type", "text/html");
                }
                if (Задача.Свойство("ИмяДанных"))
                {
                    ИмяФайла = ОбъединитьПути(ТекущийКаталог(), Задача.ИмяДанных);
                    var Файл = Новый_Файл(ИмяФайла);
                    var Расширение = Файл.Расширение;
                    if (!(Файл.Существует()))
                    {
                        ИмяФайла = СтрЗаменить(ИмяФайла, "/", @"\");
                        Файл = Новый_Файл(ИмяФайла);
                    }
                    if (!(Файл.Существует()))
                    {
                        СтатусОтвета = 404;
                    }
                    if (СтатусОтвета == 200)
                    {
                        var MIME = СоответствиеРасширенийТипамMIME.Получить(Расширение);
                        if (MIME == Неопределено)
                        {
                            MIME = СоответствиеРасширенийТипамMIME.Получить("default");
                        }
                        ДвоичныеДанныеОтвета = Новый_ДвоичныеДанные(СокрЛП(ИмяФайла));
                        Заголовок.Вставить("Content-Length", ДвоичныеДанныеОтвета.Размер());
                        Заголовок.Вставить("Content-Type", MIME);
                    }
                }
                try
                {
                    var ПС = Символы.ВК + Символы.ПС;
                    var ТекстОтветаКлиенту = СокрЛП(СтатусыHTTP.Получить(СтатусОтвета)) + ПС;
                    foreach (КлючИЗначение СтрокаЗаголовкаответа in Заголовок)
                    {
                        ТекстОтветаКлиенту = ТекстОтветаКлиенту + СтрокаЗаголовкаответа.Ключ + ":" + СтрокаЗаголовкаответа.Значение + ПС;
                    }
                    var мДанные = Новый_Массив();
                    мДанные.Добавить(ПолучитьДвоичныеДанныеИзСтроки(ТекстОтветаКлиенту + ПС));
                    мДанные.Добавить(ДвоичныеДанныеОтвета);
                    Задача.Соединение.ОтправитьДвоичныеДанныеАсинхронно(СоединитьДвоичныеДанные(мДанные));
                    Задача.Этап = "Вернуть";
                }
                catch (Exception e)
                {
                    Сообщить("webserver: " + ОписаниеОшибки(e));
                    Задача.Этап = "Удалить";
                }
            }
            catch (Exception e)
            {
                ЛогСообщить("Ошибка формирования ответа");
                ЛогСообщить(ОписаниеОшибки(e));
                Задача.Этап = "Удалить";
            }
        } // ОбработатьОтветСервера()


        void ОбработатьЗадачи()
        {
            var НачалоЦикла = ТекущаяУниверсальнаяДатаВМиллисекундах();
            var к = мЗадачи.Количество();
            while (к > 0 && !(ТекущаяУниверсальнаяДатаВМиллисекундах() - НачалоЦикла > 50))
            {
                к = к - 1;
                dynamic Задача = мЗадачи.Получить(0) as Структура;
                мЗадачи.Удалить(0);
                if (Задача.Этап == "Данные")
                {
                    if (Задача.Соединение.Статус == "Данные")
                    {
                        РазобратьДанныеЗапроса(Задача);
                        Задача.Этап = "Новая";
                    }
                    else if (Задача.Соединение.Статус == "Ошибка")
                    {
                        ВызватьИсключение("Ошибка получения данных");
                    }
                    
                }
                if (Задача.Этап == "Новая")
                {
                    var ПарИдКонтроллера = "";
                    var ИмяКонтроллера = Задача.ПараметрыЗапроса.ИмяКонтроллера;
                    if (ИмяКонтроллера == "procid")
                    {
                        ПарИдКонтроллера = Задача.ПараметрыЗапроса.ИмяМетода;
                        if (Контроллеры.Получить(ПарИдКонтроллера) == Неопределено)
                        {
                            ПарИдКонтроллера = "";
                        }
                    }
                    else
                    {
                        if (ИмяКонтроллера == "" || ИмяКонтроллера == "doc")
                        {
                            ПарИдКонтроллера = "1";
                        }
                        else
                        {
                            ПарИдКонтроллера = "" + ПолучитьИД();
                        }
                        if (Контроллеры.Получить(ПарИдКонтроллера) == Неопределено)
                        {
                            if (ПередатьДанные(Параметры.Хост, Параметры.ПортС, Новый_Структура("procid, cmd", ПарИдКонтроллера, "startproc")) == Неопределено)
                            {
                                Сообщить("webserver: ошибка создания процесса");
                                ПарИдКонтроллера = "";
                            }
                        }
                    }
                    if (!(ПарИдКонтроллера == ""))
                    {
                        Задача.ИдКонтроллера = ПарИдКонтроллера;
                        Задача.Этап = "Ожидание";
                    }
                    else
                    {
                        Задача.Результат = "<div id='container' class='container-fluid data'>wrong session id</div><script>aupd=false</script>";
                        Задача.Этап = "Обработка";
                    }
                }
                if (Задача.Этап == "Ожидание")
                {
                    dynamic структКонтроллер = Контроллеры.Получить(Задача.ИдКонтроллера) as Структура;
                    if (!(структКонтроллер == Неопределено))
                    {
                        структКонтроллер.Вставить("ВремяНачало", ТекущаяУниверсальнаяДатаВМиллисекундах());
                        Задача.Вставить("структКонтроллер", структКонтроллер);
                        Задача.Вставить("ВремяНачало", ТекущаяУниверсальнаяДатаВМиллисекундах());
                        Задача.ПараметрыЗапроса.Вставить("taskid", Задача.ИдЗадачи);
                        Задача.Этап = "Передать";
                    }
                }
                if (Задача.Этап == "Передать")
                {
                    if (!(ПередатьДанные(Задача.структКонтроллер.Хост, Задача.структКонтроллер.Порт, Задача.ПараметрыЗапроса) == Неопределено))
                    {
                        Задача.Этап = "Обработка";
                    }
                }
                if (Задача.Этап == "Обработка")
                {
                    if (!(Задача.Соединение == Неопределено))
                    {
                        if (НачалоЦикла - Задача.ВремяНачало > 30 * 1000)
                        {
                            Задача.ВремяНачало = НачалоЦикла;
                            if (!(Задача.Соединение.Активно))
                            {
                                Сообщить("webserver: соединение потеряно");
                                dynamic структКонтроллер = Контроллеры.Получить(Задача.ИдКонтроллера) as Структура;
                                if (!(структКонтроллер == Неопределено))
                                {
                                    var ПараметрыЗапроса = Новый_Структура("ИдЗадачи, cmd", Задача.ИдЗадачи, "taskend");
                                    ПередатьДанные(структКонтроллер.Хост, структКонтроллер.Порт, ПараметрыЗапроса);
                                }
                                Задача.Этап = "Завершить";
                            }
                        }
                    }
                    if (!(Задача.Результат == Неопределено))
                    {
                        ОбработатьОтветСервера(Задача);
                    }
                }
                if (Задача.Этап == "Вернуть")
                {
                    if (!(Задача.Соединение.Статус == "Занят"))
                    {
                        Задача.Соединение.Закрыть();
                        Задача.Соединение = Неопределено;
                        Задача.Этап = "Завершить";
                    }
                }
                if (Задача.Этап == "Завершить")
                {
                    Задачи.Удалить(Задача.ИдЗадачи);
                    ЛогСообщить("<- taskid=" + СокрЛП(Задача.ИдЗадачи) + " time=" + Цел(ТекущаяУниверсальнаяДатаВМиллисекундах() - Задача.ВремяНачало) + Загрузка + Задачи.Количество() + " tasks");
                    continue;
                }
                мЗадачи.Добавить(Задача);
            }
        } // ОбработатьЗадачи()
                                               

        void ЛогСообщить(string Сообщение, int Тип = 0)
        {
            Сообщить("" + ТекущаяДата() + " " + Сообщение);
        } // ЛогСообщить()


        void УдалитьКонтроллерИЗадачи(dynamic структКонтроллер)
        {
            Контроллеры.Удалить(структКонтроллер.ИдКонтроллера);
            foreach (dynamic элЗадача in Задачи)
            {
                if (элЗадача.Значение.структКонтроллер == структКонтроллер)
                {
                    элЗадача.Значение.Этап = "Вернуть";
                }
            }
        } // УдалитьКонтроллерИЗадачи()


        void ОбработатьСоединения()
        {
            //var Версия = "0.0.1";

            Порт = 8888;
            var ПортО = 8889;

            if (АргументыКоманднойСтроки.Length > 1)
            {
                ПортО = (int)Число(АргументыКоманднойСтроки[0]);
                Порт = (int)Число(АргументыКоманднойСтроки[1]);
            }

            var Таймаут = 5;
            var TCPСервер = Новый_TCPСервер(Порт);
            TCPСервер.ПриниматьЗаголовки = Истина;
            TCPСервер.ЗапуститьАсинхронно();
            ЛогСообщить("Веб-сервер запущен на порту: " + Порт);
            var TCPСерверО = Новый_TCPСервер(ПортО);
            TCPСерверО.ЗапуститьАсинхронно();
            ЛогСообщить("Ответы на порту: " + ПортО);
            ОстановитьСервер = Ложь;
            TCPСоединение Соединение;
            Задачи = Новый_Соответствие();
            мЗадачи = Новый_Массив();
            Контроллеры = Новый_Соответствие();
            Сообщения = Новый_Массив();
            Соединения = Новый_Массив();
            СоединенияО = Новый_Массив();
            var СуммаЦиклов = 0;
            var РабочийЦикл = 0;
            var ЗамерВремени = ТекущаяУниверсальнаяДатаВМиллисекундах();
            var Загрузка = " ";
            while (!(ОстановитьСервер))
            {
                var НачалоЦикла = ТекущаяУниверсальнаяДатаВМиллисекундах();
                СуммаЦиклов = СуммаЦиклов + 1;
                if (СуммаЦиклов > 999)
                {
                    var ПредЗамер = ЗамерВремени;
                    ЗамерВремени = ТекущаяУниверсальнаяДатаВМиллисекундах();
                    Загрузка = " " + РабочийЦикл / 10 + "% " + Цел(1000 * РабочийЦикл / (ЗамерВремени - ПредЗамер)) + " q/s ";
                    СуммаЦиклов = 0;
                    РабочийЦикл = 0;
                }
                Соединение = TCPСерверО.ПолучитьСоединение(5);
                if (!(Соединение == Неопределено))
                {
                    СоединенияО.Добавить(Соединение);
                    Таймаут = 5;
                }
                var к = СоединенияО.Количество();
                while (к > 0)
                {
                    к = к - 1;
                    Соединение = (TCPСоединение)СоединенияО.Получить(0);
                    СоединенияО.Удалить(0);
                    if (Соединение.Статус == "Данные")
                    {
                        dynamic КонтроллерЗапрос = Неопределено as Структура;
                        try
                        {
                            КонтроллерЗапрос = ДвоичныеДанныеВСтруктуру(Соединение.ПолучитьДвоичныеДанные());
                        }
                        catch (Exception e)
                        {
                            Сообщить("webserver: " + ОписаниеОшибки(e));
                        }
                        if (!(КонтроллерЗапрос == Неопределено))
                        {
                            if (КонтроллерЗапрос.Свойство("procid"))
                            {
                                dynamic структКонтроллер = Контроллеры.Получить(КонтроллерЗапрос.procid) as Структура;
                                if (КонтроллерЗапрос.Свойство("cmd"))
                                {
                                    Сообщить("webserver: " + КонтроллерЗапрос.cmd);
                                    if (КонтроллерЗапрос.cmd == "termproc")
                                    {
                                        if (!(структКонтроллер == Неопределено))
                                        {
                                            УдалитьКонтроллерИЗадачи(структКонтроллер);
                                        }
                                    }
                                    else if (КонтроллерЗапрос.cmd == "init")
                                    {
                                        if (структКонтроллер == Неопределено)
                                        {
                                            ЛогСообщить("Подключен контроллер procid=" + КонтроллерЗапрос.procid);
                                            структКонтроллер = Новый_Структура("ИдКонтроллера, Хост, Порт, ВремяНачало", КонтроллерЗапрос.procid, КонтроллерЗапрос.Хост, КонтроллерЗапрос.Порт, ТекущаяУниверсальнаяДатаВМиллисекундах());
                                            Контроллеры.Вставить(КонтроллерЗапрос.procid, структКонтроллер);
                                        }
                                    }
                                }
                                if (КонтроллерЗапрос.Свойство("taskid"))
                                {
                                    dynamic Задача = Задачи.Получить(КонтроллерЗапрос.taskid) as Структура;
                                    if (!(Задача == Неопределено))
                                    {
                                        var ContentType = "";
                                        if (КонтроллерЗапрос.Свойство("ContentType", ContentType))
                                        {
                                            Задача.Вставить("ContentType", ContentType);
                                        }
                                        Задача.Результат = КонтроллерЗапрос.Результат;
                                    }
                                }
                            }

                            else if (КонтроллерЗапрос.Свойство("cmd"))
                            {
                                if (КонтроллерЗапрос.cmd == "stopserver")
                                {
                                    ОстановитьСервер = Истина;
                                }
                                else if (КонтроллерЗапрос.cmd == "init")
                                {
                                    if (Параметры == Неопределено)
                                    {
                                        Сообщить("Получены параметры");
                                        Параметры = КонтроллерЗапрос;
                                    }
                                }
                            }
                        }
                        Соединение.Закрыть();
                        РабочийЦикл = РабочийЦикл + 1;
                        continue;
                    }
                    else if (Соединение.Статус == "Ошибка")
                    {
                        Соединение.Закрыть();
                        continue;
                    }
                    СоединенияО.Добавить(Соединение);
                }
                if (Параметры == Неопределено)
                {
                    Приостановить(50);
                    continue;
                }
                Соединение = TCPСервер.ПолучитьСоединение(Таймаут);
                if (!(Соединение == Неопределено))
                {
                    Соединения.Добавить(Соединение);
                    Таймаут = 5;
                }
                к = Соединения.Количество();
                while (к > 0)
                {
                    к = к - 1;
                    Соединение = (TCPСоединение)Соединения.Получить(0);
                    Соединения.Удалить(0);
                    if (Соединение.Статус == "Заголовки" || Соединение.Статус == "Данные")
                    {
                        var ТекстовыеДанныеВходящие = "";
                        try
                        {
                            ТекстовыеДанныеВходящие = Соединение.ПолучитьЗаголовки();
                        }
                        catch (Exception e)
                        {
                            Сообщить("webserver: " + ОписаниеОшибки(e));
                        }
                        if (!(ТекстовыеДанныеВходящие == ""))
                        {
                            try
                            {
                                var Запрос = РазобратьЗапросКлиента(ТекстовыеДанныеВходящие, Соединение);
                                ОбработатьЗапросКлиента(Запрос as Структура, Соединение);
                            }
                            catch (Exception e)
                            {
                                ЛогСообщить(ОписаниеОшибки(e));
                                ЛогСообщить("Ошибка обработки запроса:");
                                ЛогСообщить(ТекстовыеДанныеВходящие);
                            }
                            РабочийЦикл = РабочийЦикл + 1;
                        }
                        continue;
                    }
                    else if (Соединение.Статус == "Ошибка")
                    {
                        Соединение.Закрыть();
                        Сообщить("webserver: ошибка соединения");
                        continue;
                    }
                    Соединения.Добавить(Соединение);
                }
                if (Задачи.Количество() != 0)
                {
                    ОбработатьЗадачи();
                }
                if (Сообщения.Количество() != 0)
                {
                    ПередатьДанные(Параметры.Хост, Параметры.ПортД, (Структура)Сообщения.Получить(0));
                    Сообщения.Удалить(0);
                }
                var ВремяЦикла = ТекущаяУниверсальнаяДатаВМиллисекундах() - НачалоЦикла;
                if (ВремяЦикла > 100)
                {
                    Сообщить("!webserver ВремяЦикла=" + ВремяЦикла);
                }
                if (Таймаут < 50)
                {
                    Таймаут = Таймаут + 1;
                }
            }
            TCPСервер.Остановить();
            TCPСерверО.Остановить();
        } // ОбработатьСоединения()

        public void Main()
        {
                            
            СтатусыHTTP = Новый_Соответствие();
            СтатусыHTTP.Вставить(200, "HTTP/1.1 200 OK");
            СтатусыHTTP.Вставить(400, "HTTP/1.1 400 Bad Request");
            СтатусыHTTP.Вставить(401, "HTTP/1.1 401 Unauthorized");
            СтатусыHTTP.Вставить(402, "HTTP/1.1 402 Payment Required");
            СтатусыHTTP.Вставить(403, "HTTP/1.1 403 Forbidden");
            СтатусыHTTP.Вставить(404, "HTTP/1.1 404 Not Found");
            СтатусыHTTP.Вставить(405, "HTTP/1.1 405 Method Not Allowed");
            СтатусыHTTP.Вставить(406, "HTTP/1.1 406 Not Acceptable");
            СтатусыHTTP.Вставить(500, "HTTP/1.1 500 Internal Server Error");
            СтатусыHTTP.Вставить(501, "HTTP/1.1 501 Not Implemented");
            СтатусыHTTP.Вставить(502, "HTTP/1.1 502 Bad Gateway");
            СтатусыHTTP.Вставить(503, "HTTP/1.1 503 Service Unavailable");
            СтатусыHTTP.Вставить(504, "HTTP/1.1 504 Gateway Timeout");
            СтатусыHTTP.Вставить(505, "HTTP/1.1 505 HTTP Version Not Supported");
            СоответствиеРасширенийТипамMIME = Новый_Соответствие();
            СоответствиеРасширенийТипамMIME.Вставить(".html", "text/html");
            СоответствиеРасширенийТипамMIME.Вставить(".css", "text/css");
            СоответствиеРасширенийТипамMIME.Вставить(".js", "text/javascript");
            СоответствиеРасширенийТипамMIME.Вставить(".jpg", "image/jpeg");
            СоответствиеРасширенийТипамMIME.Вставить(".svg", "image/svg+xml");
            СоответствиеРасширенийТипамMIME.Вставить(".jpeg", "image/jpeg");
            СоответствиеРасширенийТипамMIME.Вставить(".png", "image/png");
            СоответствиеРасширенийТипамMIME.Вставить(".gif", "image/gif");
            СоответствиеРасширенийТипамMIME.Вставить(".ico", "image/x-icon");
            СоответствиеРасширенийТипамMIME.Вставить(".zip", "application/x-compressed");
            СоответствиеРасширенийТипамMIME.Вставить(".rar", "application/x-compressed");
            СоответствиеРасширенийТипамMIME.Вставить("default", "text/plain");
            ОбработатьСоединения();
        }

    }

    class MainClass
    {

        public static void Main(string[] args)
        {
            var hostedScript = new HostedScriptEngine();
            var app = new webserver("webserver");
            app._syscon = new SystemGlobalContext();
            app._syscon.ApplicationHost = new ApplicationHost();
            app.Main();

        }
    }
}

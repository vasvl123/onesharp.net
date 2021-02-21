﻿// /*----------------------------------------------------------
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v.2.0. If a copy of the MPL
// was not distributed with this file, You can obtain one
// at http://mozilla.org/MPL/2.0/.
// ----------------------------------------------------------*/

using System;
using ScriptEngine.HostedScript.Library;

namespace morphserver
{
    class morphserver : functions
    {
        public morphserver() : base("morphserver")
        {
        }

        Перем Хост;
        int Порт;
        bool ОстановитьСервер;
        Соответствие Задачи;
        Массив мЗадачи;
        Массив Соединения;
        treedb Связи;
        Перем Данные;

        
        Массив МассивИзСтроки(string стр)
        {
            var м = Новый_Массив();
            var дстр = СтрДлина(стр);
            for (int н = 1; н <= дстр; н++) {
                м.Добавить(КодСимвола(Сред(стр, н, 1)));
            }
            return м;
        } // МассивИзСтроки()


        bool ВыполнитьЗадачу(dynamic структЗадача)
        {

            Перем Команда = null;

            структЗадача.Запрос.Свойство("cmd", ref Команда);

            if (Команда == "stopserver")
            {
                ОстановитьСервер = Истина;
                return Ложь;
            }

            структЗадача.Запрос.Параметры.Свойство("Действие", ref Команда);

            if (Команда == "ФормыСлов")
            {

                var НачалоЦикла = ТекущаяУниверсальнаяДатаВМиллисекундах();

                while (ТекущаяУниверсальнаяДатаВМиллисекундах() - НачалоЦикла < 50)
                {

                    Массив мСлова = СтрРазделить(структЗадача.Запрос.Параметры.Слова, Символы.ПС);

                    if (структЗадача.Результат.Количество() == мСлова.Количество())
                    {
                        return Истина;
                    }

                    var сл = (string)мСлова.Получить(структЗадача.Результат.Количество());
                    var слф = ВРег(сл);
                    var рез = "";

                    Сообщить(слф);

                    н = Связи.НайтиЗначение(МассивИзСтроки(Символ(1) + слф + Символ(1)));

                    if (!(н == 0))
                    {
                        мр1 = Связи.ПолучитьВложенныеЗначения(н);
                        foreach (мф1 in мр1)
                        {
                            уПрервать = Ложь;
                            сЛемма = Связи.ПолучитьСтроку(мф1[0]);
                            мр2 = Связи.ПолучитьВложенныеЗначения(мф1[1]);
                            foreach (мф2 in мр2)
                            {
                                нФорма = Связи.ПолучитьСтроку(мф2[0]);
                                мр3 = Связи.ПолучитьВложенныеЗначения(мф2[1]);
                                foreach (мф3 in мр3)
                                {
                                    нЛемма = Связи.ПолучитьСтроку(мф3[0]);
                                    рез = рез + ? (рез == "", "", Символы.ПС) + слф + Символы.Таб + сЛемма + Символы.Таб + нФорма + Символы.Таб + нЛемма;
                                    if (сЛемма == "PREP" || сЛемма == "CONJ")
                                    {
                                        уПрервать = Истина;
                                        break; // и хватит
                                    }
                                }
                                if (уПрервать)
                                {
                                    break;
                                }
                            }
                            if (уПрервать)
                            {
                                break;
                            }
                        }
                    }

                    if (рез == "")
                    { // форма слова не найдена
                        if (СтрНайти(слф, "Ё") == 0)
                        { // е заменить на ё
                            foreach (н == 1 По СтрДлина(слф)) {
                                if (Сред(слф, н, 1) == "Е")
                                {
                                    структЗадача.Запрос.Параметры.Слова = структЗадача.Запрос.Параметры.Слова + Символы.ПС + Лев(слф, н - 1) + "Ё" + Сред(слф, н + 1);
                                }
                            }
                        }
                    }

                    структЗадача.Результат.Вставить("s_" + структЗадача.Результат.Количество(), Новый Структура("Слово, Результат", сл, рез));

                }

                return Ложь;

            }
            else if (Команда == "Связи")
            {

                мСлова = СтрРазделить(структЗадача.Запрос.Параметры.Слова, Символы.ПС);

                foreach (бигр in мСлова)
                {

                    мб = СтрРазделить(бигр, Символы.Таб);

                    ток1имя = мб[0];
                    ток1нач = Связи.НайтиЗначение(МассивИзСтроки(Символ(1) + мб[1]));
                    ток1гр = Связи.ДобавитьЗначение(МассивИзСтроки(Символ(2) + мб[2]));
                    свимя = Связи.ДобавитьЗначение(МассивИзСтроки(Символ(3) + ВРег(мб[3])));
                    ток2имя = мб[4];
                    ток2нач = Связи.НайтиЗначение(МассивИзСтроки(Символ(1) + мб[5]));
                    ток2гр = Связи.ДобавитьЗначение(МассивИзСтроки(Символ(2) + мб[6]));
                    р = мб[7];

                    гр = МассивИзСтроки(Символ(2) + мб[2] + Символ(1));
                    гр.Добавить(ток2гр);
                    гр.Добавить(свимя);

                    св = МассивИзСтроки(Символ(1) + мб[1] + Символ(2));
                    св.Добавить(ток2нач);
                    св.Добавить(свимя);
                    св.Добавить(КодСимвола(р));

                    if (р == "+")
                    { // добавить грамматику и связь
                        Связи.ДобавитьЗначение(гр);
                        Связи.ДобавитьЗначение(св);

                    }
                    else if (р == "-")
                    { // добавить неверную связь
                        Связи.ДобавитьЗначение(св);

                    }
                    else if (р == "")
                    { // удалить связь
                        св[св.Количество() - 1] = КодСимвола("-");
                        н = Связи.НайтиЗначение(св);
                        if (!(н == 0))
                        {
                            Связи.УдалитьЗначение(н);
                        }
                        св[св.Количество() - 1] = КодСимвола("+");
                        н = Связи.НайтиЗначение(св);
                        if (!(н == 0))
                        {
                            Связи.УдалитьЗначение(н);
                        }
                    }

                    if (р == "г")
                    { // удалить грамматику
                        н = Связи.НайтиЗначение(гр);
                        if (!(н == 0))
                        {
                            Связи.УдалитьЗначение(н);
                        }
                    }

                    //структЗадача.Результат.Вставить(ток1имя + "_" + ток2имя, р);

                }

                return Истина;

            }
            else if (Команда == "Грамматики")
            {

                сгр = Новый Соответствие;

                сл = структЗадача.Запрос.Параметры.Слова;

                мсл = СтрРазделить(сл, Символы.ПС);

                инд = Новый Соответствие; // индекс

                foreach (гр in мсл)
                {

                    if (!(сгр.Получить(гр) == Неопределено))
                    {
                        continue;
                    }
                    сгр.Вставить(гр, "");

                    мгр = СтрРазделить(гр, Символы.Таб);

                    н = инд.Получить(мгр[0]);
                    if (н == Неопределено)
                    {
                        н = Связи.НайтиЗначение(МассивИзСтроки(Символ(2) + мгр[0])); // найти грамматику1
                        инд.Вставить(мгр[0], н);
                    }

                    if (!(н == 0))
                    { // найдено совпадение по образцу

                        гр2 = инд.Получить(мгр[1]);
                        if (гр2 == Неопределено)
                        {
                            гр2 = Связи.НайтиЗначение(МассивИзСтроки(Символ(2) + мгр[1])); // найти грамматику2
                            инд.Вставить(мгр[1], гр2);
                        }

                        if (!(гр2 == 0))
                        {

                            м = Новый Массив;
                            м.Добавить(1);
                            м.Добавить(гр2);
                            н = Связи.НайтиЗначение(м, н);

                            мр = инд.Получить(н);
                            if (мр == Неопределено)
                            {
                                мр = Связи.ПолучитьВложенныеЗначения(н);
                                инд.Вставить(н, мр);
                            }

                            foreach (мф in мр)
                            {

                                н = инд.Получить(мгр[2]);
                                if (н == Неопределено)
                                {
                                    н = Связи.НайтиЗначение(МассивИзСтроки(Символ(1) + мгр[2])); // найти форму1
                                    инд.Вставить(мгр[2], н);
                                }

                                б = "";

                                if (!(н == 0))
                                {

                                    нф2 = инд.Получить(мгр[3]);
                                    if (нф2 == Неопределено)
                                    {
                                        нф2 = Связи.НайтиЗначение(МассивИзСтроки(Символ(1) + мгр[3])); // найти форму2
                                        инд.Вставить(мгр[3], нф2);
                                    }

                                    if (!(нф2 == 0))
                                    {

                                        м = Новый Массив;
                                        м.Добавить(2);
                                        м.Добавить(нф2);
                                        н = Связи.НайтиЗначение(м, н); // позиция нужного значения

                                        мр1 = инд.Получить(н);
                                        if (мр1 == Неопределено)
                                        {
                                            мр1 = Связи.ПолучитьВложенныеЗначения(н);
                                            инд.Вставить(н, мр1);
                                        }

                                        foreach (мф1 in мр1)
                                        {
                                            if (мф[0] == мф1[0])
                                            {
                                                мб = Связи.ПолучитьВложенныеЗначения(мф1[1]); // что внутри
                                                foreach (б1 in мб)
                                                {
                                                    б = Символ(б1[0]);
                                                }
                                            }
                                        }

                                    }

                                }

                                структЗадача.Результат.Вставить("гр_" + структЗадача.Результат.Количество(), гр + "_" + Связи.ПолучитьСтроку(мф[0]) + Символы.Таб + б);

                            }

                        }

                    }

                }

                return Истина;

            }
            else if (Команда == "Элемент")
            {

                поз = структЗадача.Запрос.Параметры.Позиция;
                Раскрыть = Истина;
                //структЗадача.Запрос.Свойство("Раскрыть", Раскрыть);

                ИмяДанных = структЗадача.Запрос.Параметры.База;
                База = Данные.Получить(ИмяДанных);

                if (База == Неопределено)
                {
                    return Истина;
                }

                з = "";

                Элемент = База.ПолучитьЭлемент(поз);
                структЗадача.Результат.Вставить("Элемент", Элемент);
                структЗадача.Результат.Вставить("Элементы", Новый Массив);

                if (поз == "0")
                { // начало файла
                    м = База.ПолучитьЗначения(Элемент.Соседний);
                    з = ИмяДанных;
                }
                else
                {
                    м = База.ПолучитьЗначения(Элемент.Дочерний);
                    зн = База.ПолучитьМассив(поз);

                    if (ИмяДанных == "Связи")
                    {
                        тэ = "НачалоФайла";
                        foreach (зэ in зн)
                        {
                            if (тэ == "НачалоФайла")
                            {
                                if (зэ == 1)
                                {
                                    тэ = "Форма";
                                }
                                else if (зэ == 2)
                                {
                                    тэ = "Лемма";
                                }
                                else if (зэ == 3)
                                {
                                    тэ = "Отношение";
                                }

                            }
                            else if (тэ == "Форма")
                            {
                                if (зэ == 1)
                                {
                                    тэ = "НачФорма";
                                    з = тэ;
                                }
                                else if (зэ == 2)
                                {
                                    тэ = "СвязьСл";
                                    з = тэ;
                                }
                                else
                                {
                                    з = з + Символ(зэ);
                                }
                            }
                            else if (тэ == "НачФорма")
                            {
                                з = "";
                                тэ = "ПозицияЛеммы1";
                            }
                            else if (тэ == "ПозицияЛеммы1")
                            {
                                тэ = "ПозицияНФ1";
                            }
                            else if (тэ == "ПозицияНФ1")
                            {
                                тэ = "ПозицияЛеммыНФ1";
                            }
                            else if (тэ == "ПозицияЛеммыНФ1")
                            {
                                тэ = "";
                            }
                            else if (тэ == "СвязьСл")
                            {
                                з = "";
                                тэ = "ПозицияФормы1";
                            }
                            else if (тэ == "ПозицияФормы1")
                            {
                                тэ = "ПозицияСвязи1";
                            }
                            else if (тэ == "ПозицияСвязи1")
                            {
                                тэ = "СимволПМ";
                            }
                            else if (тэ == "СимволПМ")
                            {
                                з = Символ(зэ);
                                тэ = "";

                            }
                            else if (тэ == "Лемма")
                            {
                                if (зэ == 1)
                                {
                                    тэ = "СвязьГр";
                                    з = тэ;
                                }
                                else
                                {
                                    з = з + Символ(зэ);
                                }
                            }
                            else if (тэ == "СвязьГр")
                            {
                                з = "";
                                тэ = "ПозицияЛеммы2";
                            }
                            else if (тэ == "ПозицияЛеммы2")
                            {
                                тэ = "ПозицияСвязи2";
                            }
                            else if (тэ == "ПозицияСвязи2")
                            {
                                тэ = "";

                            }
                            else if (тэ == "Отношение")
                            {
                                з = з + Символ(зэ);
                            }
                        }

                    }
                    else if (ИмяДанных == "Тезаурус")
                    {

                        тэ = "НачалоФайла";
                        foreach (зэ in зн)
                        {
                            if (тэ == "НачалоФайла")
                            {
                                if (зэ == 1)
                                {
                                    тэ = "Синсет";
                                }
                            }
                            else if (тэ == "Синсет")
                            {
                                if (зэ < 16)
                                {
                                    if (зэ == 1)
                                    {
                                        тэ = "Син";
                                    }
                                    else if (зэ == 2)
                                    {
                                        тэ = "Выше";
                                    }
                                    else if (зэ == 3)
                                    {
                                        тэ = "Ниже";
                                    }
                                    else if (зэ == 4)
                                    {
                                        тэ = "Целое";
                                    }
                                    else if (зэ == 5)
                                    {
                                        тэ = "Ассоц";
                                    }
                                    else if (зэ == 6)
                                    {
                                        тэ = "Часть";
                                    }
                                    з = тэ;
                                }
                                else
                                {
                                    з = з + Символ(зэ);
                                }
                            }
                            else
                            {
                                з = "";
                                тэ = "Синсет";
                            }
                        }

                    }

                    if (з == "")
                    {
                        if (зэ < 16)
                        {
                            з = тэ;
                        }
                        else
                        {
                            з = База.ПолучитьСтроку(зэ);
                            // Если ИмяДанных = "Связи" Тогда
                            if (ИмяДанных == "Тезаурус")
                            {
                                Элемент = База.ПолучитьЭлемент(зэ);
                                структЗадача.Результат.Вставить("Элемент", Элемент);
                                м = База.ПолучитьЗначения(Элемент.Дочерний);
                            }
                        }
                    }
                    else
                    { // текст

                        if (!(Элемент.Значение < 16 && Раскрыть == Истина))
                        {
                            if (м.Количество() == 1)
                            { // раскрыть дерево
                                while (м.Количество() == 1)
                                {
                                    эл = База.ПолучитьЭлемент(Элемент.Дочерний);
                                    if (эл.Значение < 16)
                                    {
                                        break;
                                    }
                                    з = з + Символ(эл.Значение);
                                    Элемент = эл;
                                    м = База.ПолучитьЗначения(Элемент.Дочерний);
                                }
                                структЗадача.Результат.Вставить("Элемент", Элемент);
                            }
                        }

                    }

                }

                foreach (эл in м)
                {
                    структЗадача.Результат.Элементы.Добавить(эл[1]);
                }

                структЗадача.Результат.Вставить("Строка", з);

                return Истина;

            }

        }


        void ОбработатьСоединения()
        {

            Порт = 8886;

            if (АргументыКоманднойСтроки.Количество())
            {
                Порт = Число(АргументыКоманднойСтроки[0]);
            }

            Таймаут = 5;

            TCPСервер = Новый TCPСервер(Порт);
            TCPСервер.ЗапуститьАсинхронно();

            Сообщить(СокрЛП(ТекущаяДата()) + " Сервер морфологии запущен на порту: " + Порт);

            Данные = Новый Соответствие;

            ПодключитьСценарий(ОбъединитьПути(ТекущийКаталог(), "treedb.os"), "treedb");
            Связи = Новый treedb(ОбъединитьПути(ТекущийКаталог(), "morph", "Связи.dat"));
            Данные.Вставить("Связи", Связи);
            Тезаурус = Новый treedb(ОбъединитьПути(ТекущийКаталог(), "morph", "Тезаурус.dat"));
            Данные.Вставить("Тезаурус", Тезаурус);

            Задачи = Новый Соответствие;
            мЗадачи = Новый Массив;

            ОстановитьСервер = Ложь;
            ПерезапуститьСервер = Ложь;
            Соединение = Неопределено;

            Соединения = Новый Массив();

            СуммаЦиклов = 0;
            РабочийЦикл = 0;
            ЗамерВремени = ТекущаяУниверсальнаяДатаВМиллисекундах();

            while (!(ОстановитьСервер))
            {

                НачалоЦикла = ТекущаяУниверсальнаяДатаВМиллисекундах();
                СуммаЦиклов = СуммаЦиклов + 1;

                if (СуммаЦиклов > 999)
                {
                    ПредЗамер = ЗамерВремени;
                    ЗамерВремени = ТекущаяУниверсальнаяДатаВМиллисекундах();
                    Загрузка = " " + РабочийЦикл / 10 + "% " + Цел(1000 * РабочийЦикл / (ЗамерВремени - ПредЗамер)) + " q/s " + Задачи.Количество() + " tasks";
                    СуммаЦиклов = 0;
                    РабочийЦикл = 0;
                }

                к = мЗадачи.Количество();
                while (к > 0 && !(ТекущаяУниверсальнаяДатаВМиллисекундах() - НачалоЦикла > 50))
                {
                    к = к - 1;
                    структЗадача = мЗадачи.Получить(0);
                    мЗадачи.Удалить(0);

                    ЕстьРезультат = Ложь;
                    РабочийЦикл = РабочийЦикл + 1;
                    try
                    {
                        ЕстьРезультат = ВыполнитьЗадачу(структЗадача);
                    }
                    catch
                    {
                        Сообщить(ОписаниеОшибки());
                        Задачи.Удалить(структЗадача.ИдЗадачи);
                        continue;
                    }
                    if (ЕстьРезультат == Истина)
                    {
                        try
                        {
                            ОбратныйЗапрос = "";
                            if (структЗадача.Запрос.Свойство("ОбратныйЗапрос", ОбратныйЗапрос))
                            { // возвращаем результат
                                ОбратныйЗапрос.Вставить("РезультатДанные", Новый Структура("Ответ, Результат", структЗадача.Ответ, структЗадача.Результат));
                                if (ПередатьДанные(ОбратныйЗапрос.Хост, ОбратныйЗапрос.Порт, ОбратныйЗапрос) == Неопределено)
                                {
                                    continue;
                                }
                                Сообщить("morphserver " + ТекущаяДата() + " time=" + (ТекущаяДата() - структЗадача.ВремяНачало) + Загрузка);
                                структЗадача.Результат = Неопределено;
                            }
                        }
                        catch
                        {
                            Сообщить(ОписаниеОшибки());
                        }
                        Задачи.Удалить(структЗадача.ИдЗадачи);
                        //Сообщить("morphserver: всего задач " + Задачи.Количество());
                        continue;
                    }

                    мЗадачи.Добавить(структЗадача);

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
                    Соединение = Соединения.Получить(0);
                    Соединения.Удалить(0);

                    if (Соединение.Статус == "Данные")
                    {

                        try
                        {
                            Запрос = Неопределено;
                            Запрос = ДвоичныеДанныеВСтруктуру(Соединение.ПолучитьДвоичныеДанные());
                        }
                        catch
                        {
                            Сообщить("morphserver: " + ОписаниеОшибки());
                        }

                        if (!(Запрос == Неопределено))
                        {
                            структЗадача = Новый Структура("ИдЗадачи, Запрос, Ответ, Результат, ВремяНачало", ПолучитьИД(), Запрос, Неопределено, Новый Структура(), ТекущаяДата());
                            Задачи.Вставить(структЗадача.ИдЗадачи, структЗадача);
                            мЗадачи.Добавить(структЗадача);
                            //Сообщить("dataserver: всего задач " + Задачи.Количество());
                        }

                        Соединение.Закрыть();
                        continue;

                    }
                    else if (Соединение.Статус == "Ошибка")
                    {

                        Соединение.Закрыть();
                        continue;

                    }

                    Соединения.Добавить(Соединение);

                }

                ВремяЦикла = ТекущаяУниверсальнаяДатаВМиллисекундах() - НачалоЦикла;
                if (ВремяЦикла > 100)
                {
                    Сообщить("!morphserver ВремяЦикла=" + ВремяЦикла);
                }
                if (Таймаут < 50)
                {
                    Таймаут = Таймаут + 1;
                }

            }

            if (!(Связи == Неопределено))
            {
                Связи.Закрыть();
            }

            TCPСервер.Остановить();
            Сообщить("Завершил работу сервера морфологии.");

        }

        public void Main()
        {
            ОбработатьСоединения();
        }

    }
}

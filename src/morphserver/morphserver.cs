﻿// /*----------------------------------------------------------
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v.2.0. If a copy of the MPL
// was not distributed with this file, You can obtain one
// at http://mozilla.org/MPL/2.0/.
// ----------------------------------------------------------*/

using System;

namespace onesharp
{
    class morphserver : Onesharp
    {

        public morphserver() : base("morpserver") {}

        functions Функции = new functions();

        //Перем Хост;
        int Порт;
        bool ОстановитьСервер;
        Соответствие Задачи;
        Массив мЗадачи;
        Массив Соединения;
        treedb Связи;
        Соответствие Данные;
                
        Массив МассивИзСтроки(string стр)
        {
            var м = Массив.Новый();
            var дстр = Стр.Длина(стр);
            for (int н = 1; н <= дстр; н++) {
                м.Добавить(КодСимвола(Сред(стр, н, 1)));
            }
            return м;
        } // МассивИзСтроки()


        public class ТипПараметры : Структура
        {
            public string Слова => (string)base["Слова"];
            public int Позиция => (int)base["Позиция"];
            public string База => (string)base["База"];

            public ТипПараметры(string strProperties, params object[] values) : base(strProperties, values) { }
        }

        public class ТипЗапрос : Структура
        {
            public string cmd => (string)base["cmd"];
            public ТипПараметры Параметры => (ТипПараметры)base["Параметры"];

            public ТипЗапрос(string strProperties, params object[] values) : base(strProperties, values) { }
        }

        public class ТипРезультат : Структура
        {
            public Массив Элементы => (Массив)base["Элементы"];
            public int Позиция => (int)base["Позиция"];
            public string База => (string)base["База"];

            public ТипРезультат(string strProperties, params object[] values) : base(strProperties, values) { }
        }

        public class ТипЗадача : Структура
        {
            public string ИдЗадачи;
            public ТипЗапрос Запрос;
            public ТипРезультат Результат;
            public string Ответ;
            public decimal ВремяНачало;

            public ТипЗадача(string strProperties, params object[] values) : base(strProperties, values) { }
        }

        public class ТипОбратныйЗапрос : Структура
        {
            public string Хост;
            public int Порт;

            public ТипОбратныйЗапрос(string strProperties, params object[] values) : base(strProperties, values) { }        
        }

        public class ТипЭлемент : Структура
        {
            public int Соседний;
            public int Дочерний;
            public int Значение;

            public ТипЭлемент(string strProperties, params object[] values) : base(strProperties, values) { }        
        }

        bool ВыполнитьЗадачу(ТипЗадача структЗадача)
        {

            object Команда = "";

            структЗадача.Запрос.Свойство("cmd", out Команда);

            if (Команда == "stopserver")
            {
                ОстановитьСервер = Истина;
                return Ложь;
            }

            структЗадача.Запрос.Параметры.Свойство("Действие", out Команда);

            if (Команда == "ФормыСлов")
            {

                var НачалоЦикла = ТекущаяУниверсальнаяДатаВМиллисекундах();

                while (ТекущаяУниверсальнаяДатаВМиллисекундах() - НачалоЦикла < 50)
                {

                    var мСлова = new Массив(Стр.Разделить(структЗадача.Запрос.Параметры.Слова, Символы.ПС));

                    if (структЗадача.Результат.Количество() == мСлова.Количество())
                    {
                        return Истина;
                    }

                    var сл = (string)мСлова.Получить(структЗадача.Результат.Количество());
                    var слф = ВРег(сл);
                    var рез = "";

                    Сообщить(слф);

                    var н = Связи.НайтиЗначение(МассивИзСтроки(Символ(1) + слф + Символ(1)));

                    if (!(н == 0))
                    {
                        var мр1 = Связи.ПолучитьВложенныеЗначения(н);
                        foreach (Массив мф1 in мр1 as Массив)
                        {
                            var уПрервать = Ложь;
                            var сЛемма = Связи.ПолучитьСтроку((int)мф1[0]);
                            var мр2 = Связи.ПолучитьВложенныеЗначения((int)мф1[1]);
                            foreach (Массив мф2 in мр2 as Массив)
                            {
                                var нФорма = Связи.ПолучитьСтроку((int)мф2[0]);
                                var мр3 = Связи.ПолучитьВложенныеЗначения((int)мф2[1]);
                                foreach (Массив мф3 in мр3 as Массив)
                                {
                                    var нЛемма = Связи.ПолучитьСтроку((int)мф3[0]);
                                    рез = рез + ((рез == "") ? "" : Символы.ПС) + слф + Символы.Таб + сЛемма + Символы.Таб + нФорма + Символы.Таб + нЛемма;
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
                        if (Стр.Найти(слф, "Ё") == 0)
                        { // е заменить на ё
                            var дслф = Стр.Длина(слф);
                            for (н = 1; н <= дслф; н++) {
                                if (Сред(слф, н, 1) == "Е")
                                {
                                    структЗадача.Запрос.Параметры.Слова = структЗадача.Запрос.Параметры.Слова + Символы.ПС + Лев(слф, н - 1) + "Ё" + Сред(слф, н + 1);
                                }
                            }
                        }
                    }

                    структЗадача.Результат.Вставить("s_" + структЗадача.Результат.Количество(), Структура.Новый("Слово, Результат", сл, рез));

                }

                return Ложь;

            }
            else if (Команда == "Связи")
            {

                var мСлова = new Массив(Стр.Разделить(структЗадача.Запрос.Параметры.Слова, Символы.ПС));

                foreach (string бигр in мСлова)
                {
                    var мб = Стр.Разделить(бигр, Символы.Таб);

                    string ток1имя = мб[0];
                    var ток1нач = Связи.НайтиЗначение(МассивИзСтроки(Символ(1) + мб[1]));
                    var ток1гр = Связи.ДобавитьЗначение(МассивИзСтроки(Символ(2) + мб[2]));
                    var свимя = Связи.ДобавитьЗначение(МассивИзСтроки(Символ(3) + ВРег(мб[3])));
                    string ток2имя = мб[4];
                    var ток2нач = Связи.НайтиЗначение(МассивИзСтроки(Символ(1) + мб[5]));
                    var ток2гр = Связи.ДобавитьЗначение(МассивИзСтроки(Символ(2) + мб[6]));
                    string р = мб[7];

                    var гр = МассивИзСтроки(Символ(2) + мб[2] + Символ(1));
                    гр.Добавить(ток2гр);
                    гр.Добавить(свимя);

                    var св = МассивИзСтроки(Символ(1) + мб[1] + Символ(2));
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
                        var н = Связи.НайтиЗначение(св);
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
                        var н = Связи.НайтиЗначение(гр);
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

                var сгр = Соответствие.Новый();

                string сл = структЗадача.Запрос.Параметры.Слова;

                var мсл = Стр.Разделить(сл, Символы.ПС);

                var инд = Соответствие.Новый(); // индекс

                foreach (string гр in мсл)
                {
                    if (!(сгр.Получить(гр) is null))
                    {
                        continue;
                    }
                    сгр.Вставить(гр, "");

                    var мгр = Стр.Разделить(гр, Символы.Таб);

                    int н;
                    var _н = инд.Получить(мгр[0]);
                    if (_н is null)
                    {
                        н = Связи.НайтиЗначение(МассивИзСтроки(Символ(2) + мгр[0])); // найти грамматику1
                        инд.Вставить(мгр[0], н);
                    }
                    else
                    {
                        н = (int)_н;
                    }

                    if (!(н == 0))
                    { // найдено совпадение по образцу

                        var гр2 = (int?)инд.Получить(мгр[1]);
                        if (гр2 is null)
                        {
                            гр2 = Связи.НайтиЗначение(МассивИзСтроки(Символ(2) + мгр[1])); // найти грамматику2
                            инд.Вставить(мгр[1], гр2);
                        }

                        if (!(гр2 == 0))
                        {

                            var м = Массив.Новый();
                            м.Добавить(1);
                            м.Добавить(гр2);
                            н = Связи.НайтиЗначение(м, н);

                            var мр = инд.Получить(н);
                            if (мр is null)
                            {
                                мр = Связи.ПолучитьВложенныеЗначения(н);
                                инд.Вставить(н, мр);
                            }

                            foreach (Массив мф in мр as Массив)
                            {

                                _н = инд.Получить(мгр[2]);
                                if (_н is null)
                                {
                                    н = Связи.НайтиЗначение(МассивИзСтроки(Символ(1) + мгр[2])); // найти форму1
                                    инд.Вставить(мгр[2], н);
                                }
                                {
                                    н = (int)_н;
                                }

                                var б = "";

                                if (!(н == 0))
                                {

                                    int нф2;
                                    var _нф2 = инд.Получить(мгр[3]);
                                    if (_нф2 is null)
                                    {
                                        нф2 = Связи.НайтиЗначение(МассивИзСтроки(Символ(1) + мгр[3])); // найти форму2
                                        инд.Вставить(мгр[3], нф2);
                                    }
                                    else
                                    {
                                        нф2 = (int)_нф2;
                                    }

                                    if (!(нф2 == 0))
                                    {

                                        м = Массив.Новый();
                                        м.Добавить(2);
                                        м.Добавить(нф2);
                                        н = Связи.НайтиЗначение(м, н); // позиция нужного значения

                                        var мр1 = инд.Получить(н) as Массив;
                                        if (мр1 is null)
                                        {
                                            мр1 = Связи.ПолучитьВложенныеЗначения(н);
                                            инд.Вставить(н, мр1);
                                        }

                                        foreach (Массив мф1 in мр1)
                                        {
                                            if (мф[0] == мф1[0])
                                            {
                                                var мб = Связи.ПолучитьВложенныеЗначения((int)мф1[1]); // что внутри
                                                foreach (Массив б1 in мб)
                                                {
                                                    б = Символ((int)б1[0]);
                                                }
                                            }
                                        }

                                    }

                                }

                                структЗадача.Результат.Вставить("гр_" + структЗадача.Результат.Количество(), гр + "_" + Связи.ПолучитьСтроку((int)мф[0]) + Символы.Таб + б);

                            }

                        }

                    }

                }

                return Истина;

            }
            else if (Команда == "Элемент")
            {

                var поз = (int)Число(структЗадача.Запрос.Параметры.Позиция);
                var Раскрыть = Истина;
                //структЗадача.Запрос.Свойство("Раскрыть", Раскрыть);

                string ИмяДанных = структЗадача.Запрос.Параметры.База;
                var База = Данные.Получить(ИмяДанных) as treedb;

                if (База == Неопределено)
                {
                    return Истина;
                }

                var з = "";

                var Элемент = База.ПолучитьЭлемент(поз) as ТипЭлемент;
                структЗадача.Результат.Вставить("Элемент", Элемент);
                структЗадача.Результат.Вставить("Элементы", Массив.Новый());

                Массив м;

                if (поз == 0)
                { // начало файла
                    м = База.ПолучитьЗначения(Элемент.Соседний);
                    з = ИмяДанных;
                }
                else
                {
                    м = База.ПолучитьЗначения(Элемент.Дочерний);
                    var зн = База.ПолучитьМассив(поз);
                    int зэ = 0;
                    string тэ = "";

                    if (ИмяДанных == "Связи")
                    {
                        тэ = "НачалоФайла";
                        foreach (int _зэ in зн)
                        {
                            зэ = _зэ;
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
                        foreach (КлючИЗначение _зэ in зн)
                        {
                            зэ = (int)_зэ.Значение;
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
                                Элемент = (ТипЭлемент)База.ПолучитьЭлемент(зэ);
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
                                    var эл = (ТипЭлемент)База.ПолучитьЭлемент(Элемент.Дочерний);
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

                foreach (Массив эл in м)
                {
                    структЗадача.Результат.Элементы.Добавить(эл[1]);
                }

                структЗадача.Результат.Вставить("Строка", з);

                return Истина;

            }


            return Ложь;

        }


        void ОбработатьСоединения()
        {

            Порт = 8886;

            if (АргументыКоманднойСтроки.Length != 0)
            {
                Порт = (int)Число(АргументыКоманднойСтроки[0]);
            }

            var Таймаут = 5;

            var TCPСервер = new TCPСервер(Порт);
            TCPСервер.ЗапуститьАсинхронно();

            Сообщить(ТекущаяДата() + " Сервер морфологии запущен на порту: " + Порт);

            Данные = Соответствие.Новый();

            //ПодключитьСценарий(ОбъединитьПути(ТекущийКаталог(), "treedb.os"), "treedb");
            Связи = new treedb(ОбъединитьПути(ТекущийКаталог(), "morph", "Связи.dat"));
            Данные.Вставить("Связи", Связи);
            var Тезаурус = new treedb(ОбъединитьПути(ТекущийКаталог(), "morph", "Тезаурус.dat"));
            Данные.Вставить("Тезаурус", Тезаурус);

            Задачи = Соответствие.Новый();
            мЗадачи = Массив.Новый();

            ОстановитьСервер = Ложь;
            var ПерезапуститьСервер = Ложь;
            TCPСоединение Соединение = null;

            Соединения = Массив.Новый();

            var СуммаЦиклов = 0;
            var РабочийЦикл = 0;
            var ЗамерВремени = ТекущаяУниверсальнаяДатаВМиллисекундах();

            string Загрузка = "";

            while (!(ОстановитьСервер))
            {

                var НачалоЦикла = ТекущаяУниверсальнаяДатаВМиллисекундах();
                СуммаЦиклов = СуммаЦиклов + 1;

                if (СуммаЦиклов > 999)
                {
                    var ПредЗамер = ЗамерВремени;
                    ЗамерВремени = ТекущаяУниверсальнаяДатаВМиллисекундах();
                    Загрузка = " " + РабочийЦикл / 10 + "% " + Цел(1000 * РабочийЦикл / (ЗамерВремени - ПредЗамер)) + " q/s " + Задачи.Количество() + " tasks";
                    СуммаЦиклов = 0;
                    РабочийЦикл = 0;
                }

                var к = мЗадачи.Количество();
                while (к > 0 && !(ТекущаяУниверсальнаяДатаВМиллисекундах() - НачалоЦикла > 50))
                {
                    к = к - 1;
                    var структЗадача = (ТипЗадача)мЗадачи.Получить(0);
                    мЗадачи.Удалить(0);

                    var ЕстьРезультат = Ложь;
                    РабочийЦикл = РабочийЦикл + 1;
                    try
                    {
                        ЕстьРезультат = ВыполнитьЗадачу(структЗадача);
                    }
                    catch (Exception e)
                    {
                        Сообщить(ОписаниеОшибки(e));
                        Задачи.Удалить(структЗадача.ИдЗадачи);
                        continue;
                    }
                    if (ЕстьРезультат == Истина)
                    {
                        try
                        {
                            object _ОбратныйЗапрос = null;
                            if (структЗадача.Запрос.Свойство("ОбратныйЗапрос", out _ОбратныйЗапрос))
                            { // возвращаем результат
                                var ОбратныйЗапрос = (ТипОбратныйЗапрос)_ОбратныйЗапрос;
                                ОбратныйЗапрос.Вставить("РезультатДанные", Структура.Новый("Ответ, Результат", структЗадача.Ответ, структЗадача.Результат));
                                if (Функции.ПередатьДанные(ОбратныйЗапрос.Хост, (int)Число(ОбратныйЗапрос.Порт), ОбратныйЗапрос) == Неопределено)
                                {
                                    continue;
                                }
                                Сообщить("morphserver " + ТекущаяДата() + " time=" + (ТекущаяУниверсальнаяДатаВМиллисекундах() - структЗадача.ВремяНачало) + Загрузка);
                                структЗадача.Результат = null;
                            }
                        }
                        catch (Exception e)
                        {
                            Сообщить(ОписаниеОшибки(e));
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
                    Соединение = (TCPСоединение)Соединения.Получить(0);
                    Соединения.Удалить(0);

                    if (Соединение.Статус == "Данные")
                    {

                        Структура Запрос = null;

                        try
                        {
                            Запрос = (Структура)Функции.ДвоичныеДанныеВСтруктуру(Соединение.ПолучитьДвоичныеДанные());
                        }
                        catch (Exception e)
                        {
                            Сообщить("morphserver: " + ОписаниеОшибки(e));
                        }

                        if (!(Запрос == Неопределено))
                        {
                            var структЗадача = Структура.Новый("ИдЗадачи, Запрос, Ответ, Результат, ВремяНачало", ПолучитьИД(), Запрос, Неопределено, Структура.Новый(), ТекущаяУниверсальнаяДатаВМиллисекундах());
                            Задачи.Вставить(структЗадача["ИдЗадачи"], структЗадача);
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

                var ВремяЦикла = ТекущаяУниверсальнаяДатаВМиллисекундах() - НачалоЦикла;
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

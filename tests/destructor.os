Перем юТест;

Функция ПолучитьСписокТестов(ЮнитТестирование) Экспорт

	юТест = ЮнитТестирование;

	ВсеТесты = Новый Массив;

	ВсеТесты.Добавить("ТестДолжен_ОтработатьДеструктор");

КонецФункции

Процедура ТестДолжен_ОтработатьДеструктор() Экспорт

КонецПроцедуры

Процедура Деструктор() 
	Сообщить("~ from u.class");
КонецПроцедуры
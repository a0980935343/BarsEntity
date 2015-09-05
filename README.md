# BarsEntity
Создание нового Bars-объекта (включая Entity, Map, Controller, View и UpdateSchema, DomainService(Interceptor)) и многое, многое другое



Версия 1.4.3
----------

Метод ListSummary в контроллере для полей типа decimal, long или int


Версия 1.4.2.1
----------

В миграции создание таблицы методом AddTable или AddEntityTable в зависимости от базового класса


Версия 1.4.2
----------

Иерархический маппинг (SubclassMap по дискриминатору или BaseJoinedSubclassMap)


Версия 1.4.1
----------

Генератор Enum-типа для полей, если тип не найден в решении


Версия 1.4
----------

Генератор ViewModel для сущности (Get/List)


Версия 1.3.1
----------

Улучшения при генерации представления


Версия 1.3
----------

Генерация JavaScript для B4

Генераторы возвращают несколько файлов, а не один

Версия 1.2
----------

Перегрузка действия контроллера List/Get/Update/Delete/Create

Тип представления выбирается комбо-боксом из EAS/B4/ViewModel. B4 пока не
реализовано.

textProperty - для ссылочных полей поле можно выбрать из имеющихся у класса
(если он найден по имени) или вписать произвольное.

Enum - полю не базового типа можно указать, что оно является перечислением.
Генерируется не как ссылочное.

 

Версия 1.1
----------

Создание объектов копированием существующих в solution-е.

Для копирования надо:

1.  Открыть окно и подождать прогрузки типов решения (полминуты примерно)

2.  Набрать имя копируемого типа в поле "Имя класса" и нажать Enter

3.  Если классы с таким именем найдены, то в появившемся окне надо выбрать
    образец и нажать "Выбрать"

 

Версия 1.0
----------

Ура! Плагин доделан до релиза.

Позволяет создавать новый Bars-объект, а именно:

-   Entity;

-   Map;

-   Controller;

-   View (JavaScript or ViewModel)

-   UpdateSchema;

-   DomainService(Interceptor);

-   AuditLogMap;

-   и многое другое;

 

Файлы создавать в проекте не обязательно - можно копировать сгенерированный
текст по необходимости.
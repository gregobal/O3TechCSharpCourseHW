# Неделя 5: домашнее задание

## Перед тем как начать
- Как подготовить окружение [см. тут](./docs/01-prepare-environment.md)
- Как накатить миграции на базу данных [см. тут](./docs/02-data-migrations.md)
- **САМОЕ ВАЖНОЕ** - полное описание базы данных, схему и описание поле можно найти [тут](./docs/03-db-description.md)
- Воркшоп и примеры запросов [см. тут](https://gitlab.ozon.dev/cs/classroom-9/students/week-5/workshop-5/-/blob/master/README.md)

## Основные требования
- решением каждого задания является ОДИН SQL-запрос
- не допускается менять схему или сами данные, если этого явно не указано в задании
- поля в выборках должны иметь псевдоним (alias) указанный в задании
- решение необходимо привести в блоке каждой задачи ВМЕСТО комментария "ЗДЕСЬ ДОЛЖНО БЫТЬ РЕШЕНИЕ" (прямо в текущем readme.md файле)
- метки времени должны быть приведены в формат _dd.MM.yyyy HH:mm:ss_ (время в БД и выборках в UTC)

## Прочие пожелания
- всем будет удобно, если вы будете придерживаться единого стиля форматирования SQL-команд, как в [этом примере](./docs/04-sql-guidelines.md)

## Задание 1: 100 заданий с самым долгим временем выполнения
Время, затраченное на выполнение задания - это период времени, прошедший с момента перехода задания в статус "В работе" и до перехода в статус "Выполнено".
Нужно вывести 100 заданий с самым долгим временем выполнения. 
Полученный список заданий должен быть отсортирован от заданий с наибольшим временем выполнения к заданиям с наименьшим временем выполнения.

Замечания:
- Невыполненные задания (не дошедшие до статуса "Выполнено") не учитываются.
- Когда исполнитель берет задание в работу, оно переходит в статус "В работе" (InProgress) и находится там до завершения работы. После чего переходит в статус "Выполнено" (Done).
  В любой момент времени задание может быть безвозвратно отменено - в этом случае оно перейдет в статус "Отменено" (Canceled).
- Нет разницы выполняется задание или подзадание.
- Выборка должна включать задания за все время.

Выборка должна содержать следующий набор полей:
- номер задания (task_number)
- заголовок задания (task_title)
- название статуса задания (status_name)
- email автора задания (author_email)
- email текущего исполнителя (assignee_email)
- дата и время создания задания (created_at)
- дата и время первого перехода в статус В работе (in_progress_at)
- дата и время выполнения задания (completed_at)
- количество дней, часов, минут и секнуд, которые задание находилось в работе - в формате "dd HH:mm:ss" (work_duration)

### Решение
```sql
  with formats(datetime, duration)
    as (values('DD.MM.YYYY HH24:MI:SS', 'DD HH24:MI:SS'))
     , firsts_in_progress_at
    as (select task_id as task_id
             , min(at) as min_at
          from task_logs
         where status = 3 /* InProgress */
         group by task_id
             , at)
select t.number                                           as task_number
     , t.title                                            as task_title
     , ts.name                                            as status_name
     , uc.email                                           as author_email
     , ua.email                                           as assignee_email
     , to_char(t.created_at, ft.datetime)                 as created_at
     , to_char(fp.min_at, ft.datetime)                    as in_progress_at
     , to_char(t.completed_at, ft.datetime)               as completed_at
     , to_char((t.completed_at - fp.min_at), ft.duration) as work_duration
  from formats ft
     , tasks t
  join firsts_in_progress_at fp on fp.task_id = t.id
  join task_statuses ts on ts.id = t.status
  join users uc on uc.id = t.created_by_user_id
  join users ua on ua.id = t.assigned_to_user_id   
 where ts.id = 4 /* Done */
 order by work_duration desc
 limit 100;
```

## Задание 2: Выбора для проверки вложенности
Задания могу быть простыми и составными. Составное задание содержит в себе дочерние - так получается иерархия заданий.
Глубина иерархии ограничено Н-уровнями, поэтому перед добавлением подзадачи к текущей задачи нужно понять, может ли пользователь добавить задачу уровнем ниже текущего или нет. Для этого нужно написать выборку для метода проверки перед добавлением подзадания, которая бы вернула уровень вложенности указанного задания и полный путь до него от родительского задания.

Замечания:
- ИД проверяемого задания передаем в sql как параметр _:parent_task_id_
- если задание _Е_ находится на 5м уровне, то путь должен быть "_//A/B/C/D/E_".

Выбора должна содержать:
- только 1 строку
- поле "Уровень задания" (level) - уровень указанного в параметре задания
- поле "Путь" (path)

### Решение
```sql
  with recursive tasks_tree
    as (select t.id              as id
             , t.parent_task_id  as parent_task_id
             , 1                 as level
             , t.id::text as path
          from tasks t
         where t.id = :parent_task_id
         union all
        select t.id                         as id
             , t.parent_task_id             as parent_task_id
             , tt.level + 1                 as level
             , t.id::text || '/' || tt.path as path
          from tasks t
          join tasks_tree tt on tt.parent_task_id = t.id)
select tt.level        as level
     , '//' || tt.path as path
  from tasks_tree tt
 order by level desc
 limit 1;
```

## Задание 3 (за алмазик): Получить переписку по заданию в формате "вопрос-ответ"
По заданию могут быть оставлены комментарии от Автора и Исполнителя. Автор задания - это пользователь, создавший задание, Исполнитель - это актуальный пользователь, назначенный на задание.
У каждого комментария есть поле _author_user_id_ - это идентификатор пользователя, который оставил комментарий. Если этот идентикатор совпадает с идентификатором Автора задания, то сообщение должно отобразиться **в левой колонке**, следующее за ним сообщение-ответ Исполнителя (если _author_user_id_ равен ИД исполнителя) должно отобразиться **на той же строчке**, что и сообщение Автора, но **в правой колонке**. Считаем, что Автор задания задает Вопросы, а Исполнитель дает на них Ответы.
Выборка должны включать "беседы" по 5 самым новым зданям и быть отсортирвана по порядку отправки комментариев в рамках задания. 

Замечания:
- Актуальный исполнитель - это пользователь на момент выборки указан в поле assiged_to_user_id.
- Если вопроса или ответа нет, в соответствующем поле должен быть NULL.
- Считаем, что все сообщения были оставлены именно текущим исполнителем (без учета возможных переназначений).
- Если комментарий был оставлен не Автором и не Исполнителем, игнорируем его

Выборка должна содержать следующий набор полей:
- номер задания (task_number)
- email автора задания (author_email)
- email АКТУАЛЬНОГО исполнителя (assignee_email)
- вопрос (question)
- ответ (answer)
- метка времени, когда был задан вопрос (asked_at)
- метка времени, когда был дан ответ (answered_at)

<details>
  <summary>Пример</summary>

Переписка по заданию №1 между author@tt.ru и assgnee@tt.ru:
- 01.01.2023 08:00:00 (автор) "вопрос 1"
- 01.01.2023 09:00:00 (исполнитель) "ответ 1"
- 01.01.2023 09:15:00 (исполнитель) "ответ 2"
- 01.01.2023 09:30:00  (автор) "вопрос 2"

Ожидаемый результат выполнения SQL-запроса:

| task_number | author_email    | assignee_email | question  | answer  | asked_at             | answered_at          |
|-------------|-----------------|----------------|-----------|---------|----------------------|----------------------|
| 1           | author@tt.ru    | assgnee@tt.ru  | вопрос 1  | ответ 1 | 01.01.2023 08:00:00  | 01.01.2023 09:00:00  |
| 1           | author@tt.ru    | assgnee@tt.ru  | вопрос 1  | ответ 2 | 01.01.2023 08:00:00  | 01.01.2023 09:15:00  |
| 1           | author@tt.ru    | assgnee@tt.ru  | вопрос 2  |         | 01.01.2023 09:30:00  |                      |

</details>


### Решение
```sql
     with formats(datetime)
       as (values('DD.MM.YYYY HH24:MI:SS'))
        , task_ids_from_top_5_by_conversations
       as (select t.id as task_id               
             from tasks t
             join task_comments tc on tc.task_id = t.id
                                  and (tc.author_user_id = t.created_by_user_id
                                      or tc.author_user_id = t.assigned_to_user_id)
            group by t.id, t.created_at
            order by t.created_at desc
            limit 5)
        , top_5_newest_tasks_with_emails
       as (   select t.id                  as task_id
                   , t.number              as task_number
                   , t.created_by_user_id  as created_by_user_id
                   , uc.email              as author_email
                   , t.assigned_to_user_id as assigned_to_user_id
                   , ua.email              as assignee_email
                   , t.created_at          as created_at
                from task_ids_from_top_5_by_conversations ti
                join tasks t on t.id = ti.task_id
                join users uc on uc.id = t.created_by_user_id
           left join users ua on ua.id = t.assigned_to_user_id)
        , comments_by_top_tasks 
       as (select tt.task_id                                           as task_id
                , tc.at                                                as at
                , tc.message                                           as message
                , case when tc.author_user_id = tt.created_by_user_id 
                       then true /* question flag */
                       when tc.author_user_id = tt.assigned_to_user_id
                       then false /* answer flag */                      
                  end                                                  as is_question_or_answer
             from top_5_newest_tasks_with_emails tt
             join task_comments tc on tc.task_id = tt.task_id 
                                  and (tc.author_user_id = tt.created_by_user_id
                                       or tc.author_user_id = tt.assigned_to_user_id))
        , questions 
       as (select task_id                                                   as task_id                
                , at                                                        as asked_at
                , message                                                   as question                
                , lead(at) over (partition by task_id order by task_id, at) as next_question_at
             from comments_by_top_tasks 
            where is_question_or_answer is true)
        , answers 
       as (select task_id as task_id                
                , at      as answered_at
                , message as answer                
             from comments_by_top_tasks 
            where is_question_or_answer is false)
   select tt.task_number                      as task_number
        , tt.author_email                     as author_email        
        , tt.assignee_email                   as assignee_email       
        , q.question                          as question
        , a.answer                            as answer
        , to_char(q.asked_at, ft.datetime)    as asked_at
        , to_char(a.answered_at, ft.datetime) as answered_at
     from formats ft
        , questions q          
full join answers a on q.task_id = a.task_id 
                   and q.asked_at <= a.answered_at 
                   and (q.next_question_at is null or q.next_question_at > a.answered_at)
     join top_5_newest_tasks_with_emails tt on tt.task_id = coalesce(q.task_id, a.task_id)
    order by tt.created_at desc, coalesce(q.asked_at, a.answered_at), a.answered_at;
```

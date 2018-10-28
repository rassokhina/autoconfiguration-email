# autoconfiguration-email
 C# project for email autoconfiguration using Mozilla Thunderbird Autoconfiguration ISPDB and mx records
 
# Features
This simple library makes it easy for users to configure thier email addresses using only 3 fields: email, username and password.
The domain if the email address is used to determine autoconfiguration (IMAP and SMTP server names, ports, SSL yes/no, etc.), via 2 mechanisms:

- **Mozilla Thunderbird ISPBD files** - the fullest free database for email configurations.
It contains settings for the world's largest ISPs. Most ISPs with a market share of more than 0.1% are included. This allows to autoconfigure almost 50% of our user's email accounts.

- If the previous mechanism failed, the library begins to check **MX records in DNS**. If the record is found, 
we try to guess the configurations using 2 methods:
  1. Cut the domain like "hoster.com" from "mx1.mail.hoster.com" record and look for configuration files in Mozilla ISPDB again.
  2. Guess SMTP, IMAP port numbers for the domain found in DNS in 2.1. Try to connect with standart ports such as 465, 25, 110, 143
  and when a mail server answers, checking whether it supports SSL, STARTTLS and encrypted passwords. 

If guessing fails, the user should manually enter the configuration information.

All the lookup mechanisms use the email address domain as base for the lookup. For example, for the email address fred@example.com , the lookup is performed as (in this order):

- look up of "example.com" in the ISPDB
- look up "MX example.com" in DNS, and for mx1.mail.hoster.com, look up "hoster.com" in the ISPDB
- for mx1.mail.hoster.com try to guess IMAP, SMTP port and authentication type





**Autoconfiguration-email** - небольшая библиотека, используемая для автоконфигурации настроек email адресов. 

Автоконфигурация определяется 2 способами:

- ISPDB - это центральная база данных, которая в настоящий момент принадлежит коммерческой организации Mozilla Messaging, но может быть использована любым клиентом. Она содержит настройки для крупнейших почтовых провайдеров.
База расположена по адресу: https://autoconfig.thunderbird.net/v1.1/

  Чтобы воспользоваться ISPDB, нужно:
    1. подставить в URL базы имя домена, например, так https://autoconfig.thunderbird.net/v1.1/example.com и отправить запрос
    2. если нет ошибки, в теле ответа будет содержаться простой XML-файл определенного формата, который нужно распарсить и получить настройки.

- если настройки не найдены, то проверяем MX-записи в DNS. Если записи найдены, то пробуем использовать их для настройки аккаунта, используя для этого два способа:
    1. "Вырезаем" из записи типа "mx1.mail.hoster.com" домен "hoster.com" и снова ищем настройки в базе ISPDB.
    2. Для домена "hoster.com" берутся возможные стандартные номера портов: 465, 995, 993, 25, 110, 143, 587, 585 и пробуем 
    соединиться с сервером. Если соединились, то значит сервер использует для SMTP SSL, если не удалось соединиться, то берем следующий порт, снова пробуем соединиться и т.д. до тех пор пока не будут найдены настройки или же список адресов и портов не закончится.

Если оба способа не сработали, автоконфигурацию определить не удалось. 

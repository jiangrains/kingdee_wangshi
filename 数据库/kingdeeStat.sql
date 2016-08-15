create table page_visit_info
(
id int identity(1,1) primary key,
openid varchar(32) not null,
page varchar(32),
visit_time datetime,
)

create table button_visit_info
(
id int identity(1,1) primary key,
openid varchar(32) not null,
button varchar(32),
visit_time datetime,
)


create table access_token_info
(
id int identity(1,1) primary key,
access_token varchar(1024) not null,
expires_in int,
refresh_time datetime,
)

create table jsapi_ticket_info
(
id int identity(1,1) primary key,
jsapi_ticket varchar(1024) not null,
expires_in int,
refresh_time datetime,
)
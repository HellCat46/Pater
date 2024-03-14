create table Account
(
    id         int identity
        constraint PK_Account
            primary key,
    email      varchar(255)                                    not null,
    password   varchar(50),
    createdAt  datetime2 default '0001-01-01T00:00:00.0000000' not null,
    isAdmin    bit       default CONVERT([bit], 0)             not null,
    name       varchar(20)                                     not null,
    picPath    nvarchar(41),
    isVerified bit       default CONVERT([bit], 0)             not null,
    [plan]     int       default 0                             not null,
    linkLimit  int       default 0                             not null
)
go


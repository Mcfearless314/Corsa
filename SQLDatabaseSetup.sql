DROP TABLE IF EXISTS corsa.maps;
DROP TABLE IF EXISTS corsa.runs;
DROP TABLE IF EXISTS corsa.password_hash;
DROP TABLE IF EXISTS corsa.users;

DROP SCHEMA IF EXISTS Corsa;

CREATE SCHEMA Corsa;


create table corsa.users
(
    id       SERIAL PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    email    VARCHAR(50) NOT NULL UNIQUE
);

create table corsa.password_hash
(
    user_id   integer,
    hash      VARCHAR(350) NOT NULL,
    salt      VARCHAR(180) NOT NULL,
    algorithm VARCHAR(12)  NOT NULL,
    FOREIGN KEY (user_id) REFERENCES Corsa.users (id) ON DELETE CASCADE
);

CREATE TABLE corsa.runs
(
    runID      VARCHAR(20) PRIMARY KEY,
    user_id    integer   NOT NULL,
    startOfRun timestamp NOT NULL,
    endOfRun   timestamp,
    timeOfRun  TIME,
    distance   float,
    FOREIGN KEY (user_id) REFERENCES Corsa.users (id) ON DELETE CASCADE
);

CREATE TABLE corsa.maps
(
    mapID VARCHAR(20) NOT NULL,
    lat   float       NOT NULL,
    lng   float       NOT NULL,
    time  timestamp   NOT NULL,
    FOREIGN KEY (mapID) REFERENCES Corsa.runs (runID) ON DELETE CASCADE
);

SELECT *
FROM corsa.users;
SELECT *
FROM corsa.password_hash;
SELECT *
FROM corsa.runs;
SELECT *
FROM corsa.maps;

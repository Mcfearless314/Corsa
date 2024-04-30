DROP TABLE IF EXISTS Corsa.maps;
DROP TABLE IF EXISTS Corsa.runs;
DROP TABLE IF EXISTS Corsa.password_hash;
DROP TABLE IF EXISTS Corsa.users;

DROP SCHEMA IF EXISTS Corsa;

CREATE SCHEMA Corsa;


create table Corsa.users
(
    id        SERIAL        PRIMARY KEY,
    username  VARCHAR(50)   NOT NULL,
    email     VARCHAR(50)   NOT NULL UNIQUE
);

create table Corsa.password_hash
(
    user_id   integer,
    hash      VARCHAR(350) NOT NULL,
    salt      VARCHAR(180) NOT NULL,
    algorithm VARCHAR(12)  NOT NULL,
    FOREIGN KEY (user_id) REFERENCES Corsa.users (id)
);

CREATE TABLE Corsa.runs
(
    runID     SERIAL    PRIMARY KEY,
    user_id   integer   NOT NULL,
    timeOfRun timestamp NOT NULL,
    distance  float     NOT NULL,
    FOREIGN KEY (user_id) REFERENCES Corsa.users (id)
);

CREATE TABLE Corsa.maps
(
    mapID     SERIAL    NOT NULL,
    lat       float     NOT NULL,
    lng       float     NOT NULL,
    time      timestamp NOT NULL,
    FOREIGN KEY (mapID) REFERENCES Corsa.runs (runID)
);

SELECT * FROM Corsa.users;
SELECT * FROM Corsa.password_hash;
SELECT * FROM Corsa.runs;
SELECT * FROM Corsa.maps;

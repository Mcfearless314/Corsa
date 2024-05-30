DROP TABLE IF EXISTS corsa.devices;
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

CREATE TABLE corsa.devices
(
    deviceID VARCHAR(20) PRIMARY KEY,
    user_id  integer   NOT NULL,
    FOREIGN KEY (user_id) REFERENCES Corsa.users (id) ON DELETE CASCADE
);

--INSERT INTO corsa.users (username, email) VALUES ('Miran', 'Miran.Komadina@gmail.com');
--INSERT INTO corsa.password_hash (user_id, hash, salt, algorithm) VALUES 
--(1, 
--'5XRn1ssg1hrJan648Dum+6cy2YFQ/Rh0FM3WAFByR/Zt79OWmzBEfUUivn3nTKCt2PzquTOJLqHjL8hjBAfuYR01a1X65o+GHGZPZZukkGSCSoNkOSW0mpjtdktoZWoZmlg5QFAuMKxAMZg/DTyNDEjWwXpM7N+kSG83vMtn1YEgVeDh9AqQqrPJ9gXaLLnNHcnM864sTk02IY9+ynwjxZb1rn9l4BgapQ2UDVFtYhVMT1Z6ggz4bhQPi5Xw2VBVMf0uNRZ+eo3HcFRlCJVi1aSGRfpjPwK3+HQb9Abgy6pLBNKOtMMkhynSeHC0RC+q5NCodfYsj0auENTxmQ9pvg==',
-- 'CzRFEgITvo52HcZl+XM0qXK3a+kgxCAgbmA58XoB7tCNnZWayqlH+mlh49Ke1Rz3Wti58+psvyKdWqDlf2CkDSjA7vZSFN1T4OufPz22c+4uuC3IHZuHTGPWbvvgRSSb0g2znPBeVpcaYhGkqG+rGTD1egW9R+71sFXY9Wk6cJw=',
--'argon2id');
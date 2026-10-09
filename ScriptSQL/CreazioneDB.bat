@echo off

:: --- CONFIGURAZIONE VARIABILI ---
set "CONTAINER_NAME=db2-estrattoconto"
set "DB_NAME=conto"
set "DB_USER=db2admin"
set "DB_PASS=Esercizi_123"

echo Avvio del container IBM Db2...
docker run -d --name %CONTAINER_NAME% -p 50000:50000 --privileged=true -e LICENSE=accept -e DB2INSTANCE=%DB_USER% -e DB2INST1_PASSWORD=%DB_PASS% -e DBNAME=%DB_NAME% -e BLU=false icr.io/db2_community/db2

echo Caricamento del database IBM Db2 (Potrebbe richiedere alcuni minuti)...
:wait_db2_loop
curl -s http://localhost:50000 >nul 2>&1
if %ERRORLEVEL% EQU 7 (
    timeout /t 2 /nobreak >nul
    goto wait_db2_loop
)

timeout /t 180 /nobreak >nul

echo Creazione tabella LAVORATORI...
docker exec -i %CONTAINER_NAME% bash -c "export HOME=/database/config/%DB_USER% && su - %DB_USER% -c 'db2 connect to %DB_NAME% > /dev/null && db2 \"CREATE TABLE LAVORATORI (CF CHAR(16) NOT NULL PRIMARY KEY, COGNOME VARCHAR(100) NOT NULL, NOME VARCHAR(100) NOT NULL, DATA_NASCITA DATE NOT NULL)\"'"

echo Creazione tabella ESTRATTO_CONTO...
docker exec -i %CONTAINER_NAME% bash -c "export HOME=/database/config/%DB_USER% && su - %DB_USER% -c 'db2 connect to %DB_NAME% > /dev/null && db2 \"CREATE TABLE ESTRATTO_CONTO (ID INT GENERATED ALWAYS AS IDENTITY (START WITH 1, INCREMENT BY 1, NO CACHE) PRIMARY KEY, CF_LAVORATORE CHAR(16) NOT NULL, PERIODO_DAL DATE NOT NULL, PERIODO_AL DATE NOT NULL, TIPO_CONTRIBUZIONE VARCHAR(100) NOT NULL, SETTIMANE_UTILI INT NOT NULL, RETRIBUZIONE DECIMAL(31,2) NOT NULL, DATORE_LAVORO VARCHAR(150) NOT NULL, NOTE VARCHAR(255))\"'"

IF %ERRORLEVEL% EQU 0 (
    echo Tabelle dell'estratto conto create con successo!
) ELSE (
    echo Errore durante la creazione della tabella.
)

echo --------------------------------------------------

echo Avvio del container RabbitMQ dedicato...
docker run -d --name rabbitmq-estrattoconto -p 5672:5672 -p 15672:15672 rabbitmq:3-management

echo --------------------------------------------------

echo Avvio del container Oracle...
docker run -d --name oracle-logmonitor -p 1521:1521 -e ORACLE_PASSWORD=%DB_PASS% gvenzl/oracle-xe

echo Caricamento del database Oracle...
:wait_oracle_loop
docker exec oracle-logmonitor healthcheck.sh >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    timeout /t 2 /nobreak >nul
    goto wait_oracle_loop
)

echo Creazione tabella Oracle...
echo CREATE TABLE LOGMONITOR (ID NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY, OPERAZIONE VARCHAR2(50) NOT NULL, DATA_LOG TIMESTAMP NOT NULL, ID_RECORD NUMBER NOT NULL, CF_LAVORATORE CHAR(16) NOT NULL, DATORE_LAVORO VARCHAR2(150) NOT NULL, RETRIBUZIONE NUMBER(31,2) NOT NULL); | docker exec -i oracle-logmonitor sqlplus -s SYSTEM/%DB_PASS%@//localhost:1521/FREE

IF %ERRORLEVEL% EQU 0 (
    echo Ambiente configurato con successo! Le tabelle sono pronte.
) ELSE (
    echo Errore durante la configurazione finale.
)

pause
@echo off
echo Estrazione log da Oracle DB in corso...
echo --------------------------------------------------------------------------------

(
  echo SET LINESIZE 200
  echo SET PAGESIZE 100
  echo COLUMN OPERAZIONE FORMAT A12
  echo COLUMN CF_LAVORATORE FORMAT A17
  echo COLUMN DATORE_LAVORO FORMAT A20
  echo SELECT ID, OPERAZIONE, ID_RECORD, CF_LAVORATORE, DATORE_LAVORO, RETRIBUZIONE FROM LOGMONITOR;
) | docker exec -i oracle-logmonitor sqlplus -s SYSTEM/Esercizi_123@//localhost:1521/FREE

echo --------------------------------------------------------------------------------
pause
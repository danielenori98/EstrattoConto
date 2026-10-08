@echo off

echo Inserimento dati in corso...
echo INSERT INTO LAVORATORI (CF, COGNOME, NOME, DATA_NASCITA) VALUES ('C0D1C3F1SC4L3', 'CognomeProva', 'NomeProva', '1999-12-25'); | docker exec -i db2-estrattoconto bash -c "export HOME=/database/config/db2admin && su - db2admin -c 'db2 connect to conto > /dev/null && db2'" | findstr "DB20000I SQL"

echo INSERT INTO ESTRATTO_CONTO (CF_LAVORATORE, PERIODO_DAL, PERIODO_AL, TIPO_CONTRIBUZIONE, SETTIMANE_UTILI, RETRIBUZIONE, DATORE_LAVORO, NOTE) VALUES ('C0D1C3F1SC4L3', '2024-01-01', '2024-12-31', 'Lavoro dipendente', 52, 28500.00, 'Ditta 1', ''); | docker exec -i db2-estrattoconto bash -c "export HOME=/database/config/db2admin && su - db2admin -c 'db2 connect to conto > /dev/null && db2'" | findstr "DB20000I SQL"

echo INSERT INTO ESTRATTO_CONTO (CF_LAVORATORE, PERIODO_DAL, PERIODO_AL, TIPO_CONTRIBUZIONE, SETTIMANE_UTILI, RETRIBUZIONE, DATORE_LAVORO, NOTE) VALUES ('C0D1C3F1SC4L3', '2025-01-01', '2025-06-30', 'Lavoro dipendente', 26, 14200.00, 'Ditta 2', 'Nota 1'); | docker exec -i db2-estrattoconto bash -c "export HOME=/database/config/db2admin && su - db2admin -c 'db2 connect to conto > /dev/null && db2'" | findstr "DB20000I SQL"

echo INSERT INTO LAVORATORI (CF, COGNOME, NOME, DATA_NASCITA) VALUES ('CF_TEST_8888888', 'CognomeDue', 'NomeDue', '1995-05-15'); | docker exec -i db2-estrattoconto bash -c "export HOME=/database/config/db2admin && su - db2admin -c 'db2 connect to conto > /dev/null && db2'" | findstr "DB20000I SQL"

echo INSERT INTO ESTRATTO_CONTO (CF_LAVORATORE, PERIODO_DAL, PERIODO_AL, TIPO_CONTRIBUZIONE, SETTIMANE_UTILI, RETRIBUZIONE, DATORE_LAVORO, NOTE) VALUES ('CF_TEST_8888888', '2022-01-01', '2022-12-31', 'Lavoro dipendente', 52, 31000.00, 'Ditta 3', ''); | docker exec -i db2-estrattoconto bash -c "export HOME=/database/config/db2admin && su - db2admin -c 'db2 connect to conto > /dev/null && db2'" | findstr "DB20000I SQL"

echo INSERT INTO ESTRATTO_CONTO (CF_LAVORATORE, PERIODO_DAL, PERIODO_AL, TIPO_CONTRIBUZIONE, SETTIMANE_UTILI, RETRIBUZIONE, DATORE_LAVORO, NOTE) VALUES ('CF_TEST_8888888', '2023-01-01', '2023-08-15', 'Lavoro dipendente', 32, 19500.00, 'Ditta 4', 'Nota 2'); | docker exec -i db2-estrattoconto bash -c "export HOME=/database/config/db2admin && su - db2admin -c 'db2 connect to conto > /dev/null && db2'" | findstr "DB20000I SQL"

echo Inserimento completato!
pause
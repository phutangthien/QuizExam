## APPLICATION LIST URL ##
- Identity: https://localhost:5001
- Exam API: http://localhost:5002
- Exam Admin: http://localhost:6001
- Exam portal: http://localhost:6002

## Docker command run Databases

####            MONGODB         ######
### Create container MongoDB from Image
docker run -d  --name mongodb -e MONGO_INITDB_ROOT_USERNAME=admin -e MONGO_INITDB_ROOT_PASSWORD=admin_password  -p 27017:27017  mongo
### Run container mongodb
docker start mongodb
### Run shell container mongodb
docker exec -it mongodb mongosh "mongodb://admin:admin_password@localhost:27017"


####            SQL SERVER      ####
### Create container SQL Server from Image
 docker run -e 'ACCEPT_EULA=Y' -e "MSSQL_SA_PASSWORD=Phu@1234" -p 1433:1433 --name sql_server_container -d mcr.microsoft.com/mssql/server:2017-latest

### Run shell container sql server
docker exec -it sql_server_container /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "Phu@1234"
docker exec -it sql_server_container /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "Phu@1234" -Q "SELECT name FROM sys.databases"
docker exec -it sql_server_container /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "Phu@1234" -Q "SELECT name AS username FROM sys.database_principals WHERE type IN ('S', 'U', 'G')"


####            CREATE DOCKER COMPOSE           ####
docker-compose up -d


#### INIT GIT REPO ####

#### CREATE Solution ####


#### IMPLEMENT QUICKSTART UI FOR IDENTITY API ####
https://github.com/IdentityServer/IdentityServer4.Quickstart.UI
# MeterReadingsAPI

An ASP.NET Core based Web API that accepts CSV data containing meter readings, validates the data and loads it into a database, returning the number of successful/failed rows. It also ensures the creation of a database and seeds it with account data if none exist.

## Instructions
This has been tested on Windows and requires a local PostgreSQL installation, .NET 8.0 and Docker Desktop to run locally. If you want to run it outside of Docker, you may need to change the server in the connection string. You may also need to adjust the user ID, password and port.

## Testing
Contains integration and unit tests, but they don't run in Docker and the integration tests rely on a separate connection string, so changes made to the app's appsettings may need to be done here too. Would've set it up to use an environment variable but it's probably fine as-is for a quick, small project.

# ConfigurationRepository.VaultWithDbWebApp

This example shows how we can safely use configuration wich insecure part is stored in a database and secure part stored separatley in a vault. To combine these techincs into single application\`s configuration we use configuration parametrization.

This application uses `docker` and `docker-compose`:
- `hashicorp/vault` image for vault containder storing secrets.
- `mcr.microsoft.com/mssql/server:2022` image for MS SQL Server container storing app\`s configuration.
- image for container with application.

Main packages used:
- [VaultSharp](https://github.com/rajanadar/VaultSharp) to access Vault from application and initialize it\`s data.
- [VaultSharp.Extensions.Configuration](https://github.com/MrZoidberg/VaultSharp.Extensions.Configuration) to use Vault as configuration provider for application. This configuration stores only secrets that is used to build database connection string.
- [ConfigurationRepository.Dapper](https://github.com/TimurRybakov/ConfigurationRepository/tree/master/src/ConfigurationRepository.Dapper) to access MS SQL Server database that stores main application\`s configuration. The connection to this database is made with connection string built from vault secrets.
- [ParametrizedConfiguration](https://github.com/TimurRybakov/ConfigurationRepository/tree/master/src/ParametrizedConfiguration) to parametrize configurations with secret values and other parameters.
- [ReloadableConfiguration](https://github.com/TimurRybakov/ConfigurationRepository/tree/master/src/ReloadableConfiguration) to periodically reload configurations from their sources.

Application\`s endpoints:
- `GET /configuration` - returns current application\`s configuration. This can be used to check how configuration is being reloaded.
- `PUT /database`- updates database with new configuration values including configuration parameters. This can be used to check how configuration is parametrized after reload with `GET /configuration`.
- `PUT /vault`- updates vault with login and/or password. This can be used to see configuration changes with `GET /configuration` and changes with database access with `PUT /database` if login or password are invalid.

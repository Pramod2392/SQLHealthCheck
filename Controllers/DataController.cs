using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SQLHealthCheck.Models;

namespace SQLHealthCheck.Controllers;

[ApiController]
[Route("[controller]")]
public class DataController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly SecretClient _secretClient;

    public DataController(IConfiguration configuration)
    {
        this._configuration = configuration;
        this._secretClient = new SecretClient(new Uri(_configuration.GetValue<string>("AzureKeyVaultURI")),       
                                              new InteractiveBrowserCredential());
    }

    [HttpPost]
    [Route("AddEmployeeToSQLDBTable")]

    public async Task<string> AddEmployee(Employee employee)
    {
        try
        {
            string output = "";
            string sqlDBConnectionStringIdentifier = _configuration.GetValue<string>("SQLDBConnectionStringIdentifier");
            string sqlDBConnectionString = _secretClient.GetSecret(sqlDBConnectionStringIdentifier).Value.Value;

            System.Data.SqlClient.SqlConnection dbConnection = new(sqlDBConnectionString);

            System.Data.SqlClient.SqlCommand cmd = new();
            var firstNameSQLParameter = cmd.CreateParameter();
            firstNameSQLParameter.DbType = System.Data.DbType.String;
            firstNameSQLParameter.Direction = System.Data.ParameterDirection.Input;
            firstNameSQLParameter.ParameterName = "firstName";
            firstNameSQLParameter.Value = employee?.FirstName;

            var lastNameSQLParameter = cmd.CreateParameter();
            lastNameSQLParameter.DbType = System.Data.DbType.String;
            lastNameSQLParameter.Direction = System.Data.ParameterDirection.Input;
            lastNameSQLParameter.ParameterName = "lastName";
            lastNameSQLParameter.Value = employee?.LastName;

            cmd.Parameters.Add(firstNameSQLParameter);
            cmd.Parameters.Add(lastNameSQLParameter);

            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = "INSERT into Employee (FirstName, LastName) VALUES (@firstName, @lastName)";
            cmd.Connection = dbConnection;

            await dbConnection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await dbConnection.CloseAsync();

            output = "Successfully Added the employee to the employee table";

            return output;
        }
        catch (System.Exception)
        {
            throw;
        }
    }
    
}
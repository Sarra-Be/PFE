using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using stage_api.configuration;
using stage_api.NewFolder;
using System.Reflection;

using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Drawing;
using stage_api.Models;

namespace stage_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntityController : ControllerBase
    {
        private readonly DbContext _context;

        public EntityController(DbContext context)
        {
            _context = context;
        }

        private int GenerateRandomNumber()
        {
            Random rnd = new Random();
            return rnd.Next(1, 1000000); // Adjust the range as needed
        }

        private bool ValueExists(SqlConnection connection, string tableName, Dictionary<string, object> data)
        {
            // Construct a SQL command to check if the value exists
            string sqlCommand = $"SELECT COUNT(*) FROM {tableName} WHERE ";

            // Construct WHERE clause to check each key-value pair
            var whereClause = new List<string>();
            foreach (var pair in data)
            {
                whereClause.Add($"{pair.Key} = '{pair.Value}'");
            }
            sqlCommand += string.Join(" AND ", whereClause);

            // Execute the command to check if the value exists
            using (var command = new SqlCommand(sqlCommand, connection))
            {
                int count = (int)command.ExecuteScalar();
                return count > 0;
            }
        }

        [HttpPost]
        [Route("insertData/{tableName}")]
        public IActionResult InsertData(string? tableName, [FromBody] List<Dictionary<string, object>> dataList)
        {
            try
            {
                // Connection string to your database
                string connectionString = "Server=(localdb)\\mssqllocaldb;Database=stage_api.configuration;Trusted_Connection=True;MultipleActiveResultSets=true";

                using (var connection = new SqlConnection(connectionString))
                {
                    // Open the connection
                    connection.Open();

                    foreach (var data in dataList)
                    {

                        if (ValueExists(connection, tableName, data))
                        {
                            // Skip insertion if the value already exists
                            continue;
                        }
                        data.Remove("Id");
                        var id = GenerateRandomNumber();

                        // Add the Id to the dictionary
                        data.Add("Id", id);

                        var values = data.Values.Select(value => $"'{value}'");


                        // Construct SQL command to insert data into the specified table
                        string sqlCommand = $"INSERT INTO {tableName} ({string.Join(",", data.Keys)}) VALUES ({string.Join(",", values)})";

                        // Create and execute the command
                        using (var command = new SqlCommand(sqlCommand, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet]
        [Route("GetTables")]
        public IActionResult getTables()
        {
            var modelInfoList = new List<ModelInfo>();

            try
            {
                // Connection string to your database
                string connectionString = "Server=(localdb)\\mssqllocaldb;Database=stage_api.configuration;Trusted_Connection=True;MultipleActiveResultSets=true";

                using (var connection = new SqlConnection(connectionString))
                {
                    // Open the connection
                    connection.Open();

                    // Get schema information about tables
                    DataTable tablesSchema = connection.GetSchema("Tables");

                    // Iterate through each row in the schema
                    foreach (DataRow row in tablesSchema.Rows)
                    {
                        // Get table name from the schema
                        string tableName = row["TABLE_NAME"].ToString();

                        // Construct model info object for table
                        var modelInfo = new ModelInfo
                        {
                            Name = tableName,
                            Attributes = new List<string>()
                        };

                        // Get column schema for the table
                        DataTable columnsSchema = connection.GetSchema("Columns", new[] { null, null, tableName });

                        // Iterate through each column in the table
                        foreach (DataRow columnRow in columnsSchema.Rows)
                        {
                            // Get column name from the schema
                            string columnName = columnRow["COLUMN_NAME"].ToString();

                            // Add column name to the attributes list
                            modelInfo.Attributes.Add(columnName);
                        }

                        // Add model information to the list
                        modelInfoList.Add(modelInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Internal Server Error");
            }

            return Ok(modelInfoList);
        }


        [HttpGet]
        [Route("getEntities")]
        public IActionResult GetModelInfo()
        {
            // Get all loaded assemblies in the current application domain
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // List to store model information
            var modelInfoList = new List<ModelInfo>();

            // Namespace containing your models
            string modelNamespace = "stage_api.Models";

            // Iterate through each assembly
            foreach (var assembly in assemblies)
            {
                try
                {
                    // Get all types defined in the assembly
                    var types = assembly.GetTypes();

                    // Filter types that are in the model namespace and are not ModelInfo
                    var modelTypes = types.Where(t => t.Namespace == modelNamespace && t.IsClass && t != typeof(ModelInfo));

                    // Iterate through model types
                    foreach (var modelType in modelTypes)
                    {
                        // Create model information object
                        var modelInfo = new ModelInfo
                        {
                            Name = modelType.Name,
                            Attributes = new List<string>() // Initialize the list
                        };

                        // Get properties of the model type
                        foreach (var prop in modelType.GetProperties())
                        {
                            // Add property name to the attributes list
                            modelInfo.Attributes.Add(prop.Name);
                        }

                        // Add model information to the list
                        modelInfoList.Add(modelInfo);
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    // Handle exceptions when trying to load types from the assembly
                    foreach (var loaderException in ex.LoaderExceptions)
                    {
                        Console.WriteLine(loaderException.Message);
                    }
                }
            }

            return Ok(modelInfoList);
        }

        [HttpPost("CreateTable")]
        public async Task<IActionResult> CreateTable([FromBody] CreateTableRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Build SQL command dynamically using request parameters
                string sqlCommand = $"CREATE TABLE {request.TableName} (Id INT PRIMARY KEY";

                foreach (var attribute in request.Attributes)
                {
                    sqlCommand += $", {attribute.Name} {attribute.DataType}";
                }

                sqlCommand += ")";

                // Execute the dynamically generated SQL command
                await _context.Database.ExecuteSqlRawAsync(sqlCommand);

                var performedBy = _context.Users.FirstOrDefault(u => u.Id == request.CreatedById);
                var log = new ActionLog
                {
                    Name = "Table creation",
                    PerformedBy = performedBy,
                    CreatedAt = DateTime.Now,
                };
                _context.ActionLogs.Add(log);
                _context.SaveChanges();

                var message = "Table created successfully!";
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                // Handle exceptions
                var message = ex.Message;
                return StatusCode(500, new { message });
            }
        }
    }
}

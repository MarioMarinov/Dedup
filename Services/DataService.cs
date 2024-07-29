using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Serilog;
using Services.Extensions;
using Services.Models;
using Shipwreck.Phash;


namespace Services
{
    public class DataService : IDataService
    {
        readonly string _dbFileName;
        readonly string _connectionString;
        private static readonly object _lock = new object();

        /// <summary>
        /// Create or empty specific tables in the database
        /// </summary>
        /// <returns></returns>
        public async Task CreateTablesAsync()
        {
            await InitTablesAsync(["Images"]);
        }

        /// <summary>
        /// Delete image data
        /// </summary>
        /// <param name="imageData"></param>
        /// <returns></returns>
        public async Task<int> DeleteImageDataAsync(List<ImageModel> imageData)
        {
            var rowsDeleted = 0;
            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transact = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        var cmd = connection.CreateCommand();
                        cmd.CommandText = """
                    DELETE FROM Images
                    WHERE FileName = $FileName AND RelativePath = $RelativePath;
                    """;

                        var fileNameParam = cmd.CreateParameter();
                        fileNameParam.ParameterName = "$FileName";
                        cmd.Parameters.Add(fileNameParam);

                        var relativePathParam = cmd.CreateParameter();
                        relativePathParam.ParameterName = "$RelativePath";
                        cmd.Parameters.Add(relativePathParam);

                        foreach (var model in imageData)
                        {
                            fileNameParam.Value = model.FileName;
                            relativePathParam.Value = model.RelativePath;

                            rowsDeleted += cmd.ExecuteNonQuery();
                            Log.Information($"Deleted {Path.Combine(model.RelativePath,model.FileName)}");
                        }
                        transact.Commit();
                    }
                    catch
                    {
                        Log.Error("Delete transaction failed, rolling back");
                        transact.Rollback();
                        throw;
                    }
                    return rowsDeleted;
                }
            }
        }

        /// <summary>
        /// Get all images data
        /// </summary>
        /// <returns>Image models</returns>
        public async Task<List<ImageModel>> SelectImageDataAsync()
        {
            var res = new List<ImageModel>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT * FROM Images ORDER BY RelativePath, FileName";
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        string fileName = reader.GetString(0);
                        string? imageHash = reader.GetSafeFieldValue<string?>(1);
                        long length = reader.GetInt64(2);
                        string relativePath = reader.GetString(3);
                        var model = new ImageModel(fileName, relativePath, length, imageHash);
                        
                        res.Add(model);
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Add image data
        /// </summary>
        /// <param name="imageData"></param>
        /// <returns></returns>
        public async Task InsertImageDataAsync(List<ImageModel> imageData)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transact = await connection.BeginTransactionAsync())
                {
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = """
                    INSERT INTO Images (FileName, ImageHash, Length, RelativePath)
                    VALUES ($FileName, $ImageHash, $Length, $RelativePath);
                    """;

                    var fileNameParam = cmd.CreateParameter();
                    fileNameParam.ParameterName = "$FileName";
                    cmd.Parameters.Add(fileNameParam);

                    var imageHashParam = cmd.CreateParameter();
                    imageHashParam.IsNullable = true;
                    imageHashParam.ParameterName = "$ImageHash";
                    cmd.Parameters.Add(imageHashParam);

                    var lengthParam = cmd.CreateParameter();
                    lengthParam.ParameterName = "$Length";
                    cmd.Parameters.Add(lengthParam);

                    var relativePathParam = cmd.CreateParameter();
                    relativePathParam.ParameterName = "$RelativePath";
                    cmd.Parameters.Add(relativePathParam);
                    
                    var nullValue = (object)DBNull.Value;
                    foreach (var model in imageData)
                    {
                        fileNameParam.Value = model.FileName;
                        imageHashParam.Value = model.ImageHash ?? nullValue;
                        lengthParam.Value = model.Length;
                        relativePathParam.Value = model.RelativePath;

                        cmd.ExecuteNonQuery();
                    }

                    transact.Commit();
                }
            }
        }


        /// <summary>
        /// Create tables or empty them if they're already created
        /// </summary>
        /// <param name="tableNames">List of tables to be initialized</param>
        /// <returns></returns>
        public async Task InitTablesAsync(string[] tableNames)
        {
            if (tableNames.Contains("Images"))
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = """
                CREATE TABLE IF NOT EXISTS Images (
                    FileName TEXT NOT NULL,
                    ImageHash TEXT,
                    Length INTEGER NOT NULL,
                    RelativePath TEXT NOT NULL,
                    UNIQUE (FileName, RelativePath)
                );
                DELETE FROM Images;
                """;
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public DataService(IOptions<AppSettings> settings)
        {
            _dbFileName = settings.Value.DbFilePath;
            _connectionString = $"Data Source = {_dbFileName}";
            if (!File.Exists(_dbFileName)) { 
                Task.Run(CreateTablesAsync);
            }
        }
    }
}

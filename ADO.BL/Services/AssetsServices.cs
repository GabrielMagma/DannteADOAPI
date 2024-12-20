
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Globalization;
using System.Text;

namespace ADO.BL.Services
{
    public class AssetsServices : IAssetsServices
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly string _assetsDirectoryPath;
        private readonly string[] _timeFormats;

        public AssetsServices(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("PgDbConnection");
            _assetsDirectoryPath = configuration["AssetsDirectoryPath"];
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
        }

        public ResponseQuery<List<string>> ReadAssets(ResponseQuery<List<string>> response)
        {
            try
            {
                var files = Directory.GetFiles(_assetsDirectoryPath, "*_assets.csv");
                var errors = new List<string>();

                foreach (var filePath in files)
                {
                    var fileErrors = UpsertAssets(filePath);
                    if (fileErrors.Any())
                    {
                        errors.AddRange(fileErrors);
                    }
                    Console.WriteLine($"Archivo {filePath} procesado.");
                }

                response.Message = "All Registers are created and/or updated";
                response.SuccessData = true;
                response.Success = true;
                return response;

            }
            //catch (SqliteException ex)
            //{
            //    response.Message = ex.Message;
            //    response.Success = false;
            //    response.SuccessData = false;
            //}
            catch (FormatException ex)
            {
                response.Message = ex.Message;
                response.Success = false;
                response.SuccessData = false;
            }
            catch (Exception ex)
            {                
                response.Message = ex.Message;
                response.Success = false;
                response.SuccessData = false;
            }
                       
            return response;
        }

        private DateTime ParseDate(string dateString)
        {
            foreach (var format in _timeFormats)
            {
                if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    return parsedDate;
                }
            }
            throw new FormatException($"El formato de fecha {dateString} no es válido.");
        }

        private List<string> UpsertAssets(string filePath)
        {
            var errors = new List<string>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                // Crear tabla temporal
                string tempTableQuery = @"
                    CREATE TEMP TABLE temp_all_asset AS 
                    SELECT * FROM public.all_asset LIMIT 0;
                ";

                using (var tempTableCmd = new NpgsqlCommand(tempTableQuery, connection))
                {
                    tempTableCmd.ExecuteNonQuery();
                }

                // Usar COPY para cargar datos en la tabla temporal
                using (var writer = connection.BeginBinaryImport(
                     $"COPY temp_all_asset(type_asset, code_sig, uia, codetaxo, fparent, latitude, longitude, poblation, " +
                     $"group015, uccap14, date_inst, date_unin, state, id_zone, name_zone, id_region, name_region, id_locality, name_locality, " +
                     $"id_sector, name_sector, geographical_code, address) " +
                     $"FROM STDIN (FORMAT BINARY)"
                 ))
                {
                    var existingKeys = new HashSet<string>();
                    using (var reader = new StreamReader(filePath, Encoding.UTF8, true))
                    {
                        int lineNumber = 0;
                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            lineNumber++;
                            var values = line.Split(new char[] { ',', ';' });
                            var key = $"{values[1]}-{values[2]}"; // Combinar code_sig y uia como clave única.

                            if (!existingKeys.Add(key))
                            {
                                errors.Add($"Registro duplicado ignorado en línea {lineNumber}: {key}");
                                continue;
                            }

                            try
                            {
                                writer.StartRow();
                                writer.Write(values[0], NpgsqlTypes.NpgsqlDbType.Varchar); // type_asset
                                writer.Write(values[1], NpgsqlTypes.NpgsqlDbType.Varchar); // code_sig
                                writer.Write(values[2], NpgsqlTypes.NpgsqlDbType.Varchar); // uia
                                writer.Write(values[3], NpgsqlTypes.NpgsqlDbType.Varchar); // codetaxo
                                writer.Write(values[4], NpgsqlTypes.NpgsqlDbType.Varchar); // fparent
                                writer.Write(float.Parse(values[5]), NpgsqlTypes.NpgsqlDbType.Real); // latitude
                                writer.Write(float.Parse(values[6]), NpgsqlTypes.NpgsqlDbType.Real); // longitude
                                writer.Write(values[7], NpgsqlTypes.NpgsqlDbType.Varchar); // poblation
                                writer.Write(values[8], NpgsqlTypes.NpgsqlDbType.Varchar); // group015
                                writer.Write(values[9], NpgsqlTypes.NpgsqlDbType.Varchar); // uccap14
                                writer.Write(string.IsNullOrEmpty(values[10]) ? (DateTime?)null : ParseDate(values[10]), NpgsqlTypes.NpgsqlDbType.Date); // date_inst
                                writer.Write(string.IsNullOrEmpty(values[11]) ? new DateTime(2099, 12, 31) : ParseDate(values[11]), NpgsqlTypes.NpgsqlDbType.Date); // date_unin
                                writer.Write(string.IsNullOrEmpty(values[12]) ? 2 : int.Parse(values[12]), NpgsqlTypes.NpgsqlDbType.Integer); // state
                                writer.Write(string.IsNullOrEmpty(values[13]) ? (long?)null : long.Parse(values[13]), NpgsqlTypes.NpgsqlDbType.Bigint); // id_zone
                                writer.Write(values[14], NpgsqlTypes.NpgsqlDbType.Varchar); // name_zone
                                writer.Write(string.IsNullOrEmpty(values[15]) ? (long?)null : long.Parse(values[15]), NpgsqlTypes.NpgsqlDbType.Bigint); // id_region
                                writer.Write(values[16], NpgsqlTypes.NpgsqlDbType.Varchar); // name_region
                                writer.Write(string.IsNullOrEmpty(values[17]) ? (long?)null : long.Parse(values[17]), NpgsqlTypes.NpgsqlDbType.Bigint); // id_locality
                                writer.Write(values[18], NpgsqlTypes.NpgsqlDbType.Varchar); // name_locality
                                writer.Write(string.IsNullOrEmpty(values[19]) ? (long?)null : long.Parse(values[19]), NpgsqlTypes.NpgsqlDbType.Bigint); // id_sector
                                writer.Write(values[20], NpgsqlTypes.NpgsqlDbType.Varchar); // name_sector
                                writer.Write(string.IsNullOrEmpty(values[21]) ? (long?)null : long.Parse(values[21]), NpgsqlTypes.NpgsqlDbType.Bigint); // geographical_code
                                writer.Write(values[22], NpgsqlTypes.NpgsqlDbType.Varchar); // address
                            }
                            catch (Exception ex)
                            {
                                errors.Add($"Error procesando el archivo {filePath} en la línea {lineNumber}.\n" +
                                    $"Datos del registro: {string.Join(", ", values)}\nError: {ex.Message}");
                            }
                        }
                    }

                    writer.Complete();
                }

                // Realizar el UPSERT desde la tabla temporal a la tabla definitiva
                string upsertQuery = @"
                    INSERT INTO public.all_asset(type_asset, code_sig, uia, codetaxo, fparent, latitude, longitude, poblation, 
                        group015, uccap14, date_inst, date_unin, state, id_zone, name_zone, id_region, name_region, id_locality, name_locality, 
                        id_sector, name_sector, geographical_code, address) 
                    SELECT type_asset, code_sig, uia, codetaxo, fparent, latitude, longitude, poblation, 
                        group015, uccap14, date_inst, date_unin, state, id_zone, name_zone, id_region, name_region, id_locality, name_locality, 
                        id_sector, name_sector, geographical_code, address
                    FROM temp_all_asset
                    ON CONFLICT (code_sig, uia) 
                    DO UPDATE 
                    SET 
                        date_unin = CASE WHEN EXCLUDED.state = 3 THEN EXCLUDED.date_unin ELSE all_asset.date_unin END,
                        state = CASE WHEN EXCLUDED.state = 3 THEN EXCLUDED.state ELSE all_asset.state END
                    WHERE all_asset.state != 3;
                ";

                using (var upsertCmd = new NpgsqlCommand(upsertQuery, connection))
                {
                    upsertCmd.ExecuteNonQuery();
                }

                // Eliminar la tabla temporal
                string dropTempTableQuery = "DROP TABLE IF EXISTS temp_all_asset;";

                using (var dropTempTableCmd = new NpgsqlCommand(dropTempTableQuery, connection))
                {
                    dropTempTableCmd.ExecuteNonQuery();
                }
            }

            return errors;
        }
    }
}

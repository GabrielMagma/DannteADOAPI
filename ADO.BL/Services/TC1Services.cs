﻿using ADO.BL.Interfaces;
using ADO.BL.Responses;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Globalization;
using System.Text;

namespace ADO.BL.Services
{
    public class TC1Services : ITC1Services
    {
        private readonly string _connectionString;
        private readonly string _assetsDirectoryPath;
        private readonly string[] _timeFormats;

        public TC1Services(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("PgDbConnection");
            _assetsDirectoryPath = configuration["Tc1DirectoryPath"];
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
        }

        public ResponseQuery<List<string>> ReadAssets(ResponseQuery<List<string>> response)
        {
            try
            {
                var files = Directory.GetFiles(_assetsDirectoryPath, "*_TC1.csv");  // OJO TOCA ESTANDARIZAR!!!

                foreach (var filePath in files)
                {
                    InsertAssets(filePath);
                    Console.WriteLine($"Archivo {filePath} subido exitosamente.");
                }

                response.Message = "Proceso completado para todos los archivos";
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

        private async Task InsertAssets(string filePath)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Extraer el nombre del archivo sin la extensión
                var fileName = Path.GetFileNameWithoutExtension(filePath);

                // Asumiendo que el formato del archivo es YYYYMM_TC1.csv
                if (fileName.Length >= 6)
                {
                    // Obtener los primeros 4 dígitos como el año
                    int year = int.Parse(fileName.Substring(0, 4));

                    // Obtener los siguientes 2 dígitos como el mes
                    int month = int.Parse(fileName.Substring(4, 2));

                    // Usar COPY para cargar datos directamente en la tabla definitiva
                    using (var writer = connection.BeginBinaryImport(
                         $"COPY public.files_tc1(niu, uia, year, month, files, files_date) " +
                         $"FROM STDIN (FORMAT BINARY)"
                     ))
                    {
                        using (var reader = new StreamReader(filePath, Encoding.UTF8, true))
                        {
                            int lineNumber = 0;
                            while (!reader.EndOfStream)
                            {
                                var line = await reader.ReadLineAsync();
                                lineNumber++;
                                var values = line.Split(new char[] { ',', ';' });

                                try
                                {
                                    // Iniciar una nueva fila
                                    writer.StartRow();
                                    writer.Write(values[0], NpgsqlTypes.NpgsqlDbType.Varchar); // Niu (de la primera columna del archivo)
                                    writer.Write(values[1], NpgsqlTypes.NpgsqlDbType.Varchar); // Uia (de la segunda columna del archivo)
                                    writer.Write(year, NpgsqlTypes.NpgsqlDbType.Integer); // Year (extraído del nombre del archivo)
                                    writer.Write(month, NpgsqlTypes.NpgsqlDbType.Integer); // Month (extraído del nombre del archivo)
                                    writer.Write(Path.GetFileName(filePath), NpgsqlTypes.NpgsqlDbType.Varchar); // Files (nombre del archivo)
                                    writer.Write(DateOnly.FromDateTime(DateTime.Today), NpgsqlTypes.NpgsqlDbType.Date); // FilesDate (fecha actual)
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception($"Error procesando el archivo {filePath} en la línea {lineNumber}: {ex.Message}");
                                }
                            }
                        }

                        // Finalizar la escritura
                        writer.Complete();
                    }
                }
                else
                {
                    throw new Exception("Formato de nombre de archivo no válido. Debe ser YYYYMM_TC1.csv");
                }
            }
        }
    }
}
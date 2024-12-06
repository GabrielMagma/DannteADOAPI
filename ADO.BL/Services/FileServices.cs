using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using CsvHelper;
using System.Globalization;
using System.Text;

namespace ADO.BL.Services
{
    public class FileServices : IFileServices
    {
        private readonly IFileDataAccess fileDataAccess;
        private readonly IMapper mapper;
        public FileServices(IFileDataAccess _fileDataAccess, IMapper _mapper)
        {
            fileDataAccess = _fileDataAccess;
            mapper = _mapper;
        }
        public ResponseQuery<string> CreateFileCSV(string name, ResponseQuery<string> response)
        {
            try
            {   
                                
                List<Ideam> ideamListData = new List<Ideam>();
                string[] fileLines = File.ReadAllLines($"./files/{name}.csv");
                List<Register> valueFinal = new List<Register>();
                Register register = new Register();
                string routeFile = $".\\files\\{name}{DateTime.Now.ToString("yyyy-MM-dd")}.csv";
                TextWriter newFile = new StreamWriter(routeFile);
                newFile.Close();
                
                using (var writer = new StreamWriter(new FileStream(routeFile, FileMode.Open), Encoding.UTF8))
                {
                    using (var csvWriter = new CsvWriter(writer, CultureInfo.CurrentCulture))
                    {

                        foreach (var item in fileLines.Skip(1))
                        {
                            IdeamDTO ideam = new IdeamDTO();
                            var valueLines = item.Split(",");

                            ideam.Id = 0;

                            register.CodigoEstacion = int.Parse(valueLines[0]);
                            ideam.Stationcode = valueLines[0];

                            register.NombreEstacion = valueLines[1];
                            ideam.Stationname = valueLines[1];

                            register.Latitud = valueLines[2];
                            ideam.Latitude = Double.Parse(valueLines[2]);

                            register.Longitud = valueLines[3];
                            ideam.Longitude = Double.Parse(valueLines[3]);

                            register.Altitud = int.Parse(valueLines[4]);
                            ideam.Altitude = Double.Parse(valueLines[4]);

                            register.Departamento = valueLines[8];
                            ideam.Department = valueLines[8];

                            register.Municipio = valueLines[9];
                            ideam.Municipality = valueLines[9];

                            register.IdParametro = valueLines[12];
                            ideam.Parameterid = valueLines[12];

                            register.Frecuencia = valueLines[15];
                            ideam.Frequency = valueLines[15];

                            var date = ParseDate(valueLines[16]);
                            
                            register.Fecha = date.ToString();
                            ideam.Date = date;

                            register.Valor = double.Parse(valueLines[17]);
                            ideam.Precipitation = double.Parse(valueLines[18]);

                            valueFinal.Add(register);
                            var ideamMapped = mapper.Map<Ideam>(ideam);
                            ideamListData.Add(ideamMapped);
                            

                            register = new Register();
                        }
                        csvWriter.WriteRecords(valueFinal);
                        response.SuccessData = fileDataAccess.CreateFile(ideamListData);
                    }

                }

                response.Message = "File created on the project root ./files";
                response.Success = true;
                return response;

            }
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

        private DateOnly ParseDate(string dateString)
        {
            var _timeFormats = new List<string> {
                    "yyyy-MM-dd HH:mm",
                    "dd-MM-yyyy HH:mm",
                    "yyyy/MM/dd HH:mm",
                    "dd/MM/yyyy HH:mm",
                };
            foreach (var format in _timeFormats)
            {
                if (DateOnly.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly parsedDate))
                {
                    return parsedDate;
                }
            }
            throw new FormatException($"El formato de fecha {dateString} no es válido.");
        }

    }
}

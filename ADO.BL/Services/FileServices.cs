using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using CsvHelper;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.Globalization;
using System.Text;

namespace ADO.BL.Services
{
    public class FileServices : IFileServices
    {
        private readonly IFileDataAccess fileDataAccess;
        private readonly IMapper mapper;
        private readonly string[] _timeFormats;
        public FileServices(IConfiguration configuration, IFileDataAccess _fileDataAccess, IMapper _mapper)
        {
            fileDataAccess = _fileDataAccess;
            mapper = _mapper;
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
        }

        public ResponseQuery<string> CreateFileCSV(string name, ResponseQuery<string> response)
        {
            try
            {   
                                
                List<IaIdeam> ideamListData = new List<IaIdeam>();            
                string[] fileLines = File.ReadAllLines($"C:\\Users\\ingen\\source\\repos\\DannteADOAPI\\files\\Ideam\\{name}.csv");
                List<Register> valueFinal = new List<Register>();
                Register register = new Register();
                string routeFile = $"C:\\Users\\ingen\\source\\repos\\DannteADOAPI\\files\\Ideam\\{name}{DateTime.Now.ToString("yyyy-MM-dd")}.csv";
                TextWriter newFile = new StreamWriter(routeFile);
                newFile.Close();
                string[] fileLinesComp = File.ReadAllLines($"C:\\Users\\ingen\\source\\repos\\DannteADOAPI\\files\\IdeamComp\\ideamComp.csv");
                var ideamCompList = new List<IdeamCompDTO>();
                foreach (string line in fileLinesComp) {
                    var valueLines = line.Split(",");
                    var ideamElement = new IdeamCompDTO();
                    ideamElement.Codigo = valueLines[0];
                    ideamElement.Latitud = valueLines[8];
                    ideamElement.Longitud = valueLines[9];
                    ideamElement.Altitud = valueLines[7];
                    ideamElement.Departamento = valueLines[10];
                    ideamElement.Municipio = valueLines[11];
                    ideamCompList.Add( ideamElement );
                }


                using (var writer = new StreamWriter(new FileStream(routeFile, FileMode.Open), Encoding.UTF8))
                {
                    using (var csvWriter = new CsvWriter(writer, CultureInfo.CurrentCulture))
                    {

                        foreach (var item in fileLines)
                        {
                            IdeamDTO ideam = new IdeamDTO();
                            var valueLines = item.Split(",");

                            if (valueLines[0] != "CodigoEstacion")
                            {
                                ideam.Id = 0;

                                var IdeamTemp = ideamCompList.Find(i => i.Codigo == valueLines[0]);

                                register.CodigoEstacion = int.Parse(valueLines[0]);
                                ideam.Stationcode = valueLines[0];

                                register.NombreEstacion = valueLines[1];
                                ideam.Stationname = valueLines[1];

                                register.Latitud = IdeamTemp.Latitud.ToString();
                                ideam.Latitude = Double.Parse(IdeamTemp.Latitud.ToString());

                                register.Longitud = IdeamTemp.Longitud.ToString();
                                ideam.Longitude = Double.Parse(IdeamTemp.Longitud.ToString());

                                register.Altitud = int.Parse(IdeamTemp.Altitud.ToString());
                                ideam.Altitude = Double.Parse(IdeamTemp.Altitud.ToString());

                                register.Departamento = IdeamTemp.Departamento.ToString();
                                ideam.Department = IdeamTemp.Departamento.ToString();

                                register.Municipio = IdeamTemp.Municipio.ToString();
                                ideam.Municipality = IdeamTemp.Municipio.ToString();

                                register.IdParametro = "PRECIPITACION";
                                ideam.Parameterid = "PRECIPITACION";

                                register.Frecuencia = "DIARIA";
                                ideam.Frequency = "DIARIA";

                                var date = ParseDate(valueLines[4]);

                                register.Fecha = date.ToString();
                                ideam.Date = date;

                                register.Valor = double.Parse(valueLines[6]);
                                ideam.Precipitation = double.Parse(valueLines[6]);

                                valueFinal.Add(register);
                                var ideamMapped = mapper.Map<IaIdeam>(ideam);
                                ideamListData.Add(ideamMapped);


                                register = new Register();
                            }
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
            foreach (var format in _timeFormats)
            {
                if (DateOnly.TryParseExact(dateString, format.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly parsedDate))
                {
                    return parsedDate;
                }
            }
            return DateOnly.Parse("31/12/2099");
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft;
using ExcelDataReader;
using System.Data;
using System.Collections;
using Tesseract;
using System.Net.WebSockets;
using SixLabors.ImageSharp;
using Org.BouncyCastle.Asn1;
using Newtonsoft.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Drawing;
using Services.OCRService;
using System.Diagnostics.Contracts;
using Services.ContractorsService;

namespace Services.FilesService
{
    public class FilesReaderService : IFilesReaderService
    {
        public FileReaderConfiguration Configuration { get; }
        public Dictionary<string, string> ContractorList { get; private set; } = new Dictionary<string, string>();
        public IEnumerable<string> GetContractorNames => ContractorList.Values;

        public FilesReaderService(FileReaderConfiguration? configuration = null)
        {

            Configuration = new FileReaderConfiguration()
            {
                SearchPrefix ="Scan",
                ImportExtFilter = "*.xlsb | *.xlsx | *.xls | *.csv",
                ExportExtFilter = "*.pdf",
                SaveDir = "./",
                DataFileExtFilter = "*.pdf | *.jpg | *.jpeg | *.bmp | *.png | *.tiff",
                SearchDir = @"C:\Users\Kubiaxx\Documents\Programowanie\PRACA\Koszty Listopad"

            };
            
            if (configuration != null)
                Configuration = configuration;



            var contractorData = GetContractorsRawData();
            
            if(contractorData != string.Empty)
            {
                var text = JsonConvert.DeserializeObject<ContractorList>(contractorData);
                ContractorsService.ContractorsService.SetNewList = text;
            }

        }

        public IEnumerable<FileInfo> CreateFileList()
        {
            var allFiles = Directory.EnumerateFiles(Configuration.SearchDir, "*.*", SearchOption.TopDirectoryOnly)
                .Where(f => Configuration.DataFileExtFilter.Contains(Path.GetExtension(f)));

            List<FileInfo> correctFiles = new List<FileInfo>();

            foreach (string file in allFiles)
            {
                var splited = file.Split(@"\").Last().ToLower();
                bool correct = false;

                for (int i = 0; i < Configuration.SearchPrefix.Length; i++)
                {
                    if (Configuration.SearchPrefix[i] != splited[i])
                    {
                        correct = false;
                        break;
                    }

                    correct = true;
                }

                if (correct)
                {
                    var fileData = LoadData(file).GetEnumerator();

                    using(fileData)
                        while (fileData.MoveNext())
                            correctFiles.Add(new FileInfo(file, fileData.Current));               
                }

            }

            return correctFiles;
        }

        private IEnumerable<byte[]> LoadData(string filePath)
        {

            var alldata = new List<byte[]>();
            string ext = Path.GetExtension(filePath);

            switch(ext)
            {
                case ".pdf":
                    alldata.AddRange(Pdf2Image.PdfSplitter.ExtractJpeg(filePath));
                    break;

                default:
                    if (Configuration.DataFileExtFilter.Contains(ext))
                    {
                        alldata.Add(File.ReadAllBytes(filePath));
                        break;
                    }

                    throw new Exception("Zły format pliku.");
            }

            return alldata;

        }

        public void ImportContractors()
        {
            
        }

        /// <summary>
        /// Importing cotractor list from excel file.
        /// </summary>
        /// <param name="filePath">Path to file</param>
        /// <exception cref="Exception">When wrong file type is selected this will throw.</exception>
        public void ImportContractors(string filePath)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            IDictionary? oldList = null;

            if (ContractorsService.ContractorsService.All.Count > 0)
                oldList = ContractorsService.ContractorsService.All;



            using (var filestram = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                string ext = Path.GetExtension(filePath);

                IExcelDataReader excelData;
                var a = new ExcelReaderConfiguration();

                switch (ext)
                {
                    case ".xlsb":
                        excelData = ExcelReaderFactory.CreateReader(filestram);
                        break;

                    case ".xlsx":
                        excelData = ExcelReaderFactory.CreateReader(filestram);
                        break;

                    case ".xls":
                        excelData = ExcelReaderFactory.CreateReader(filestram);
                        break;

                    case ".csv":
                        excelData = ExcelReaderFactory.CreateCsvReader(filestram);
                        break;

                    default:
                        throw new Exception("Błędne roszerzenie pliku.");
                }

                //EXTRA READ FOR HEADER SKIP
                //TODO: Popraw aby wykrywał miejsce w którym zaczynją sie dane.
                excelData.Read();

                do
                {
                    while (excelData.Read())
                    {
                        var read = excelData.GetValue(1).ToString();

                        if (read != null)
                        {
                            read = Regex.Replace(read, "[A-Z-]", "");
                            var contractor = new ContractorsService.Contractor { Name = excelData.GetString(0), NIP = read };

                            if (oldList != null && oldList[read] != null)
                            {
                                var b = (Contractor)oldList[read];
                                
                                contractor.OCRScanAreas = b.OCRScanAreas;
                            }

                            ContractorsService.ContractorsService.All.TryAdd(read, new Contractor {Name = excelData.GetString(0), NIP = read });
                            continue;
                        }

                    }

                } while (excelData.NextResult());


                
                excelData.Dispose();
                GC.Collect();
            }
        }

        public void SaveContractors()
        {
            if(File.Exists(Configuration.ContractorDataPath))
            {
                var rawText = JsonConvert.SerializeObject(ContractorsService.ContractorsService.All, Formatting.Indented);
                File.WriteAllText(Configuration.ContractorDataPath, rawText);
            }
        }

        private string GetContractorsRawData()
        {
            if (File.Exists(Configuration.ContractorDataPath))
            {
                return File.ReadAllText(Configuration.ContractorDataPath);
            }

            var stream = File.Create(Configuration.ContractorDataPath);
            stream.Dispose();
            
            return string.Empty;
        }
        
    }


    //
    public class FileReaderConfiguration
    {
        public string SaveDir { get; set; } = @".\";
        public string SearchDir { get; set; } = @".\";
        public string SearchPrefix { get; set; } = string.Empty;
        public string ImportExtFilter { get; set; } = "*.*";
        public string ExportExtFilter { get; set; } = "*.*";
        public string DataFileExtFilter { get; set; } = "*.*";
        public string ContractorDataPath { get; } = @".\contractor.json";

    }
    
    public class FileInfo : IDisposable
    {
        public FileInfo(string filePath, byte[] fileData)
        {
            FilePath = filePath;
            FileExt = Path.GetExtension(filePath);
            FileData = fileData;
        }

        public string FilePath { get; private set; }
        public byte[] FileData { get; private set; }
        public string FileExt { get; }
        public System.Drawing.Image ImageData { get; private set; }

        public void LoadImageAsBitmap()
        {
            using(MemoryStream ms = new MemoryStream(FileData))
            {
                ImageData = Bitmap.FromStream(ms);
            }
            
        }

        public void Dispose()
        {
            ImageData.Dispose();
        }
    }


}

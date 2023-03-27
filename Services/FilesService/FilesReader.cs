using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft;
using ExcelDataReader;
using System.Data;

namespace Services.FilesService
{
    public class FilesReaderService
    {

        public string SearchPrefix { get; set; }
        public string SearchLocation { get; set; }
        public List<string> AllSearchedFiles { get; }

        public static Dictionary<string, string> ContractorList { get; } = new Dictionary<string, string>();

        public FilesReaderService() 
        {
            SearchPrefix = "";
            SearchLocation = "";
            AllSearchedFiles = new List<string>();
            
        }

        public IEnumerable<string> CreateFileList()
        {
            List<string> allFiles = Directory.GetFiles(SearchLocation, "*.*", SearchOption.TopDirectoryOnly).ToList();
            List<string> correctFiles = new List<string>();

            foreach (string file in allFiles)
            {
                var splited = file.Split(@"\").Last().ToLower();
                bool correct = false;

                for(int i = 0; i < SearchPrefix.Length; i++) 
                {
                    if (SearchPrefix[i] != splited[i])
                    {
                        correct = false;
                        break;
                    }
                    correct = true;

                }

                if (correct)
                    correctFiles.Add(file);

            }

            return correctFiles;
        }
        
        public void ImportContractors(string filePath)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var filestram = File.Open(filePath, FileMode.Open, FileAccess.Read);

            string ext = Path.GetExtension(filePath);

            var excelData = ExcelReaderFactory.CreateOpenXmlReader(filestram);
            excelData.Read();
            

            do
            {
                while (excelData.Read()) {

                    ContractorList.TryAdd(excelData.GetValue(0).ToString(), excelData.GetString(0));
                }

            } while(excelData.NextResult());
           
        }   
    }
}

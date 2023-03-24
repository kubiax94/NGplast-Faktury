using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft;

namespace Services.FilesService
{
    public class FilesReaderService
    {

        public string SearchPrefix { get; set; }
        public string SearchLocation { get; set; }
        public List<string> AllSearchedFiles { get; }

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
        
        //public IEnumerable<object> ImportContractors(string filePath, )
        //{
            
        //}
    }
}

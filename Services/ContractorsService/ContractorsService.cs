using Services.FilesService;
using Services.OCRService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Services.ContractorsService
{
    internal class ContractorsService
    {
        IFilesReaderService _fileReader;
        internal static ContractorList All { get; private set; } = new ContractorList();
        internal static ContractorList SetNewList { set => All = value; } 

        internal ContractorsService(IFilesReaderService filesReader) 
        {
            _fileReader = filesReader;
            

        }

        private void Reset() {
            All.Clear();
        }


        public Contractor GetContractor(string nip)
        {
            Contractor result;

            if(All.ContainsKey(nip))
               return All[nip];   
            

            result = new Contractor{ NIP = nip };
            return result;
        }

        public void ImportContractos(string filePath)
        {
            
        }

        public void AddNewContractor(string nip, string name)
        {
            if (All.ContainsKey(nip))
                throw new Exception($"Podany NIP znajduje się już w bazie.");


            var newContractor = new Contractor { NIP = nip, Name = name };
            All.Add(nip, newContractor);
        }

        public void UpdateContractor(Contractor contractor)
        {

            if (All.ContainsKey(contractor.NIP) && All.isLoaded)
                All[contractor.NIP] = contractor;

        }


        /// <summary>
        /// Get name of contractor base on nip.
        /// </summary>
        /// <param name="nip">nip nr of contractor</param>
        /// <returns>Contractor name but if nip not exist in db then return string.empty</returns>
        public string GetContractorName(string nip)
        {
            All.TryGetValue(nip, out var result);

            if(result != null)
                return result.Name;

            return string.Empty;
        }
    }

    public class ContractorList : Dictionary<string, Contractor>
    {
        public bool isLoaded => this.Count > 0; 



        public ContractorList()
        {

        }

        public ContractorList(IEnumerable<Contractor> contractors)
        {
            var contractor = contractors.GetEnumerator();


            using(contractor)
            {
                while(contractor.MoveNext())
                {

                    if (this.TryAdd(contractor.Current.NIP, contractor.Current))
                        throw new Exception("Duplicate contractor nip in list");
                    
                }
            }
        }

    }

    public class Contractor
    {
        private bool _updated = false;

        public string NIP { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public Dictionary<string, InnvoiceOCRScanArea> OCRScanAreas { get; set; } = new();
    }
}

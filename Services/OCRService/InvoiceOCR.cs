using IronOcr;
using IronSoftware.Drawing;
using Services.FilesService;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static IronOcr.OcrResult;

namespace Services.OCRService
{
    public class InnvoiceOCR
    {

        string activeFilter;
        string deflautFilter = @"[A-Z0-9]+\/+[A-Z0-9]+\/?[A-Z0-9]+\/?[A-Z0-9]+\w";
        string otherFilter = @"([A-Z]+[0-9]{5,9})\w+";

        //@"(NIP+[: -]+?[ A-Z]{1,6}\d+)"
        string nipFilter = @"(NIP)";
        string blacklist = "6452536067";

        public InnvoiceOCR() {

        }

        public void SearchForInnvoicesDetails(InnvoiceOCRInput inputs)
        {

            var Ocr = new IronTesseract();
            Ocr.Configuration.TesseractVersion = TesseractVersion.Tesseract5; 

            foreach (var file in inputs)
            {
                using (var Input = new OcrInput(file.Key))
                {
                    activeFilter = deflautFilter;
                    Input.DeNoise();
                    Input.Deskew();
                    var Result = Ocr.Read(Input);
                    
                    Console.WriteLine($"Dla pliku: { file.Key }");


                    var inv_lines = Result.Lines.Where(x => x.Text.ToLower().Contains("faktura")).ToList();


                    for (int i =0; i < Result.Lines.Length; i++)
                    {

                        var fv_nr = GetInnvoiceInfo(Result.Lines[i], deflautFilter);


                        if (file.Value.NIP == string.Empty) {
                            var nip = FindContractorName(Result.Lines[i], nipFilter);

                            if (nip != string.Empty)
                            {
                                FilesReaderService.ContractorList.TryGetValue(nip, out var name);
                                file.Value.ContractorName = name;
                                file.Value.NIP = nip;
                            }
                                
                        }


                        if (fv_nr != null && fv_nr.Confidence > 80 && !file.Value.Found)
                        {
                            file.Value.Found = true;
                            file.Value.Acc = fv_nr.Confidence;
                            file.Value.InnvoiceNumber = fv_nr.Text;
                            file.Value.OrginalFileName = Path.GetFileName(file.Key);
                            
                        }
                            
                        if(file.Value.Found && file.Value.NIP != string.Empty)
                            break;

                        if(i == Result.Lines.Length - 1 && activeFilter != otherFilter)
                        {
                            activeFilter = otherFilter;
                            i = 0;
                        }
                    }
                }
            }
        }
        private OcrResult.Word? GetInnvoiceInfo(OcrResult.Line line, string regex)
        {

            if (Regex.IsMatch(line.Text, regex)) {


                foreach (var word in line.Words)
                {
                    if (Regex.IsMatch(word.Text, regex))
                        return word;
                }

            }

            return null;
        }

        private string FindContractorName(OcrResult.Line line, string filter)
        {

            if(Regex.IsMatch(line.Text ,filter))
            {
                string trim;
                foreach (var word in line.Words)
                {
                    trim = Regex.Replace(word.Text, @"[-]", "");

                    if (Regex.IsMatch(trim, @"[0-9]{9,14}\d+") && trim != blacklist)
                    {
                        return trim;
                    }     
                }
  
            }

            return string.Empty;
        }
    }


    public class InnvoiceOCRInput : Dictionary<string, InnvoiceOCRResult>
    {

        public InnvoiceOCRInput(string filePath)
        {

            if (!TryAdd(filePath, new InnvoiceOCRResult()))
                throw new Exception("DUPLICATE FILE NAME");

        }

        public InnvoiceOCRInput(IEnumerable<string> filesPath)
        {

            foreach(var file in filesPath)
            {
                if(!TryAdd(file, new InnvoiceOCRResult())) {
                    throw new Exception("DUPLICATE FILE NAME");
                }

                TryGetValue(file, out var result);

                if(result != null)
                    result.OrginalFileName = Path.GetFileName(file);
            }
        }

        public IEnumerable<InnvoiceOCRResult> InnvoiceOCRResults { 
            get {
                return this.Values.ToList();
            }
        }

    }

    public class InnvoiceOCRResult
    {
        public string ContractorName { get; set; } = string.Empty;
        public string InnvoiceNumber { get; set; } = string.Empty;
        public string OrginalFileName { get; set; } = string.Empty;
        public string NIP { get; set; } = string.Empty; 
        public double Acc { get; set; }
        public bool Found { get; set; } = false;


        public override string ToString()
        {
            return $@"Nazwa kontrahęta: { ContractorName }, Nr Faktury: { InnvoiceNumber }, Precyzja: { Acc }";
        }
    }
}

using IronOcr;
using IronSoftware.Drawing;
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


        public InnvoiceOCR() {

        }

        public void SearchForInnvoicesDetails(InnvoiceOCRInput inputs)
        {

            var Ocr = new IronTesseract();


            foreach (var file in inputs)
            {
                using (var Input = new OcrInput(file.Key))
                {
                    activeFilter = deflautFilter;

                    Input.Deskew();
                    var Result = Ocr.Read(Input);
                    
                    Console.WriteLine($"Dla pliku: { file.Key }");
                    
                    for (int i =0; i < Result.Lines.Length; i++)
                    {
                        var filtred = GetInnvoiceInfo(Result.Lines[i], activeFilter);

                        if (filtred != null && filtred.Confidence > 80)
                        {
                            file.Value.Found = true;
                            file.Value.Acc = filtred.Confidence;
                            file.Value.InnvoiceNumber = filtred.Text;
                            file.Value.OrginalFileName = Path.GetFileName(file.Key);
                            
                        }
                            

                        if(file.Value.Found)
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
        private static OcrResult.Word GetInnvoiceInfo(OcrResult.Line line, string filters)
        {

            if (Regex.IsMatch(line.Text, filters)) {


                foreach (var word in line.Words)
                {
                    if (Regex.IsMatch(word.Text, filters))
                        return word;
                }

            }

            return null;
        }

        private static string FindContractorName(IEnumerable<string> lines)
        {
            foreach(var line in lines)
            {
                if(line.ToLower() == "NIP")
                {
                    var trim = line.Split(' ');
                    return trim[trim.Length - 1];
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

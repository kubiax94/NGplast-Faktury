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

namespace Services.OCRService
{
    public class InnvoiceOCR
    {

        string filters = "FS,(S),MAG";


        public InnvoiceOCR() {

        }

        public void SearchForInnvoicesDetails(IEnumerable<string> files)
        {

            var Ocr = new IronTesseract();

            foreach (var file in files)
            {
                using (var Input = new OcrInput(file))
                {

                    Input.Deskew();
                    var Result = Ocr.Read(Input);

                    Console.WriteLine($"Dla pliku: {file}");

                    foreach (var line in Result.Lines)
                    {
                        var filtred = GetInnvoiceNumber(line.Text, filters);

                        if (filtred == string.Empty)
                            continue;

                        Console.WriteLine(filtred);
                    }

                }
            }
        }
        //[A-Z]{1,5}(\/[A-Z]{2})?\/\d{1,5}(\/\d+)?(\/[A-Z]+)?
        private static string GetInnvoiceNumber(string line, string filters)
        {
            var prefixs = filters.Split(',');
            var lineSplited = line.Split(' ');


            if (lineSplited[0].ToLower().Contains("faktura") || lineSplited[0].ToLower().Contains("nr"))
            {
                foreach (var prefix in prefixs)
                {
                    foreach (var word in lineSplited)
                    {
                        if (word.Contains(prefix))
                            return word;
                    }
                }
            }

            return string.Empty;
        }
    }


    public class InnvoiceOCRInput : Dictionary<string, InnvoiceOCRResult>
    {
        public Dictionary<string, InnvoiceOCRResult> Result { get; set; }


        public InnvoiceOCRInput(string filePath)
        {
            if (Result == null)
                Result = new Dictionary<string, InnvoiceOCRResult>();


            if (!Result.TryAdd(filePath, new InnvoiceOCRResult()))
                throw new Exception("DUPLICATE FILE NAME");

        }

        public InnvoiceOCRInput(string[] filesPath)
        {

            if (Result == null)
                Result = new Dictionary<string, InnvoiceOCRResult>();

            foreach(var file in filesPath)
            {
                if(!Result.TryAdd(file, new InnvoiceOCRResult())) {
                    throw new Exception("DUPLICATE FILE NAME");
                }
            }
        }

        public void Clear()
        {
            Result.Clear();
        }
    }

    public class InnvoiceOCRResult
    {
        public string ContractorName { get; set; } = string.Empty;
        public string InnvoiceNumber { get; set; } = string.Empty;


        public override string ToString()
        {
            return $@"Nazwa kontrahęta: { ContractorName }, Nr Faktury: { InnvoiceNumber }";
        }
    }
}

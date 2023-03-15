using IronOcr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.OCRService
{
    public class InvoiceOCR
    {
        string _inputFile = "C:/test.pdf";
        Stream _outputFile;
        



        public InvoiceOCR() {

        }

        public void SearchForInnvoicesDetails(IEnumerable<string> files) 
        {

            var Ocr = new IronTesseract();



            foreach(var file in files)
            {
                using (var Input = new OcrInput(file))
                {

                    // Input.Deskew();  // use if image not straight
                    // Input.DeNoise(); // use if image contains digital noise
                    var Result = Ocr.Read(Input);

                    Console.WriteLine(Result.Text);
                }
            }
        }
    }
}

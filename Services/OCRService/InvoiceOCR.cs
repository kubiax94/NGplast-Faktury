using IronOcr;
using IronSoftware.Drawing;
using Services.ContractorsService;
using Services.FilesService;
using Services.OCRService;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tesseract;
using static IronOcr.OcrInput;
using static IronOcr.OcrResult;
using static iTextSharp.text.pdf.AcroFields;
using static System.Net.Mime.MediaTypeNames;

namespace Services.OCRService
{
    public class InnvoiceOCR
    {
        private static string DataLanguageDir = "./data/";

        private static TesseractEngine Engine { get; }
        private static InnvoiceOCRSearchFilter  Filter { get; }

        static InnvoiceOCR()
        {
            Engine = new TesseractEngine(DataLanguageDir, "pol+eng", EngineMode.Default);
            Filter = new InnvoiceOCRSearchFilter("pol");
            Filter.AddWordToBlackList("6452536067");
            Filter.AddWordToBlackList("645-253-60-67");
            Filter.AddWordToBlackList("PL6452536067");
            Filter.AddWordToBlackList("Data");
            Filter.AddWordToBlackList("tel");
        }

        IFilesReaderService _filesReader;

        string activeFilter;
        string deflautFilter = @"[A-Z0-9]+\/+[A-Z0-9]+\/?[A-Z0-9]+\/?[A-Z0-9]+\w";
        string otherFilter = @"([A-Z]+[0-9]{5,9})\w+";
        string[] filters = { @"([A-Z0-9]+\/+[A-Z0-9]+\/?[A-Z0-9]+\/?[A-Z0-9]+\w)?([A-Z]+[0-9]{5,9})\w+", @"([A-Z]+[0-9]{5,9})\w+", @"[0-9]{9,10}}" };
        //@"(NIP+[: -]+?[ A-Z]{1,6}\d+)"
        string nipFilter = @"(NIP)";
        string blacklist = "6452536067";

        public InnvoiceOCR(IFilesReaderService filesReader) {

            _filesReader = filesReader;


           // test();
        }

        



        public void test(InnvoiceOCRInputList inputs)
        {
            IEnumerable<InnvoiceOCRInput> _inputs = inputs.Keys;


            using (Engine)
            {
                foreach(var input in _inputs)
                {
                    using (var img = Pix.LoadFromMemory(input.File.FileData))
                    {
                        var betterimg = img.Deskew();
                        using (var page = Engine.Process(betterimg, new Rect(0, 0, 1650, (int)(img.Height * 0.5))))
                        {
                            //var res = pattern.Matches(page.GetText());
                            //Console.WriteLine(res.FirstOrDefault());

                            var iter = page.GetIterator();

                            Filter.SearchForContractor(iter, inputs[input]);

                        }
                        betterimg.Dispose();
                    }
                }
            }
        }

        private bool CheckForCorrectValues(string line, string[] regex)
        {

            foreach (var regexItem in regex)
            {
                if (Regex.IsMatch(line, regexItem))
                {
                    return true;
                }

            }
            return false;
        }

        private bool CheckForCorrectValues(string line, string[] regex, out string[]? splitedvalue)
        {
            splitedvalue = line.Split(" ");
            return (CheckForCorrectValues(line, regex));
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


    public class InnvoiceOCRInput
    {
        private List<InnvoiceOCRScanArea> innvoiceOCRScanAreas = new List<InnvoiceOCRScanArea>();
        public FilesService.FileInfo File { get; private set; }

        private InnvoiceOCRInput()
        {
            innvoiceOCRScanAreas.Capacity = 2;


        }

        public InnvoiceOCRInput(FilesService.FileInfo fileInfo) : this()
        {
            File = fileInfo;
        }

        private InnvoiceOCRScanArea? GetScanArea(string name)
        {
            return innvoiceOCRScanAreas.Find(x => x.Name == name);
        }

        public void AddScanArea(InnvoiceOCRScanArea innvoiceOCRScanArea)
        {
            if(innvoiceOCRScanAreas.Any(x => x.Name == innvoiceOCRScanArea.Name))
                throw new Exception("ScanArea name exist!");
            
            innvoiceOCRScanAreas.Add(innvoiceOCRScanArea);
        } 
        
    }


    public class InnvoiceOCRInputList : Dictionary<InnvoiceOCRInput, InnvoiceOCRResult>
    {

        public InnvoiceOCRInputList(FilesService.FileInfo input)
        {

            if (!TryAdd(new InnvoiceOCRInput(input), new InnvoiceOCRResult(input)))
                throw new Exception("DUPLICATE FILE NAME");

        }

        public InnvoiceOCRInputList(IEnumerable<FilesService.FileInfo> inputs) 
        {

            foreach(var input in inputs)
            {
                if (!TryAdd(new InnvoiceOCRInput(input), new InnvoiceOCRResult(input)))
                    throw new Exception("DUPLICATE FILE NAME");
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

        public InnvoiceOCRResult(FilesService.FileInfo fileInfo)
        {
            FileInfo = fileInfo;
        }

        FilesService.FileInfo FileInfo { get; set; }
        public byte[] ImageData => FileInfo.FileData;

        public Contractor? Contractor { get; private set; }
        public Contractor SetContractor { set {
                Contractor = value;
            }
        }

        public string ContractorName { get => Contractor == null ? string.Empty : Contractor.Name; }
        public IEnumerable<Rect> GetScanAreas {
            get {
                List<Rect> results = new List<Rect>();


                if (Contractor == null)
                    return results;


                foreach(var r in Contractor.OCRScanAreas)
                {
                    results.Add(r.Value.GetArea());
                }

                return results;
            }

        }


        string _iNumber = string.Empty;
        public string InnvoiceNumber {
            get {
                return _iNumber;
            }

            set {
                _iNumber = value.ToUpper();
            }
        }

        public string OrginalFileName => FileInfo.FilePath; 
        public double Acc { get; set; }
        public bool Found { get; set; } = false;
        public string CustomFilters { get; set; } = string.Empty;
        public string NIP { get => Contractor == null ? string.Empty : Contractor.NIP; }

        public bool hasScanAreas => ScanArea.Count > 0;
        public List<InnvoiceOCRScanArea> ScanArea { get; } = new List<InnvoiceOCRScanArea>();
        public void AddAreaToScan(InnvoiceOCRScanArea area)
        {
            if (Contractor == null)
                return;

            ScanArea.Add(area);

            if (!Contractor.OCRScanAreas.ContainsKey(area.Name))
            {
                Contractor.OCRScanAreas.Add(area.Name, area);
            }

            if (!ContractorsService.ContractorsService.All[NIP].OCRScanAreas.ContainsKey(area.Name))
            {
                ContractorsService.ContractorsService.All[NIP].OCRScanAreas.Add(area.Name, area);
            }
        }

        public override string ToString()
        {
            return $@"Nazwa kontrahęta: { ContractorName }, Nr Faktury: { InnvoiceNumber }, Precyzja: { Acc }";
        }
    }

    public class InnvoiceOCRScanArea
    {

        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public InnvoiceOCRScanArea()
        {

        }

        public InnvoiceOCRScanArea(string name, Rect area)
        {
            Name = name;
            X = area.X1;
            Y = area.Y1;
            Width = area.Width;
            Height = area.Height;
        }

        internal System.Drawing.Image SetImageData { get; set; }

        public Rect GetArea()
        {
            return new Rect(X, Y, Width, Height);
        }

    }

    enum SearchType 
    {
        Filter,
        ScanArea
    }

    internal class InnvoiceOCRSearchFilter
    {
        private static Dictionary<string, Regex> MainFilterList = new Dictionary<string, Regex>();

        private static string TrimNIP(string nip, bool trimCountrySymbol = false)
        {
            return Regex.Replace(nip, @"([A-Z :,.-])", ""); ;
        }


        static InnvoiceOCRSearchFilter()
        {
            MainFilterList.Clear();

            //MainFilterList.Add("NIP", new Regex(@"(NIP+[: -])+?( [A-Z]{2})?([ 0-9-]{9,})")); OLD WAY
            MainFilterList.Add("NIP", new Regex(@"(NIP)+([ :.-])+([ A-Z]{2})+([ 0-9-]{9,})"));
            MainFilterList.Add("NR", new Regex("[-()A-Z0-9]+\\/+[A-Z0-9]+\\/?[A-Z0-9]+\\/?[A-Z0-9]+\\w"));
        }

        public string Lang { get; } = "";


        protected List<string> Blacklist { get; private set; } = new List<string>();
        protected List<string> Whitelist { get; private set; } = new List<string>();


        public InnvoiceOCRSearchFilter(string lang)
        {     
            Lang = lang;
        }
        public void AddWordToWhitelist(string word)
        {
            Whitelist.Add(word.ToLower());
        }
        public void AddWordToBlackList(string word)
        {
            Blacklist.Add(word.ToLower());
        }

        public string SearchForContractor(ResultIterator textIter, InnvoiceOCRResult result)
        {

            if (result.NIP != string.Empty)
                return result.NIP;

            textIter.Begin();

            Regex nipFilter = MainFilterList["NIP"];          

            using (textIter)
            {
                do
                {
                    do
                    {
                        
                        do
                        {
                            var text = textIter.GetText(PageIteratorLevel.TextLine);

                            Match match = nipFilter.Match(text);

                            if (!match.Success)
                                continue;

                            var nip = TrimNIP(match.Value);


                            if (CheckForBlacList(nip, out string ss))
                                continue;

                            do
                            {
                                var word = textIter.GetText(PageIteratorLevel.Word);
                                word = TrimNIP(word);


                                if (word.Contains(nip))
                                {
                                    textIter.TryGetBoundingBox(PageIteratorLevel.Word, out var rect);

                                    
                                    if (ContractorsService.ContractorsService.All.ContainsKey(nip))
                                    {
                                        Console.WriteLine(result.OrginalFileName);
                                        result.SetContractor = ContractorsService.ContractorsService.All[nip];
                                        result.AddAreaToScan(new InnvoiceOCRScanArea("NIP", rect));
                                        return nip;
                                    }
                                }

                            } while (textIter.Next(PageIteratorLevel.TextLine, PageIteratorLevel.Word));
                        } while (textIter.Next(PageIteratorLevel.Para, PageIteratorLevel.TextLine));
                    } while (textIter.Next(PageIteratorLevel.Block, PageIteratorLevel.Para));
                } while (textIter.Next(PageIteratorLevel.Block));

            }

            return string.Empty;
        }

        private bool CheckForBlacList(string text, out string result)
        {
            
            foreach (string bl in Blacklist)
            {
                if (text.Contains(bl, StringComparison.CurrentCultureIgnoreCase))
                {
                    result = bl;
                     
                    return true;
                }
                    
            }
            result = string.Empty;
            return false;
        }

    }

    public class InnvoiceOCRSearchConfig
    {
        SearchType SearchType { get; }
        public InnvoiceOCRScanArea? OCRScanArea { get; set; }
        public InnvoiceOCRSearchConfig() { }
    }

}

namespace Services.FilesService
{
    public interface IFilesReaderService
    {

        IEnumerable<FileInfo> CreateFileList();
        Dictionary<string, string> ContractorList { get; }
        
        IEnumerable<string> GetContractorNames { get; }
        FileReaderConfiguration Configuration { get; }

        void ImportContractors(string filePath);
        void SaveContractors();
    }
}
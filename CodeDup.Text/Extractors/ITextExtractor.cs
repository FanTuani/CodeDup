using System.Threading.Tasks;

namespace CodeDup.Text.Extractors
{
    public interface ITextExtractor
    {
        bool CanHandle(string extension);
        Task<string> ExtractTextAsync(string filePath);
    }
}



using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Repositories.Base;

public abstract class BaseCsvFileRepository<T> where T: IEquatable<T>
{
    private const string CsvDataFilesPathConfigSection = "CsvDataFileNames";

    private readonly CsvConfiguration _csvConfig = new(CultureInfo.InvariantCulture)
    {
        TrimOptions = TrimOptions.Trim,
        PrepareHeaderForMatch = args => args.Header.ToLower()
    };

    private readonly IQueryable<T> _cache;

    protected BaseCsvFileRepository(IConfiguration appConfig, string fileNameConfigKey)
    {
        var fromConfig = appConfig.GetSection(CsvDataFilesPathConfigSection)
            .GetSection(fileNameConfigKey).Value;

        if (fromConfig is null)
        {
            throw new ApplicationException($"Repository filepath for {fileNameConfigKey} not configured.");
        }

        var filePath = Path.Combine(fromConfig.Split('/'));

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, _csvConfig);

        _cache = csv.GetRecords<T>().ToList().AsQueryable();
    }

    protected IQueryable<T> GetAll()
    {
        return _cache;
    }
}
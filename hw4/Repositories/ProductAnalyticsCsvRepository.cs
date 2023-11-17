using System.Globalization;
using System.Threading.Channels;
using CsvHelper;
using CsvHelper.Configuration;
using homework_4.Configuration;
using homework_4.Entities;
using homework_4.Interfaces;
using Microsoft.Extensions.Options;

namespace homework_4.Repositories;

public sealed class ProductAnalyticsCsvRepository : IProgressCounter, IProductAnalyticsRepository
{
    private readonly string _pathToFile;
    private int _progressCount;

    public ProductAnalyticsCsvRepository(IOptionsMonitor<RepositoryOptions> repositoryOptionsMonitor)
    {
        _pathToFile = repositoryOptionsMonitor.CurrentValue.InputCsvFilePath;
    }

    public async Task ReadAsync(Channel<ProductAnalytics> output, CancellationToken cancelToken)
    {
        using var reader = new StreamReader(_pathToFile);
        using var csvReader = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            TrimOptions = TrimOptions.Trim,
            PrepareHeaderForMatch = args => args.Header.ToLower()
        });

        await foreach (var productAnalytics in csvReader.GetRecordsAsync<ProductAnalytics>(cancelToken))
        {
            Interlocked.Increment(ref _progressCount);
            await output.Writer.WriteAsync(productAnalytics, cancelToken);
        }

        output.Writer.Complete();
    }

    public int ProgressCount => _progressCount;
}
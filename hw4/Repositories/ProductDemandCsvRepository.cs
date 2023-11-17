using System.Globalization;
using System.Threading.Channels;
using CsvHelper;
using CsvHelper.Configuration;
using homework_4.Configuration;
using homework_4.Entities;
using homework_4.Interfaces;
using Microsoft.Extensions.Options;

namespace homework_4.Repositories;

public sealed class ProductDemandCsvRepository : IProgressCounter, IProductDemandRepository
{
    private readonly string _pathToFile;
    private int _progressCount;

    public ProductDemandCsvRepository(IOptionsMonitor<RepositoryOptions> repositoryOptionsMonitor)
    {
        _pathToFile = repositoryOptionsMonitor.CurrentValue.OutputCsvFilePath;
    }

    public async Task WriteAsync(Channel<ProductDemand> input, CancellationToken cancelToken)
    {
        await using var writer = new StreamWriter(_pathToFile);
        await using var csvWriter = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            TrimOptions = TrimOptions.Trim,
            PrepareHeaderForMatch = args => args.Header.ToLower()
        });

        csvWriter.WriteHeader<ProductDemand>();
        await csvWriter.NextRecordAsync();

        await foreach (var productDemand in input.Reader.ReadAllAsync(cancelToken))
        {
            Interlocked.Increment(ref _progressCount);
            csvWriter.WriteRecord(productDemand);
            await csvWriter.NextRecordAsync();
        }
    }

    public int ProgressCount => _progressCount;
}
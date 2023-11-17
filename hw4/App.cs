using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Channels;
using homework_4.Configuration;
using homework_4.Entities;
using homework_4.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace homework_4;

public sealed class App : IDisposable
{
    private readonly IOptionsMonitor<AppOptions> _appOptionsMonitor;
    private readonly IDisposable? _appOptionsOnChangeDisposable;
    private readonly ICalculateService _calculateService;
    private readonly ILogger<App> _logger;
    private readonly IProductAnalyticsRepository _productAnalyticsRepository;
    private readonly IProductDemandRepository _productDemandRepository;
    private readonly Channel<ProductAnalytics> _readChannel;
    private readonly Channel<ProductDemand> _writeChannel;

    private readonly CancellationTokenSource _logProgressIntervalCancelTokenSource = new();
    private readonly CancellationTokenSource _rwCancelTokenSource = new();
    private readonly Stopwatch _stopwatch = new();
    private readonly Queue<Task> _tasks = new();
    private readonly ConcurrentDictionary<Task, CancellationTokenSource> _tasksCancelTokenSources = new();

    private volatile bool _isRepositoryReadingFinish;

    public App(IOptionsMonitor<AppOptions> appOptionsMonitor, ILogger<App> logger, ICalculateService calculateService,
        IProductAnalyticsRepository productAnalyticsRepository, IProductDemandRepository productDemandRepository)
    {
        _logger = logger;

        _calculateService = calculateService;
        _productAnalyticsRepository = productAnalyticsRepository;
        _productDemandRepository = productDemandRepository;

        _readChannel = Channel.CreateBounded<ProductAnalytics>(new BoundedChannelOptions(1)
        {
            SingleWriter = true
        });
        _writeChannel = Channel.CreateUnbounded<ProductDemand>(new UnboundedChannelOptions
        {
            SingleReader = true
        });

        _appOptionsMonitor = appOptionsMonitor;
        _appOptionsOnChangeDisposable = _appOptionsMonitor.OnChange(DegreeOfParallelismChangedHandler);

        Console.CancelKeyPress += CancelKeyPressEventHandler;
    }

    public void Dispose()
    {
        _rwCancelTokenSource.Dispose();

        foreach (var cancelTokenSource in _tasksCancelTokenSources.Values)
            cancelTokenSource.Dispose();

        _appOptionsOnChangeDisposable?.Dispose();
    }

    public async Task RunAsync()
    {
        try
        {
            var degreeOfParallelism = _appOptionsMonitor.CurrentValue.DegreeOfParallelism;
            _logger.LogInformation("Application started with degree of parallelism: {}", degreeOfParallelism);
            _stopwatch.Start();

            StartWorkers(degreeOfParallelism);

            var logProgressTask = LogProgressWithIntervalAsync(_logProgressIntervalCancelTokenSource.Token);
            var readTask = _productAnalyticsRepository.ReadAsync(_readChannel, _rwCancelTokenSource.Token);
            var writeTask = _productDemandRepository.WriteAsync(_writeChannel, _rwCancelTokenSource.Token);

            await readTask;
            await _readChannel.Reader.Completion;
            _isRepositoryReadingFinish = true;

            await Task.WhenAll(_tasks);

            _writeChannel.Writer.Complete();
            await writeTask;

            _logProgressIntervalCancelTokenSource.Cancel();
            await logProgressTask;
        }
        catch (OperationCanceledException ex)
        {
            LogOperationCanceledException(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError("An error has happened during application run. {}", ex.Message);
        }
        finally
        {
            _stopwatch.Stop();
            LogProgress();
            _logger.LogInformation("Application stopped. Elapsed time: {}", _stopwatch.Elapsed.ToString());
        }
    }

    private void StartWorkers(int workerCount)
    {
        for (var i = 0; i < workerCount; i++)
        {
            var cancelTokenSource = new CancellationTokenSource();
            var cancelToken = cancelTokenSource.Token;

            var task = _calculateService.CalculateDemandAsync(_readChannel, _writeChannel, cancelToken);
            _tasks.Enqueue(task);
            _tasksCancelTokenSources[task] = cancelTokenSource;
        }
    }

    private void DegreeOfParallelismChangedHandler(AppOptions options)
    {
        lock (_tasks)
        {
            if (_isRepositoryReadingFinish) return;

            var workerCount = options.DegreeOfParallelism;
            _logger.LogInformation("Changed degree of parallelism, current = {}", workerCount);

            if (workerCount > _tasks.Count)
            {
                var workersDelta = workerCount - _tasks.Count;
                StartWorkers(workersDelta);
            }
            else if (workerCount < _tasks.Count)
            {
                var workersDelta = _tasks.Count - workerCount;
                for (var i = 0; i < workersDelta; i++)
                {
                    var task = _tasks.Dequeue();
                    _tasksCancelTokenSources[task].Cancel();
                }
            }
        }
    }

    private async Task LogProgressWithIntervalAsync(CancellationToken cancelToken)
    {
        var interval = _appOptionsMonitor.CurrentValue.NotificationIntervalSec * 1000;

        while (!cancelToken.IsCancellationRequested)
        {
            LogProgress();
            await Task.Delay(interval, cancelToken);
        }
    }

    private void LogProgress()
    {
        _logger.LogInformation("Read: {}, Calculated: {}, Written: {}",
            (_productAnalyticsRepository as IProgressCounter)?.ProgressCount.ToString(),
            (_calculateService as IProgressCounter)?.ProgressCount.ToString(),
            (_productDemandRepository as IProgressCounter)?.ProgressCount.ToString());
    }

    private void CancelKeyPressEventHandler(object? _, ConsoleCancelEventArgs eventArgs)
    {
        eventArgs.Cancel = true;

        try
        {
            if (!_rwCancelTokenSource.IsCancellationRequested)
                _rwCancelTokenSource.Cancel();

            var notCanceledTasks = _tasks.Where(x => !x.IsCanceled);
            foreach (var task in notCanceledTasks)
                _tasksCancelTokenSources[task].Cancel();

            if (!_logProgressIntervalCancelTokenSource.IsCancellationRequested)
                _logProgressIntervalCancelTokenSource.Cancel();
        }
        catch (OperationCanceledException ex)
        {
            LogOperationCanceledException(ex);
        }
    }

    private void LogOperationCanceledException(OperationCanceledException ex)
    {
        _logger.LogDebug("Exception: {}.\n{}", ex.Message, ex.StackTrace);
    }
}
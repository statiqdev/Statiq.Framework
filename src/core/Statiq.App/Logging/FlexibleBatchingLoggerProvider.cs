using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetEscapades.Extensions.Logging.RollingFile.Internal;

namespace Statiq.App
{
    // Based on https://github.com/andrewlock/NetEscapades.Extensions.Logging/blob/master/src/NetEscapades.Extensions.Logging.RollingFile/Internal/BatchingLoggerProvider.cs
    public abstract class FlexibleBatchingLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private readonly List<FlexibleLogMessage> _currentBatch = new List<FlexibleLogMessage>();
        private readonly TimeSpan _interval;
        private readonly int? _queueSize;
        private readonly int? _batchSize;
        private readonly IDisposable _optionsChangeToken;

        private BlockingCollection<FlexibleLogMessage> _messageQueue;
        private Task _outputTask;
        private CancellationTokenSource _cancellationTokenSource;

        private bool _includeScopes;
        private IExternalScopeProvider _scopeProvider;

        internal IExternalScopeProvider ScopeProvider => _includeScopes ? _scopeProvider : null;

        protected FlexibleBatchingLoggerProvider(IOptionsMonitor<BatchingLoggerOptions> options)
        {
            // NOTE: Only IsEnabled and IncludeScopes are monitored

            BatchingLoggerOptions loggerOptions = options.CurrentValue;
            if (loggerOptions.BatchSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(loggerOptions.BatchSize), $"{nameof(loggerOptions.BatchSize)} must be a positive number.");
            }
            if (loggerOptions.FlushPeriod <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(loggerOptions.FlushPeriod), $"{nameof(loggerOptions.FlushPeriod)} must be longer than zero.");
            }

            _interval = loggerOptions.FlushPeriod;
            _batchSize = loggerOptions.BatchSize;
            _queueSize = loggerOptions.BackgroundQueueSize;

            _optionsChangeToken = options.OnChange(UpdateOptions);
            UpdateOptions(options.CurrentValue);
        }

        public bool IsEnabled { get; private set; }

        private void UpdateOptions(BatchingLoggerOptions options)
        {
            bool oldIsEnabled = IsEnabled;
            IsEnabled = options.IsEnabled;
            _includeScopes = options.IncludeScopes;

            if (oldIsEnabled != IsEnabled)
            {
                if (IsEnabled)
                {
                    Start();
                }
                else
                {
                    Stop();
                }
            }
        }

        protected abstract Task WriteMessagesAsync(IEnumerable<FlexibleLogMessage> messages, CancellationToken token);

        private async Task ProcessLogQueueAsync()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                int limit = _batchSize ?? int.MaxValue;

                while (limit > 0 && _messageQueue.TryTake(out FlexibleLogMessage message))
                {
                    _currentBatch.Add(message);
                    limit--;
                }

                if (_currentBatch.Count > 0)
                {
                    try
                    {
                        await WriteMessagesAsync(_currentBatch, _cancellationTokenSource.Token);
                    }
                    catch
                    {
                        // ignored
                    }

                    _currentBatch.Clear();
                }

                await IntervalAsync(_interval, _cancellationTokenSource.Token);
            }
        }

        protected virtual Task IntervalAsync(TimeSpan interval, CancellationToken cancellationToken)
        {
            return Task.Delay(interval, cancellationToken);
        }

        internal void AddMessage(FlexibleLogMessage logMessage)
        {
            if (!_messageQueue.IsAddingCompleted)
            {
                try
                {
                    _messageQueue.Add(logMessage, _cancellationTokenSource.Token);
                }
                catch
                {
                    // cancellation token canceled or CompleteAdding called
                }
            }
        }

        private void Start()
        {
            _messageQueue = _queueSize == null ?
                new BlockingCollection<FlexibleLogMessage>(new ConcurrentQueue<FlexibleLogMessage>()) :
                new BlockingCollection<FlexibleLogMessage>(new ConcurrentQueue<FlexibleLogMessage>(), _queueSize.Value);

            _cancellationTokenSource = new CancellationTokenSource();
            _outputTask = Task.Run(ProcessLogQueueAsync);
        }

        private void Stop()
        {
            _cancellationTokenSource.Cancel();
            _messageQueue.CompleteAdding();

            try
            {
                _outputTask.Wait(_interval);
            }
            catch (TaskCanceledException)
            {
            }
            catch (AggregateException ex) when (ex.InnerExceptions.Count == 1 && ex.InnerExceptions[0] is TaskCanceledException)
            {
            }
        }

        public void Dispose()
        {
            _optionsChangeToken?.Dispose();
            if (IsEnabled)
            {
                Stop();
            }
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FlexibleBatchingLogger(this, categoryName);
        }

        void ISupportExternalScope.SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }
    }
}

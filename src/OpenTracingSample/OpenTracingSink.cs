using System;
using System.Collections.Generic;
using OpenTracing;
using OpenTracing.Util;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace OpenTracingSample
{
    public class OpenTracingSink : ILogEventSink
    {
        private const string Level = "level";
        private const string Undefined = "Undefined";
        private const string Log = "log";
        private const string MessageTemplate = "message.template";
        private const string LoggingError = "mbv.common.logging.error";

        private readonly ITracer _tracer;
        private readonly IFormatProvider _formatProvider;

        public OpenTracingSink(ITracer tracer, IFormatProvider formatProvider)
        {
            _tracer = tracer;
            _formatProvider = formatProvider;
        }

        public void Emit(LogEvent logEvent)
        {
            var span = _tracer.ActiveSpan;
            if (_tracer.ActiveSpan != null)
            {
                var logEventDict = GetLogEventDict(logEvent);
                span.Log(logEventDict);
            }
        }

        private Dictionary<string, object> GetLogEventDict(LogEvent logEvent)
        {
            var logEventDict = new Dictionary<string, object>
            {
                { Level, logEvent?.Level.ToString() ?? Undefined },
                { LogFields.Event, Log }
            };

            try
            {
                logEventDict[LogFields.Message] = logEvent.RenderMessage(_formatProvider);
                logEventDict[MessageTemplate] = logEvent.MessageTemplate.Text;
                if (logEvent.Exception != null)
                {
                    logEventDict[LogFields.ErrorKind] = logEvent.Exception.GetType().FullName;
                    logEventDict[LogFields.ErrorObject] = logEvent.Exception;
                }

                if (logEvent.Properties != null)
                    foreach (var property in logEvent.Properties)
                        logEventDict[property.Key] = property.Value;
            }
            catch (Exception e)
            {
                logEventDict[LoggingError] = $"{e}";
            }

            return logEventDict;
        }
    }

    public static class OpenTracingSinkExtensions
    {
        public static LoggerConfiguration OpenTracing(this LoggerSinkConfiguration loggerConfiguration, IFormatProvider formatProvider = null)
            => loggerConfiguration.Sink(new OpenTracingSink(GlobalTracer.Instance, formatProvider));
    }
}

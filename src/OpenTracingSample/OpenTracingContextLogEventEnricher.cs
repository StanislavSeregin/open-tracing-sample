using System;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using OpenTracing.Util;

namespace OpenTracingSample
{
    public class OpenTracingContextLogEventEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var tracer = GlobalTracer.Instance;
            if (tracer?.ActiveSpan == null)
                return;

            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TraceId", tracer.ActiveSpan.Context.TraceId));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("SpanId", tracer.ActiveSpan.Context.SpanId));
        }
    }

    public static class LoggingExtensions
    {
        public static LoggerConfiguration WithOpenTracingContext(this LoggerEnrichmentConfiguration enrich)
        {
            if (enrich == null)
                throw new ArgumentNullException(nameof(enrich));

            return enrich.With<OpenTracingContextLogEventEnricher>();
        }
    }
}

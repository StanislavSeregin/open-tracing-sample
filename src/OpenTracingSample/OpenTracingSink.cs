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
        private readonly ITracer _tracer;
        private readonly IFormatProvider _formatProvider;

        public OpenTracingSink(ITracer tracer, IFormatProvider formatProvider)
        {
            _tracer = tracer;
            _formatProvider = formatProvider;
        }

        public void Emit(LogEvent logEvent)
        {
            ISpan span = _tracer.ActiveSpan;
            if (span == null)
                return;

            var fields = new Dictionary<string, object>
            {
                { "component", logEvent.Properties["SourceContext"] },
                { "level", logEvent.Level.ToString() }
            };

            fields[LogFields.Event] = "log";
            try
            {
                fields[LogFields.Message] = logEvent.RenderMessage(_formatProvider);
                fields["message.template"] = logEvent.MessageTemplate.Text;

                if (logEvent.Exception != null)
                {
                    fields[LogFields.ErrorKind] = logEvent.Exception.GetType().FullName;
                    fields[LogFields.ErrorObject] = logEvent.Exception;
                }

                if (logEvent.Properties != null)
                {
                    foreach (var property in logEvent.Properties)
                        fields[property.Key] = property.Value;
                }
            }
            catch (Exception logException)
            {
                fields["mbv.common.logging.error"] = logException.ToString();
            }

            span.Log(fields);
        }
    }

    public static class OpenTracingSinkExtensions
    {
        public static LoggerConfiguration OpenTracing(this LoggerSinkConfiguration loggerConfiguration, IFormatProvider formatProvider = null)
            => loggerConfiguration.Sink(new OpenTracingSink(GlobalTracer.Instance, formatProvider));
    }
}

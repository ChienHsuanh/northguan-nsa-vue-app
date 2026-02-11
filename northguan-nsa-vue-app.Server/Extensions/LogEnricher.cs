using Serilog.Core;
using Serilog.Events;

namespace northguan_nsa_vue_app.Server.Extensions {
    public class LogEnricher : ILogEventEnricher {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory) {
            if(logEvent.Properties.TryGetValue("SourceContext",out LogEventPropertyValue sourceContext)) {
                var controllerName = sourceContext.ToString().Replace("\"", "").Split('.').Last().Replace("Controllers", "");
                logEvent.AddPropertyIfAbsent(new LogEventProperty("ControllerName",new ScalarValue(controllerName)));
            }
        }
    }
}
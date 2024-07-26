using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Instrumentation.AspNetCore;

namespace Tracing
{
	public static class OpenTelemetryExtension
	{
		public static IServiceCollection AddOpenTelemetryTracing(this IServiceCollection services, IConfiguration configuration)
		{
			
			Action<ResourceBuilder> configureResource = r => r.AddService(
			serviceName: configuration.GetValue("OpenTelemetry:ServiceName", defaultValue: "unknown")!,
			serviceVersion: configuration.GetValue("OpenTelemetry:ServiceVersion", defaultValue: "unknown")!,
			serviceInstanceId: Environment.MachineName);

			services.AddOpenTelemetry()
				.ConfigureResource(configureResource)
				.WithTracing(builder =>
				{
					builder
						.AddHttpClientInstrumentation()
						.AddAspNetCoreInstrumentation();

					services.Configure<AspNetCoreTraceInstrumentationOptions>(configuration.GetSection("AspNetCoreInstrumentation"));
					builder.AddOtlpExporter(otlpOptions =>
					{
						otlpOptions.Endpoint = new Uri(configuration.GetValue("Otlp:Endpoint", defaultValue: "http://otel:4317")!);
					});
				});
			return services;
		}
	}
}


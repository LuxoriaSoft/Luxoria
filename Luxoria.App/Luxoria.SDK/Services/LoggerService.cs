using Luxoria.SDK.Interfaces;
using Luxoria.SDK.Models;
using Sentry;
using Sentry.Profiling;
using System.Diagnostics;
using System.Reflection;
using System.IO;

namespace Luxoria.SDK.Services
{
    public class AssemblyLoader
    {
        public static void LoadEmbeddedDll(string resourceName, string outputPath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceStream = assembly.GetManifestResourceStream(resourceName);

            if (resourceStream == null)
                throw new FileNotFoundException($"Embedded resource {resourceName} not found.");

            // Ensure the directory exists before extracting the file
            var directory = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
            {
                resourceStream.CopyTo(fileStream);
            }

            // Load the DLL from the output path
            Assembly.LoadFrom(outputPath);
        }

        public static void LoadEmbeddedSentryDll()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), "Luxoria.SDK");
            string sentryDllPath = Path.Combine(tempDirectory, "Sentry.dll");
            string sentryProfilingDllPath = Path.Combine(tempDirectory, "Sentry.Profiling.dll");

            // Adjust resource names based on actual namespace and folder structure
            // Microsoft.Diagnostics.NETCore.Client.dll
            // Microsoft.Diagnostics.FastSerialization.dll
            // Microsoft.Diagnostics.Tracing.TraceEvent.dll
            // Sentry.dll
            // Sentry.Profiling.dll
            AssemblyLoader.LoadEmbeddedDll("Luxoria.SDK.libs.Microsoft.Diagnostics.NETCore.Client.dll", Path.Combine(tempDirectory, "Microsoft.Diagnostics.NETCore.Client.dll"));
            AssemblyLoader.LoadEmbeddedDll("Luxoria.SDK.libs.Microsoft.Diagnostics.FastSerialization.dll", Path.Combine(tempDirectory, "Microsoft.Diagnostics.FastSerialization.dll"));
            AssemblyLoader.LoadEmbeddedDll("Luxoria.SDK.libs.Microsoft.Diagnostics.Tracing.TraceEvent.dll", Path.Combine(tempDirectory, "Microsoft.Diagnostics.Tracing.TraceEvent.dll"));
            AssemblyLoader.LoadEmbeddedDll("Luxoria.SDK.libs.Sentry.dll", sentryDllPath);
            AssemblyLoader.LoadEmbeddedDll("Luxoria.SDK.libs.Sentry.Profiling.dll", sentryProfilingDllPath);
        }
    }
}


namespace Luxoria.SDK.Services
{
    /// <summary>
    /// Provides logging functionality with support for multiple log targets and caller information.
    /// </summary>
    public class LoggerService : ILoggerService
    {
        private readonly List<ILogTarget> _logTargets = new();
        private readonly LogLevel _minLogLevel;

        /// <summary>
        /// Initializes the logger with a minimum log level and log targets.
        /// </summary>
        /// <param name="minLogLevel">The minimum log level for filtering logs.</param>
        /// <param name="targets">The log targets to write logs to.</param>
        public LoggerService(LogLevel minLogLevel, params ILogTarget[] targets)
        {
            _minLogLevel = minLogLevel;
            _logTargets.AddRange(targets);

            // Load the embedded Sentry DLLs before initializing Sentry
            AssemblyLoader.LoadEmbeddedSentryDll();

            // Initialize Sentry
            SentrySdkInit();
        }

        public static void LoadEmbeddedSentryDll()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), "Luxoria.SDK");
            string sentryDllPath = Path.Combine(tempDirectory, "Sentry.dll");
            string sentryProfilingDllPath = Path.Combine(tempDirectory, "Sentry.Profiling.dll");

            AssemblyLoader.LoadEmbeddedDll("Luxoria.SDK.libs.Sentry.dll", sentryDllPath);
            AssemblyLoader.LoadEmbeddedDll("Luxoria.SDK.libs.Sentry.Profiling.dll", sentryProfilingDllPath);
        }


        private static void SentrySdkInit() =>
            SentrySdk.Init(options =>
            {
                // A Sentry Data Source Name (DSN) is required.
                // See https://docs.sentry.io/product/sentry-basics/dsn-explainer/
                // You can set it in the SENTRY_DSN environment variable, or you can set it in code here.
                options.Dsn = "https://73a8787dbb4c056c9a8f1f19e57cd7f6@o4508597414068224.ingest.de.sentry.io/4508608595099728";

                // When debug is enabled, the Sentry client will emit detailed debugging information to the console.
                // This might be helpful, or might interfere with the normal operation of your application.
                // We enable it here for demonstration purposes when first trying Sentry.
                // You shouldn't do this in your applications unless you're troubleshooting issues with Sentry.
                options.Debug = true;

                // This option is recommended. It enables Sentry's "Release Health" feature.
                options.AutoSessionTracking = true;

                // Set TracesSampleRate to 1.0 to capture 100%
                // of transactions for tracing.
                // We recommend adjusting this value in production.
                options.TracesSampleRate = 1.0;

                // Sample rate for profiling, applied on top of othe TracesSampleRate,
                // e.g. 0.2 means we want to profile 20 % of the captured transactions.
                // We recommend adjusting this value in production.
                options.ProfilesSampleRate = 1.0;

                // Requires NuGet package: Sentry.Profiling
                // Note: By default, the profiler is initialized asynchronously. This can
                // be tuned by passing a desired initialization timeout to the constructor.
                options.AddIntegration(new ProfilingIntegration(
                    // During startup, wait up to 500ms to profile the app startup code.
                    // This could make launching the app a bit slower so comment it out if you
                    // prefer profiling to start asynchronously
                    TimeSpan.FromMilliseconds(500)
                ));
            });

        public void ConfigureGlobalExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                string message = "Unhandled exception: " + args.ExceptionObject;
                Debug.WriteLine(message);

                if (args.ExceptionObject is Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                    Log(message, "UnhandledException", LogLevel.Error);
                }
            };

            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                string message = "Unobserved task exception: " + args.Exception.Message;
                Debug.WriteLine(message);

                SentrySdk.CaptureException(args.Exception);
                Log(message, "UnobservedTaskException", LogLevel.Error);

                args.SetObserved();
            };
        }

        public void Log(
            string message,
            string category = "General",
            LogLevel level = LogLevel.Info,
            string callerName = "",
            string callerFile = "",
            int callerLine = 0)
        {
            if (level < _minLogLevel) return;

            string formattedMessage = FormatLog(message, category, level, callerName, callerFile, callerLine);

            // Capture error logs in Sentry
            if (level == LogLevel.Error)
            {
                SentrySdk.CaptureMessage(formattedMessage, SentryLevel.Error);
            }

            foreach (var target in _logTargets)
            {
                target.WriteLog(formattedMessage);
            }
        }

        public async Task LogAsync(
            string message,
            string category = "General",
            LogLevel level = LogLevel.Info,
            string callerName = "",
            string callerFile = "",
            int callerLine = 0)
        {
            if (level < _minLogLevel) return;

            string formattedMessage = FormatLog(message, category, level, callerName, callerFile, callerLine);

            // Capture error logs in Sentry
            if (level == LogLevel.Error)
            {
                SentrySdk.CaptureMessage(formattedMessage, SentryLevel.Error);
            }

            foreach (var target in _logTargets)
            {
                await Task.Run(() => target.WriteLog(formattedMessage));
            }
        }

        private static string FormatLog(
            string message,
            string category,
            LogLevel level,
            string callerName,
            string callerFile,
            int callerLine)
        {
            string fileName = Path.GetFileName(callerFile);
            return $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {category} " +
                   $"[{fileName}:{callerLine} {callerName}]: {message}";
        }

        public void Dispose()
        {
            SentrySdk.Close();
        }
    }
}

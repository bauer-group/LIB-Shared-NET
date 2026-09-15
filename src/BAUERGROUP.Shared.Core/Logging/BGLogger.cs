using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NLog.Targets;
using NLog.Config;
using System.Reflection;
using System.Threading.Tasks;

namespace BAUERGROUP.Shared.Core.Logging
{
    /// <summary>
    /// Provides a centralized, singleton-based logging implementation using NLog.
    /// </summary>
    /// <remarks>
    /// This class wraps NLog functionality and provides static methods for logging at various levels:
    /// Error, Fatal, Debug, Warn, Trace, and Info. It also supports automatic unhandled exception reporting.
    /// </remarks>
    public sealed class BGLogger
    {
        #region Instance
        /// <summary>
        /// Gets the underlying NLog Logger instance.
        /// </summary>
        public Logger Backend { get; private set; }

        private BGLogger()
        {
            var assembly = Assembly.GetEntryAssembly();
            if (assembly == null)
            {
                if (String.IsNullOrWhiteSpace(BGLoggerConfiguration.ApplicationName))
                    Backend = LogManager.GetCurrentClassLogger();
                else
                    Backend = LogManager.GetLogger(BGLoggerConfiguration.ApplicationName);
            }
            else
            {
                var name = assembly.GetName().Name;
                Backend = String.IsNullOrWhiteSpace(name) ? LogManager.GetCurrentClassLogger() : LogManager.GetLogger(name);
            }
        }

        ~BGLogger()
        {
            LogManager.Flush();
        }
        #endregion

        #region CommonStatic
        static BGLogger()
        {

        }

        private static readonly BGLogger InstanceInternal = new BGLogger();

        /// <summary>
        /// Gets the singleton instance of the BGLogger.
        /// </summary>
        public static BGLogger Instance { get { return InstanceInternal; } }

        private static readonly BGLoggerConfiguration InstanceConfiguration = new BGLoggerConfiguration();

        /// <summary>
        /// Gets the configuration settings for the logger.
        /// </summary>
        public static BGLoggerConfiguration Configuration { get { return InstanceConfiguration; } }
        #endregion

        #region LogingFunctions
        public static void Error(object value) { Instance.Backend.Error(value); }
        public static void Error(string message) { Instance.Backend.Error(message); }
        public static void Error<T>(T value) { Instance.Backend.Error<T>(value); }
        public static void Error(Exception exception, string message) { Instance.Backend.Error(exception, message); }
        public static void Error(IFormatProvider formatProvider, object value) { Instance.Backend.Error(formatProvider, value); }
        public static void Error<T>(IFormatProvider formatProvider, T value) { Instance.Backend.Error<T>(formatProvider, value); }
        public static void Error(string message, bool argument) { Instance.Backend.Error(message, argument); }
        public static void Error(string message, byte argument) { Instance.Backend.Error(message, argument); }
        public static void Error(string message, char argument) { Instance.Backend.Error(message, argument); }
        public static void Error(string message, decimal argument) { Instance.Backend.Error(message, argument); }
        public static void Error(string message, double argument) { Instance.Backend.Error(message, argument); }
        public static void Error(string message, float argument) { Instance.Backend.Error(message, argument); }
        public static void Error(string message, int argument) { Instance.Backend.Error(message, argument); }
        public static void Error(string message, long argument) { Instance.Backend.Error(message, argument); }
        public static void Error(string message, object argument) { Instance.Backend.Error(message, argument); }
        public static void Error(string message, sbyte argument) { Instance.Backend.Error(message, argument); }
        public static void Error(string message, string argument) { Instance.Backend.Error(message, argument); }
        public static void Error(string message, params object[] args) { Instance.Backend.Error(message, args); }
        public static void Error<TArgument>(string message, TArgument argument) { Instance.Backend.Error(message, argument); }
        public static void Error(string message, uint argument) { Instance.Backend.Error(message, argument); }
        public static void Error(string message, ulong argument) { Instance.Backend.Error(message, argument); }
        public static void Error(Exception exception, string message, params object[] args) { Instance.Backend.Error(exception, message, args); }
        public static void Error(Exception exception, IFormatProvider formatProvider, string message, params object[] args) { Instance.Backend.Error(formatProvider, message, args); }
        public static void Error(IFormatProvider formatProvider, string message, bool argument) { Instance.Backend.Error(formatProvider, message, argument); }
        public static void Error(IFormatProvider formatProvider, string message, byte argument) { Instance.Backend.Error(formatProvider, message, argument); }
        public static void Error(IFormatProvider formatProvider, string message, char argument) { Instance.Backend.Error(formatProvider, message, argument); }
        public static void Error(IFormatProvider formatProvider, string message, decimal argument) { Instance.Backend.Error(formatProvider, message, argument); }
        public static void Error(IFormatProvider formatProvider, string message, double argument) { Instance.Backend.Error(formatProvider, message, argument); }
        public static void Error(IFormatProvider formatProvider, string message, float argument) { Instance.Backend.Error(formatProvider, message, argument); }
        public static void Error(IFormatProvider formatProvider, string message, int argument) { Instance.Backend.Error(formatProvider, message, argument); }
        public static void Error(IFormatProvider formatProvider, string message, long argument) { Instance.Backend.Error(formatProvider, message, argument); }
        public static void Error(IFormatProvider formatProvider, string message, object argument) { Instance.Backend.Error(formatProvider, message, argument); }
        public static void Error(IFormatProvider formatProvider, string message, sbyte argument) { Instance.Backend.Error(formatProvider, message, argument); }
        public static void Error(IFormatProvider formatProvider, string message, string argument) { Instance.Backend.Error(formatProvider, message, argument); }
        public static void Error<TArgument>(IFormatProvider formatProvider, string message, TArgument argument) { Instance.Backend.Error(formatProvider, message, argument); }
        public static void Error(IFormatProvider formatProvider, string message, uint argument) { Instance.Backend.Error(formatProvider, message, argument); }
        public static void Error(IFormatProvider formatProvider, string message, ulong argument) { Instance.Backend.Error(formatProvider, message, argument); }
        public static void Error(IFormatProvider formatProvider, string message, params object[] args) { Instance.Backend.Error(formatProvider, message, args); }
        public static void Error(string message, object arg1, object arg2) { Instance.Backend.Error(message, arg1, arg2); }
        public static void Error(string message, object arg1, object arg2, object arg3) { Instance.Backend.Error(message, arg1, arg2, arg3); }
        public static void Error<TArgument1, TArgument2>(string message, TArgument1 argument1, TArgument2 argument2) { Instance.Backend.Error(message, argument1, argument2); }
        public static void Error<TArgument1, TArgument2, TArgument3>(string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3) { Instance.Backend.Error(message, argument1, argument2, argument3); }
        public static void Error<TArgument1, TArgument2>(IFormatProvider formatProvider, string message, TArgument1 argument1, TArgument2 argument2) { Instance.Backend.Error(formatProvider, message, argument1, argument2); }
        public static void Error<TArgument1, TArgument2, TArgument3>(IFormatProvider formatProvider, string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3) { Instance.Backend.Error(formatProvider, message, argument1, argument2, argument3); }

        public static void Fatal(object value) { Instance.Backend.Fatal(value); }
        public static void Fatal(string message) { Instance.Backend.Fatal(message); }
        public static void Fatal<T>(T value) { Instance.Backend.Fatal<T>(value); }
        public static void Fatal(Exception exception, string message) { Instance.Backend.Fatal(exception, message); }
        public static void Fatal(IFormatProvider formatProvider, object value) { Instance.Backend.Fatal(formatProvider, value); }
        public static void Fatal<T>(IFormatProvider formatProvider, T value) { Instance.Backend.Fatal<T>(formatProvider, value); }
        public static void Fatal(string message, bool argument) { Instance.Backend.Fatal(message, argument); }
        public static void Fatal(string message, byte argument) { Instance.Backend.Fatal(message, argument); }
        public static void Fatal(string message, char argument) { Instance.Backend.Fatal(message, argument); }
        public static void Fatal(string message, decimal argument) { Instance.Backend.Fatal(message, argument); }
        public static void Fatal(string message, double argument) { Instance.Backend.Fatal(message, argument); }
        public static void Fatal(string message, float argument) { Instance.Backend.Fatal(message, argument); }
        public static void Fatal(string message, int argument) { Instance.Backend.Fatal(message, argument); }
        public static void Fatal(string message, long argument) { Instance.Backend.Fatal(message, argument); }
        public static void Fatal(string message, object argument) { Instance.Backend.Fatal(message, argument); }
        public static void Fatal(string message, sbyte argument) { Instance.Backend.Fatal(message, argument); }
        public static void Fatal(string message, string argument) { Instance.Backend.Fatal(message, argument); }
        public static void Fatal(string message, params object[] args) { Instance.Backend.Fatal(message, args); }
        public static void Fatal<TArgument>(string message, TArgument argument) { Instance.Backend.Fatal(message, argument); }
        public static void Fatal(string message, uint argument) { Instance.Backend.Fatal(message, argument); }
        public static void Fatal(string message, ulong argument) { Instance.Backend.Fatal(message, argument); }
        public static void Fatal(Exception exception, string message, params object[] args) { Instance.Backend.Fatal(exception, message, args); }
        public static void Fatal(Exception exception, IFormatProvider formatProvider, string message, params object[] args) { Instance.Backend.Fatal(formatProvider, message, args); }
        public static void Fatal(IFormatProvider formatProvider, string message, bool argument) { Instance.Backend.Fatal(formatProvider, message, argument); }
        public static void Fatal(IFormatProvider formatProvider, string message, byte argument) { Instance.Backend.Fatal(formatProvider, message, argument); }
        public static void Fatal(IFormatProvider formatProvider, string message, char argument) { Instance.Backend.Fatal(formatProvider, message, argument); }
        public static void Fatal(IFormatProvider formatProvider, string message, decimal argument) { Instance.Backend.Fatal(formatProvider, message, argument); }
        public static void Fatal(IFormatProvider formatProvider, string message, double argument) { Instance.Backend.Fatal(formatProvider, message, argument); }
        public static void Fatal(IFormatProvider formatProvider, string message, float argument) { Instance.Backend.Fatal(formatProvider, message, argument); }
        public static void Fatal(IFormatProvider formatProvider, string message, int argument) { Instance.Backend.Fatal(formatProvider, message, argument); }
        public static void Fatal(IFormatProvider formatProvider, string message, long argument) { Instance.Backend.Fatal(formatProvider, message, argument); }
        public static void Fatal(IFormatProvider formatProvider, string message, object argument) { Instance.Backend.Fatal(formatProvider, message, argument); }
        public static void Fatal(IFormatProvider formatProvider, string message, sbyte argument) { Instance.Backend.Fatal(formatProvider, message, argument); }
        public static void Fatal(IFormatProvider formatProvider, string message, string argument) { Instance.Backend.Fatal(formatProvider, message, argument); }
        public static void Fatal<TArgument>(IFormatProvider formatProvider, string message, TArgument argument) { Instance.Backend.Fatal(formatProvider, message, argument); }
        public static void Fatal(IFormatProvider formatProvider, string message, uint argument) { Instance.Backend.Fatal(formatProvider, message, argument); }
        public static void Fatal(IFormatProvider formatProvider, string message, ulong argument) { Instance.Backend.Fatal(formatProvider, message, argument); }
        public static void Fatal(IFormatProvider formatProvider, string message, params object[] args) { Instance.Backend.Fatal(formatProvider, message, args); }
        public static void Fatal(string message, object arg1, object arg2) { Instance.Backend.Fatal(message, arg1, arg2); }
        public static void Fatal(string message, object arg1, object arg2, object arg3) { Instance.Backend.Fatal(message, arg1, arg2, arg3); }
        public static void Fatal<TArgument1, TArgument2>(string message, TArgument1 argument1, TArgument2 argument2) { Instance.Backend.Fatal(message, argument1, argument2); }
        public static void Fatal<TArgument1, TArgument2, TArgument3>(string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3) { Instance.Backend.Fatal(message, argument1, argument2, argument3); }
        public static void Fatal<TArgument1, TArgument2>(IFormatProvider formatProvider, string message, TArgument1 argument1, TArgument2 argument2) { Instance.Backend.Fatal(formatProvider, message, argument1, argument2); }
        public static void Fatal<TArgument1, TArgument2, TArgument3>(IFormatProvider formatProvider, string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3) { Instance.Backend.Fatal(formatProvider, message, argument1, argument2, argument3); }

        public static void Debug(object value) { Instance.Backend.Debug(value); }
        public static void Debug(string message) { Instance.Backend.Debug(message); }
        public static void Debug<T>(T value) { Instance.Backend.Debug<T>(value); }
        public static void Debug(Exception exception, string message) { Instance.Backend.Debug(exception, message); }
        public static void Debug(IFormatProvider formatProvider, object value) { Instance.Backend.Debug(formatProvider, value); }
        public static void Debug<T>(IFormatProvider formatProvider, T value) { Instance.Backend.Debug<T>(formatProvider, value); }
        public static void Debug(string message, bool argument) { Instance.Backend.Debug(message, argument); }
        public static void Debug(string message, byte argument) { Instance.Backend.Debug(message, argument); }
        public static void Debug(string message, char argument) { Instance.Backend.Debug(message, argument); }
        public static void Debug(string message, decimal argument) { Instance.Backend.Debug(message, argument); }
        public static void Debug(string message, double argument) { Instance.Backend.Debug(message, argument); }
        public static void Debug(string message, float argument) { Instance.Backend.Debug(message, argument); }
        public static void Debug(string message, int argument) { Instance.Backend.Debug(message, argument); }
        public static void Debug(string message, long argument) { Instance.Backend.Debug(message, argument); }
        public static void Debug(string message, object argument) { Instance.Backend.Debug(message, argument); }
        public static void Debug(string message, sbyte argument) { Instance.Backend.Debug(message, argument); }
        public static void Debug(string message, string argument) { Instance.Backend.Debug(message, argument); }
        public static void Debug(string message, params object[] args) { Instance.Backend.Debug(message, args); }
        public static void Debug<TArgument>(string message, TArgument argument) { Instance.Backend.Debug(message, argument); }
        public static void Debug(string message, uint argument) { Instance.Backend.Debug(message, argument); }
        public static void Debug(string message, ulong argument) { Instance.Backend.Debug(message, argument); }
        public static void Debug(Exception exception, string message, params object[] args) { Instance.Backend.Debug(exception, message, args); }
        public static void Debug(Exception exception, IFormatProvider formatProvider, string message, params object[] args) { Instance.Backend.Debug(formatProvider, message, args); }
        public static void Debug(IFormatProvider formatProvider, string message, bool argument) { Instance.Backend.Debug(formatProvider, message, argument); }
        public static void Debug(IFormatProvider formatProvider, string message, byte argument) { Instance.Backend.Debug(formatProvider, message, argument); }
        public static void Debug(IFormatProvider formatProvider, string message, char argument) { Instance.Backend.Debug(formatProvider, message, argument); }
        public static void Debug(IFormatProvider formatProvider, string message, decimal argument) { Instance.Backend.Debug(formatProvider, message, argument); }
        public static void Debug(IFormatProvider formatProvider, string message, double argument) { Instance.Backend.Debug(formatProvider, message, argument); }
        public static void Debug(IFormatProvider formatProvider, string message, float argument) { Instance.Backend.Debug(formatProvider, message, argument); }
        public static void Debug(IFormatProvider formatProvider, string message, int argument) { Instance.Backend.Debug(formatProvider, message, argument); }
        public static void Debug(IFormatProvider formatProvider, string message, long argument) { Instance.Backend.Debug(formatProvider, message, argument); }
        public static void Debug(IFormatProvider formatProvider, string message, object argument) { Instance.Backend.Debug(formatProvider, message, argument); }
        public static void Debug(IFormatProvider formatProvider, string message, sbyte argument) { Instance.Backend.Debug(formatProvider, message, argument); }
        public static void Debug(IFormatProvider formatProvider, string message, string argument) { Instance.Backend.Debug(formatProvider, message, argument); }
        public static void Debug<TArgument>(IFormatProvider formatProvider, string message, TArgument argument) { Instance.Backend.Debug(formatProvider, message, argument); }
        public static void Debug(IFormatProvider formatProvider, string message, uint argument) { Instance.Backend.Debug(formatProvider, message, argument); }
        public static void Debug(IFormatProvider formatProvider, string message, ulong argument) { Instance.Backend.Debug(formatProvider, message, argument); }
        public static void Debug(IFormatProvider formatProvider, string message, params object[] args) { Instance.Backend.Debug(formatProvider, message, args); }
        public static void Debug(string message, object arg1, object arg2) { Instance.Backend.Debug(message, arg1, arg2); }
        public static void Debug(string message, object arg1, object arg2, object arg3) { Instance.Backend.Debug(message, arg1, arg2, arg3); }
        public static void Debug<TArgument1, TArgument2>(string message, TArgument1 argument1, TArgument2 argument2) { Instance.Backend.Debug(message, argument1, argument2); }
        public static void Debug<TArgument1, TArgument2, TArgument3>(string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3) { Instance.Backend.Debug(message, argument1, argument2, argument3); }
        public static void Debug<TArgument1, TArgument2>(IFormatProvider formatProvider, string message, TArgument1 argument1, TArgument2 argument2) { Instance.Backend.Debug(formatProvider, message, argument1, argument2); }
        public static void Debug<TArgument1, TArgument2, TArgument3>(IFormatProvider formatProvider, string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3) { Instance.Backend.Debug(formatProvider, message, argument1, argument2, argument3); }

        public static void Warn(object value) { Instance.Backend.Warn(value); }
        public static void Warn(string message) { Instance.Backend.Warn(message); }
        public static void Warn<T>(T value) { Instance.Backend.Warn<T>(value); }
        public static void Warn(Exception exception, string message) { Instance.Backend.Warn(exception, message); }
        public static void Warn(IFormatProvider formatProvider, object value) { Instance.Backend.Warn(formatProvider, value); }
        public static void Warn<T>(IFormatProvider formatProvider, T value) { Instance.Backend.Warn<T>(formatProvider, value); }
        public static void Warn(string message, bool argument) { Instance.Backend.Warn(message, argument); }
        public static void Warn(string message, byte argument) { Instance.Backend.Warn(message, argument); }
        public static void Warn(string message, char argument) { Instance.Backend.Warn(message, argument); }
        public static void Warn(string message, decimal argument) { Instance.Backend.Warn(message, argument); }
        public static void Warn(string message, double argument) { Instance.Backend.Warn(message, argument); }
        public static void Warn(string message, float argument) { Instance.Backend.Warn(message, argument); }
        public static void Warn(string message, int argument) { Instance.Backend.Warn(message, argument); }
        public static void Warn(string message, long argument) { Instance.Backend.Warn(message, argument); }
        public static void Warn(string message, object argument) { Instance.Backend.Warn(message, argument); }
        public static void Warn(string message, sbyte argument) { Instance.Backend.Warn(message, argument); }
        public static void Warn(string message, string argument) { Instance.Backend.Warn(message, argument); }
        public static void Warn(string message, params object[] args) { Instance.Backend.Warn(message, args); }
        public static void Warn<TArgument>(string message, TArgument argument) { Instance.Backend.Warn(message, argument); }
        public static void Warn(string message, uint argument) { Instance.Backend.Warn(message, argument); }
        public static void Warn(string message, ulong argument) { Instance.Backend.Warn(message, argument); }
        public static void Warn(Exception exception, string message, params object[] args) { Instance.Backend.Warn(exception, message, args); }
        public static void Warn(Exception exception, IFormatProvider formatProvider, string message, params object[] args) { Instance.Backend.Warn(formatProvider, message, args); }
        public static void Warn(IFormatProvider formatProvider, string message, bool argument) { Instance.Backend.Warn(formatProvider, message, argument); }
        public static void Warn(IFormatProvider formatProvider, string message, byte argument) { Instance.Backend.Warn(formatProvider, message, argument); }
        public static void Warn(IFormatProvider formatProvider, string message, char argument) { Instance.Backend.Warn(formatProvider, message, argument); }
        public static void Warn(IFormatProvider formatProvider, string message, decimal argument) { Instance.Backend.Warn(formatProvider, message, argument); }
        public static void Warn(IFormatProvider formatProvider, string message, double argument) { Instance.Backend.Warn(formatProvider, message, argument); }
        public static void Warn(IFormatProvider formatProvider, string message, float argument) { Instance.Backend.Warn(formatProvider, message, argument); }
        public static void Warn(IFormatProvider formatProvider, string message, int argument) { Instance.Backend.Warn(formatProvider, message, argument); }
        public static void Warn(IFormatProvider formatProvider, string message, long argument) { Instance.Backend.Warn(formatProvider, message, argument); }
        public static void Warn(IFormatProvider formatProvider, string message, object argument) { Instance.Backend.Warn(formatProvider, message, argument); }
        public static void Warn(IFormatProvider formatProvider, string message, sbyte argument) { Instance.Backend.Warn(formatProvider, message, argument); }
        public static void Warn(IFormatProvider formatProvider, string message, string argument) { Instance.Backend.Warn(formatProvider, message, argument); }
        public static void Warn<TArgument>(IFormatProvider formatProvider, string message, TArgument argument) { Instance.Backend.Warn(formatProvider, message, argument); }
        public static void Warn(IFormatProvider formatProvider, string message, uint argument) { Instance.Backend.Warn(formatProvider, message, argument); }
        public static void Warn(IFormatProvider formatProvider, string message, ulong argument) { Instance.Backend.Warn(formatProvider, message, argument); }
        public static void Warn(IFormatProvider formatProvider, string message, params object[] args) { Instance.Backend.Warn(formatProvider, message, args); }
        public static void Warn(string message, object arg1, object arg2) { Instance.Backend.Warn(message, arg1, arg2); }
        public static void Warn(string message, object arg1, object arg2, object arg3) { Instance.Backend.Warn(message, arg1, arg2, arg3); }
        public static void Warn<TArgument1, TArgument2>(string message, TArgument1 argument1, TArgument2 argument2) { Instance.Backend.Warn(message, argument1, argument2); }
        public static void Warn<TArgument1, TArgument2, TArgument3>(string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3) { Instance.Backend.Warn(message, argument1, argument2, argument3); }
        public static void Warn<TArgument1, TArgument2>(IFormatProvider formatProvider, string message, TArgument1 argument1, TArgument2 argument2) { Instance.Backend.Warn(formatProvider, message, argument1, argument2); }
        public static void Warn<TArgument1, TArgument2, TArgument3>(IFormatProvider formatProvider, string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3) { Instance.Backend.Warn(formatProvider, message, argument1, argument2, argument3); }

        public static void Trace(object value) { Instance.Backend.Trace(value); }
        public static void Trace(string message) { Instance.Backend.Trace(message); }
        public static void Trace<T>(T value) { Instance.Backend.Trace<T>(value); }
        public static void Trace(Exception exception, string message) { Instance.Backend.Trace(exception, message); }
        public static void Trace(IFormatProvider formatProvider, object value) { Instance.Backend.Trace(formatProvider, value); }
        public static void Trace<T>(IFormatProvider formatProvider, T value) { Instance.Backend.Trace<T>(formatProvider, value); }
        public static void Trace(string message, bool argument) { Instance.Backend.Trace(message, argument); }
        public static void Trace(string message, byte argument) { Instance.Backend.Trace(message, argument); }
        public static void Trace(string message, char argument) { Instance.Backend.Trace(message, argument); }
        public static void Trace(string message, decimal argument) { Instance.Backend.Trace(message, argument); }
        public static void Trace(string message, double argument) { Instance.Backend.Trace(message, argument); }
        public static void Trace(string message, float argument) { Instance.Backend.Trace(message, argument); }
        public static void Trace(string message, int argument) { Instance.Backend.Trace(message, argument); }
        public static void Trace(string message, long argument) { Instance.Backend.Trace(message, argument); }
        public static void Trace(string message, object argument) { Instance.Backend.Trace(message, argument); }
        public static void Trace(string message, sbyte argument) { Instance.Backend.Trace(message, argument); }
        public static void Trace(string message, string argument) { Instance.Backend.Trace(message, argument); }
        public static void Trace(string message, params object[] args) { Instance.Backend.Trace(message, args); }
        public static void Trace<TArgument>(string message, TArgument argument) { Instance.Backend.Trace(message, argument); }
        public static void Trace(string message, uint argument) { Instance.Backend.Trace(message, argument); }
        public static void Trace(string message, ulong argument) { Instance.Backend.Trace(message, argument); }
        public static void Trace(Exception exception, string message, params object[] args) { Instance.Backend.Trace(exception, message, args); }
        public static void Trace(Exception exception, IFormatProvider formatProvider, string message, params object[] args) { Instance.Backend.Trace(formatProvider, message, args); }
        public static void Trace(IFormatProvider formatProvider, string message, bool argument) { Instance.Backend.Trace(formatProvider, message, argument); }
        public static void Trace(IFormatProvider formatProvider, string message, byte argument) { Instance.Backend.Trace(formatProvider, message, argument); }
        public static void Trace(IFormatProvider formatProvider, string message, char argument) { Instance.Backend.Trace(formatProvider, message, argument); }
        public static void Trace(IFormatProvider formatProvider, string message, decimal argument) { Instance.Backend.Trace(formatProvider, message, argument); }
        public static void Trace(IFormatProvider formatProvider, string message, double argument) { Instance.Backend.Trace(formatProvider, message, argument); }
        public static void Trace(IFormatProvider formatProvider, string message, float argument) { Instance.Backend.Trace(formatProvider, message, argument); }
        public static void Trace(IFormatProvider formatProvider, string message, int argument) { Instance.Backend.Trace(formatProvider, message, argument); }
        public static void Trace(IFormatProvider formatProvider, string message, long argument) { Instance.Backend.Trace(formatProvider, message, argument); }
        public static void Trace(IFormatProvider formatProvider, string message, object argument) { Instance.Backend.Trace(formatProvider, message, argument); }
        public static void Trace(IFormatProvider formatProvider, string message, sbyte argument) { Instance.Backend.Trace(formatProvider, message, argument); }
        public static void Trace(IFormatProvider formatProvider, string message, string argument) { Instance.Backend.Trace(formatProvider, message, argument); }
        public static void Trace<TArgument>(IFormatProvider formatProvider, string message, TArgument argument) { Instance.Backend.Trace(formatProvider, message, argument); }
        public static void Trace(IFormatProvider formatProvider, string message, uint argument) { Instance.Backend.Trace(formatProvider, message, argument); }
        public static void Trace(IFormatProvider formatProvider, string message, ulong argument) { Instance.Backend.Trace(formatProvider, message, argument); }
        public static void Trace(IFormatProvider formatProvider, string message, params object[] args) { Instance.Backend.Trace(formatProvider, message, args); }
        public static void Trace(string message, object arg1, object arg2) { Instance.Backend.Trace(message, arg1, arg2); }
        public static void Trace(string message, object arg1, object arg2, object arg3) { Instance.Backend.Trace(message, arg1, arg2, arg3); }
        public static void Trace<TArgument1, TArgument2>(string message, TArgument1 argument1, TArgument2 argument2) { Instance.Backend.Trace(message, argument1, argument2); }
        public static void Trace<TArgument1, TArgument2, TArgument3>(string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3) { Instance.Backend.Trace(message, argument1, argument2, argument3); }
        public static void Trace<TArgument1, TArgument2>(IFormatProvider formatProvider, string message, TArgument1 argument1, TArgument2 argument2) { Instance.Backend.Trace(formatProvider, message, argument1, argument2); }
        public static void Trace<TArgument1, TArgument2, TArgument3>(IFormatProvider formatProvider, string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3) { Instance.Backend.Trace(formatProvider, message, argument1, argument2, argument3); }

        public static void Info(object value) { Instance.Backend.Info(value); }
        public static void Info(string message) { Instance.Backend.Info(message); }
        public static void Info<T>(T value) { Instance.Backend.Info<T>(value); }
        public static void Info(Exception exception, string message) { Instance.Backend.Info(exception, message); }
        public static void Info(IFormatProvider formatProvider, object value) { Instance.Backend.Info(formatProvider, value); }
        public static void Info<T>(IFormatProvider formatProvider, T value) { Instance.Backend.Info<T>(formatProvider, value); }
        public static void Info(string message, bool argument) { Instance.Backend.Info(message, argument); }
        public static void Info(string message, byte argument) { Instance.Backend.Info(message, argument); }
        public static void Info(string message, char argument) { Instance.Backend.Info(message, argument); }
        public static void Info(string message, decimal argument) { Instance.Backend.Info(message, argument); }
        public static void Info(string message, double argument) { Instance.Backend.Info(message, argument); }
        public static void Info(string message, float argument) { Instance.Backend.Info(message, argument); }
        public static void Info(string message, int argument) { Instance.Backend.Info(message, argument); }
        public static void Info(string message, long argument) { Instance.Backend.Info(message, argument); }
        public static void Info(string message, object argument) { Instance.Backend.Info(message, argument); }
        public static void Info(string message, sbyte argument) { Instance.Backend.Info(message, argument); }
        public static void Info(string message, string argument) { Instance.Backend.Info(message, argument); }
        public static void Info(string message, params object[] args) { Instance.Backend.Info(message, args); }
        public static void Info<TArgument>(string message, TArgument argument) { Instance.Backend.Info(message, argument); }
        public static void Info(string message, uint argument) { Instance.Backend.Info(message, argument); }
        public static void Info(string message, ulong argument) { Instance.Backend.Info(message, argument); }
        public static void Info(Exception exception, string message, params object[] args) { Instance.Backend.Info(exception, message, args); }
        public static void Info(Exception exception, IFormatProvider formatProvider, string message, params object[] args) { Instance.Backend.Info(formatProvider, message, args); }
        public static void Info(IFormatProvider formatProvider, string message, bool argument) { Instance.Backend.Info(formatProvider, message, argument); }
        public static void Info(IFormatProvider formatProvider, string message, byte argument) { Instance.Backend.Info(formatProvider, message, argument); }
        public static void Info(IFormatProvider formatProvider, string message, char argument) { Instance.Backend.Info(formatProvider, message, argument); }
        public static void Info(IFormatProvider formatProvider, string message, decimal argument) { Instance.Backend.Info(formatProvider, message, argument); }
        public static void Info(IFormatProvider formatProvider, string message, double argument) { Instance.Backend.Info(formatProvider, message, argument); }
        public static void Info(IFormatProvider formatProvider, string message, float argument) { Instance.Backend.Info(formatProvider, message, argument); }
        public static void Info(IFormatProvider formatProvider, string message, int argument) { Instance.Backend.Info(formatProvider, message, argument); }
        public static void Info(IFormatProvider formatProvider, string message, long argument) { Instance.Backend.Info(formatProvider, message, argument); }
        public static void Info(IFormatProvider formatProvider, string message, object argument) { Instance.Backend.Info(formatProvider, message, argument); }
        public static void Info(IFormatProvider formatProvider, string message, sbyte argument) { Instance.Backend.Info(formatProvider, message, argument); }
        public static void Info(IFormatProvider formatProvider, string message, string argument) { Instance.Backend.Info(formatProvider, message, argument); }
        public static void Info<TArgument>(IFormatProvider formatProvider, string message, TArgument argument) { Instance.Backend.Info(formatProvider, message, argument); }
        public static void Info(IFormatProvider formatProvider, string message, uint argument) { Instance.Backend.Info(formatProvider, message, argument); }
        public static void Info(IFormatProvider formatProvider, string message, ulong argument) { Instance.Backend.Info(formatProvider, message, argument); }
        public static void Info(IFormatProvider formatProvider, string message, params object[] args) { Instance.Backend.Info(formatProvider, message, args); }
        public static void Info(string message, object arg1, object arg2) { Instance.Backend.Info(message, arg1, arg2); }
        public static void Info(string message, object arg1, object arg2, object arg3) { Instance.Backend.Info(message, arg1, arg2, arg3); }
        public static void Info<TArgument1, TArgument2>(string message, TArgument1 argument1, TArgument2 argument2) { Instance.Backend.Info(message, argument1, argument2); }
        public static void Info<TArgument1, TArgument2, TArgument3>(string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3) { Instance.Backend.Info(message, argument1, argument2, argument3); }
        public static void Info<TArgument1, TArgument2>(IFormatProvider formatProvider, string message, TArgument1 argument1, TArgument2 argument2) { Instance.Backend.Info(formatProvider, message, argument1, argument2); }
        public static void Info<TArgument1, TArgument2, TArgument3>(IFormatProvider formatProvider, string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3) { Instance.Backend.Info(formatProvider, message, argument1, argument2, argument3); }
        #endregion

        #region ApplicationHandler
        /// <summary>
        /// Enables or disables automatic logging of unhandled exceptions and unobserved task exceptions.
        /// </summary>
        /// <param name="enabled">If <c>true</c>, enables exception reporting; otherwise, disables it.</param>
        /// <remarks>
        /// When enabled, unhandled exceptions are logged at Fatal level before the application exits.
        /// </remarks>
        public static void UnhandledExceptionReporting(Boolean enabled = true)
        {
            if (enabled)
            {
                AppDomain.CurrentDomain.UnhandledException += ReportUnhandledException;
                TaskScheduler.UnobservedTaskException += ReportUnobservedTaskException;
            }
            else
            {
                AppDomain.CurrentDomain.UnhandledException -= ReportUnhandledException;
                TaskScheduler.UnobservedTaskException -= ReportUnobservedTaskException;
            }
        }

        private static void ReportUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            Fatal(e.Exception, String.Format("UnobservedTaskException => Observed = {0}", e.Observed));
            Environment.Exit(0);
        }

        private static void ReportUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Fatal((Exception)e.ExceptionObject, String.Format("UnhandledException => IsTerminating = {0}", e.IsTerminating));
            Environment.Exit(0);
        }
        #endregion
    }
}

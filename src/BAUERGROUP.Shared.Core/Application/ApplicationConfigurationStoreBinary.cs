#nullable enable

using BAUERGROUP.Shared.Core.Application.Interface;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

#pragma warning disable SYSLIB0011 // BinaryFormatter is obsolete

namespace BAUERGROUP.Shared.Core.Application;

/// <summary>
/// Binary configuration store using BinaryFormatter.
/// Note: BinaryFormatter is obsolete. Consider using ApplicationConfigurationStoreJSON instead.
/// </summary>
[Obsolete("Use ApplicationConfigurationStoreJSON instead. BinaryFormatter is deprecated in .NET 5+.")]
public class ApplicationConfigurationStoreBinary<T> : IApplicationConfigurationStore<T> where T : new()
{
    private T? _configuration;
    private readonly string _fileName;

    /// <summary>
    /// Creates a new binary configuration store.
    /// </summary>
    /// <param name="fileName">The configuration file path. If null, uses default location.</param>
    public ApplicationConfigurationStoreBinary(string? fileName = null)
    {
        _fileName = fileName ?? ConfigurationBinaryFileName;
        ConfigurationConverter = null;
    }

    /// <summary>
    /// Gets or sets the configuration.
    /// </summary>
    public virtual T Configuration
    {
        get
        {
            _configuration = Load();
            return _configuration;
        }
        set
        {
            _configuration = value;
            Save(_configuration);
        }
    }

    private T Load()
    {
        if (!File.Exists(_fileName))
            return new T();

        var binaryFormatter = new BinaryFormatter();

        if (ConfigurationConverter != null)
            binaryFormatter.Binder = ConfigurationConverter;

        using var fileStream = File.OpenRead(_fileName);
        var objectData = binaryFormatter.Deserialize(fileStream);

        return (T)objectData;
    }

    private void Save(T value)
    {
        var directory = Path.GetDirectoryName(_fileName);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var binaryFormatter = new BinaryFormatter();
        using var fileStream = File.OpenWrite(_fileName);
        binaryFormatter.Serialize(fileStream, value!);
    }

    /// <summary>
    /// Saves the current configuration to disk.
    /// </summary>
    public void Save()
    {
        if (_configuration != null)
        {
            Save(_configuration);
        }
    }

    /// <summary>
    /// Gets the default binary configuration file path.
    /// </summary>
    public static string ConfigurationBinaryFileName
    {
        get
        {
            var appDataFolder = ApplicationFolders.ExecutionAutomaticApplicationDataFolder;
            var entryAssembly = Assembly.GetEntryAssembly();
            var assemblyName = entryAssembly?.GetName().Name ?? "App";
            var appConfigPath = Path.Combine(appDataFolder, $"{assemblyName}.Config.data");

            if (!Directory.Exists(appDataFolder))
                Directory.CreateDirectory(appDataFolder);

            return appConfigPath;
        }
    }

    /// <summary>
    /// Gets or sets the serialization binder for type conversion.
    /// </summary>
    public SerializationBinder? ConfigurationConverter { private get; set; }
}

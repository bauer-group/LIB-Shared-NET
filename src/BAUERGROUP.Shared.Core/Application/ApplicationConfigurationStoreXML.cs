#nullable enable

using BAUERGROUP.Shared.Core.Application.Interface;
using BAUERGROUP.Shared.Core.Extensions;

namespace BAUERGROUP.Shared.Core.Application;

/// <summary>
/// XML-based configuration store for application settings.
/// </summary>
/// <typeparam name="T">The configuration type.</typeparam>
public class ApplicationConfigurationStoreXML<T> : IApplicationConfigurationStore<T> where T : new()
{
    private T? _configuration;
    private readonly string _fileName;

    /// <summary>
    /// Creates a new XML configuration store.
    /// </summary>
    /// <param name="fileName">The configuration file path. If null, uses default location.</param>
    public ApplicationConfigurationStoreXML(string? fileName = null)
    {
        _fileName = fileName ?? DefaultConfigurationFilename;
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

        using var reader = new StreamReader(_fileName);
        var xmlText = reader.ReadToEnd();

        return xmlText.DeserializeFromXML<T>() ?? new T();
    }

    private void Save(T value)
    {
        var directory = Path.GetDirectoryName(_fileName);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var writer = new StreamWriter(_fileName);
        writer.Write(value.SerializeToXML<T>());
        writer.Flush();
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

    private static string DefaultConfigurationFilename
    {
        get
        {
            var appDataFolder = ApplicationFolders.ExecutionAutomaticApplicationDataFolder;
            // The folder is created on Save, so an unwritable location fails when writing, not in the constructor
            return Path.Combine(appDataFolder, $"{ApplicationProperties.Name}.Configuration.xml");
        }
    }
}

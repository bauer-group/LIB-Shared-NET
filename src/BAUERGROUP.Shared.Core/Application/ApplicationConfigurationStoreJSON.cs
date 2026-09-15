#nullable enable

using BAUERGROUP.Shared.Core.Application.Interface;
using BAUERGROUP.Shared.Core.Extensions;

namespace BAUERGROUP.Shared.Core.Application;

/// <summary>
/// JSON-based configuration store for application settings.
/// </summary>
/// <typeparam name="T">The configuration type.</typeparam>
public class ApplicationConfigurationStoreJSON<T> : IApplicationConfigurationStore<T> where T : new()
{
    private T? _configuration;
    private readonly string _fileName;
    private readonly bool _includeTypes;

    /// <summary>
    /// Creates a new JSON configuration store.
    /// </summary>
    /// <param name="fileName">The configuration file path. If null, uses default location.</param>
    /// <param name="includeTypes">Whether to include type information in JSON.</param>
    public ApplicationConfigurationStoreJSON(string? fileName, bool includeTypes)
    {
        _fileName = fileName ?? DefaultConfigurationFilename;
        _includeTypes = includeTypes;
    }

    /// <summary>
    /// Creates a new JSON configuration store.
    /// </summary>
    /// <param name="fileName">The configuration file path. If null, uses default location.</param>
    public ApplicationConfigurationStoreJSON(string? fileName)
        : this(fileName, true)
    {
    }

    /// <summary>
    /// Creates a new JSON configuration store with default location.
    /// </summary>
    /// <param name="includeTypes">Whether to include type information in JSON.</param>
    public ApplicationConfigurationStoreJSON(bool includeTypes)
        : this(null, includeTypes)
    {
    }

    /// <summary>
    /// Creates a new JSON configuration store with default settings.
    /// </summary>
    public ApplicationConfigurationStoreJSON()
        : this(true)
    {
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

        using var fileStream = File.OpenRead(_fileName);
        var objectData = fileStream.DeserializeFromJSON<T>();

        return objectData ?? new T();
    }

    private void Save(T value)
    {
        var directory = Path.GetDirectoryName(_fileName);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var fileStream = new FileStream(_fileName, FileMode.Create, FileAccess.Write);
        value.SerializeToJSON(fileStream);
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
            return Path.Combine(appDataFolder, $"{ApplicationProperties.Name}.Configuration.json");
        }
    }
}

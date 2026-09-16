using System.Windows;
using System.Windows.Media;

namespace BAUERGROUP.Shared.Test.Desktop;

/// <summary>
/// Regression tests for the embedded browser toolbar glyphs.
/// <para>
/// The controls used to reference PNG resources that never existed in the repository.
/// <see cref="System.Windows.Controls.Image"/> creates its source lazily, so the missing
/// files were swallowed at draw time and every toolbar button simply rendered empty.
/// The glyphs are now geometries in a merged <see cref="ResourceDictionary"/>, which the
/// controls resolve eagerly while parsing - a missing dictionary or a renamed key now
/// throws instead of failing silently, and these tests pin that contract.
/// </para>
/// </summary>
public class BrowserIconResourcesTests
{
    private const string DictionaryUri =
        "/BAUERGROUP.Shared.Desktop.Browser;component/Internet/BrowserIcons.xaml";

    /// <summary>The authoring grid every glyph is drawn on.</summary>
    private const double IconGrid = 16d;

    /// <summary>Every key the two browser controls reference with <c>{StaticResource ...}</c>.</summary>
    private static readonly string[] ExpectedKeys =
    [
        "BrowserIconBack",
        "BrowserIconForward",
        "BrowserIconReload",
        "BrowserIconBrowse",
        "BrowserIconPrint"
    ];

    public static TheoryData<string> IconKeys => [.. ExpectedKeys];

    static BrowserIconResourcesTests()
    {
        // A plain test host never starts WPF's application plumbing, so the "pack" URI
        // scheme stays unregistered until the resource machinery is touched once. Without
        // this, ResourceDictionary.Source throws NotSupportedException ("The URI prefix is
        // not recognized") in whichever test happens to run first.
        _ = Application.GetResourceStream(new Uri(DictionaryUri, UriKind.Relative));
    }

    private static ResourceDictionary LoadDictionary() =>
        new() { Source = new Uri(DictionaryUri, UriKind.Relative) };

    [Fact]
    public void IconDictionary_IsEmbeddedInTheBrowserAssembly()
    {
        var resource = Application.GetResourceStream(new Uri(DictionaryUri, UriKind.Relative));

        resource.Should().NotBeNull("the icon dictionary must ship inside the assembly");
        resource!.Stream.Should().NotBeNull();
    }

    [Fact]
    public void IconDictionary_ExposesEveryGlyphTheControlsBindTo()
    {
        var dictionary = LoadDictionary();

        var keys = dictionary.Keys.Cast<object>().Select(key => key.ToString()).ToArray();

        keys.Should().BeEquivalentTo(ExpectedKeys);
    }

    [Theory]
    [MemberData(nameof(IconKeys))]
    public void Glyph_IsAGeometryThatFitsTheAuthoringGrid(string key)
    {
        var dictionary = LoadDictionary();

        dictionary[key].Should().BeAssignableTo<Geometry>(
            "the controls bind Path.Data to this key");

        var bounds = ((Geometry)dictionary[key]).Bounds;

        bounds.IsEmpty.Should().BeFalse("an empty geometry paints nothing at all");
        bounds.Width.Should().BeGreaterThan(0);
        bounds.Height.Should().BeGreaterThan(0);

        // Stretch="None" means anything outside the 16x16 box is clipped by the button.
        bounds.Left.Should().BeGreaterThanOrEqualTo(0);
        bounds.Top.Should().BeGreaterThanOrEqualTo(0);
        bounds.Right.Should().BeLessThanOrEqualTo(IconGrid);
        bounds.Bottom.Should().BeLessThanOrEqualTo(IconGrid);
    }

    [Fact]
    public void MissingDictionary_FailsLoudly()
    {
        // Guards the premise of the fix: unlike the old Image.Source, a broken resource
        // reference now surfaces as an exception rather than an empty button.
        var load = () => new ResourceDictionary
        {
            Source = new Uri(
                "/BAUERGROUP.Shared.Desktop.Browser;component/Internet/NoSuchIcons.xaml",
                UriKind.Relative)
        };

        load.Should().Throw<IOException>("a missing resource must not resolve to an empty dictionary");
    }
}

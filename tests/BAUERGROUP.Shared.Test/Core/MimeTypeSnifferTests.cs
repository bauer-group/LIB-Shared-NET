using BAUERGROUP.Shared.Core.Files;

namespace BAUERGROUP.Shared.Test.Core;

public class MimeTypeSnifferTests
{
    [Fact]
    public void GetMime_WithPngHeader_ReturnsPngMimeType()
    {
        // PNG file signature: 89 50 4E 47 0D 0A 1A 0A
        var pngHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        var result = MimeTypeSniffer.GetMime(pngHeader);

        result.Should().Be("image/png");
    }

    [Fact]
    public void GetMime_WithJpegHeader_ReturnsJpegMimeType()
    {
        // JPEG file signature: FF D8 FF
        var jpegHeader = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46 };

        var result = MimeTypeSniffer.GetMime(jpegHeader);

        result.Should().Be("image/jpeg");
    }

    [Fact]
    public void GetMime_WithGifHeader_ReturnsGifMimeType()
    {
        // GIF file signature: 47 49 46 38 (GIF89a)
        var gifHeader = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 };

        var result = MimeTypeSniffer.GetMime(gifHeader);

        result.Should().Be("image/gif");
    }

    [Fact]
    public void GetMime_WithPdfHeader_ReturnsPdfMimeType()
    {
        // PDF file signature: 25 50 44 46 (%PDF)
        var pdfHeader = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E };

        var result = MimeTypeSniffer.GetMime(pdfHeader);

        result.Should().Be("application/pdf");
    }

    [Fact]
    public void GetMime_WithUnknownHeader_ReturnsOctetStream()
    {
        // Random bytes that don't match any known signature
        var unknownHeader = new byte[] { 0x12, 0x34, 0x56, 0x78 };

        var result = MimeTypeSniffer.GetMime(unknownHeader);

        result.Should().Be("application/octet-stream");
    }

    [Fact]
    public void GetMime_WithEmptyArray_ReturnsUnknown()
    {
        var emptyHeader = Array.Empty<byte>();

        var result = MimeTypeSniffer.GetMime(emptyHeader);

        result.Should().Be("unknown/unknown");
    }

    [Fact]
    public void GetMime_WithHtmlContent_ReturnsHtmlMimeType()
    {
        // HTML content starts with <html>
        var htmlBytes = System.Text.Encoding.ASCII.GetBytes("<html><head></head><body></body></html>");

        var result = MimeTypeSniffer.GetMime(htmlBytes);

        result.Should().Be("text/html");
    }

    [Fact]
    public void GetMime_WithZipHeader_ReturnsZipMimeType()
    {
        // ZIP file signature: 50 4B 03 04
        var zipHeader = new byte[] { 0x50, 0x4B, 0x03, 0x04 };

        var result = MimeTypeSniffer.GetMime(zipHeader);

        result.Should().Be("application/zip");
    }

    [Fact]
    public void GetMime_WithBmpHeader_ReturnsBmpMimeType()
    {
        // BMP file signature: 42 4D (BM)
        var bmpHeader = new byte[] { 0x42, 0x4D, 0x00, 0x00, 0x00, 0x00 };

        var result = MimeTypeSniffer.GetMime(bmpHeader);

        result.Should().Be("image/bmp");
    }

    [Fact]
    public void GetMime_WithWebpHeader_ReturnsWebpMimeType()
    {
        // WebP file signature: RIFF....WEBP
        var webpHeader = new byte[] { 0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50 };

        var result = MimeTypeSniffer.GetMime(webpHeader);

        result.Should().Be("image/webp");
    }

    [Fact]
    public void GetMime_WithMp3Header_ReturnsMp3MimeType()
    {
        // MP3 with ID3 tag
        var mp3Header = new byte[] { 0x49, 0x44, 0x33, 0x04, 0x00, 0x00 };

        var result = MimeTypeSniffer.GetMime(mp3Header);

        result.Should().Be("audio/mpeg");
    }

    [Fact]
    public void GetMime_WithGzipHeader_ReturnsGzipMimeType()
    {
        // Gzip signature: 1F 8B
        var gzipHeader = new byte[] { 0x1F, 0x8B, 0x08, 0x00 };

        var result = MimeTypeSniffer.GetMime(gzipHeader);

        result.Should().Be("application/gzip");
    }

    [Fact]
    public void GetMime_WithXmlHeader_ReturnsXmlMimeType()
    {
        // XML declaration
        var xmlBytes = System.Text.Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");

        var result = MimeTypeSniffer.GetMime(xmlBytes);

        result.Should().Be("application/xml");
    }

    [Fact]
    public void GetMime_WithExeHeader_ReturnsExeMimeType()
    {
        // EXE/DLL signature: MZ
        var exeHeader = new byte[] { 0x4D, 0x5A, 0x90, 0x00 };

        var result = MimeTypeSniffer.GetMime(exeHeader);

        result.Should().Be("application/x-msdownload");
    }

    [Fact]
    public void GetMime_WithWoffHeader_ReturnsWoffMimeType()
    {
        // WOFF signature: wOFF
        var woffHeader = new byte[] { 0x77, 0x4F, 0x46, 0x46 };

        var result = MimeTypeSniffer.GetMime(woffHeader);

        result.Should().Be("font/woff");
    }

    [Fact]
    public void GetMimeOrDefault_WithEmptyArray_ReturnsOctetStream()
    {
        var emptyHeader = Array.Empty<byte>();

        var result = MimeTypeSniffer.GetMimeOrDefault(emptyHeader);

        result.Should().Be("application/octet-stream");
    }

    [Fact]
    public void GetMime_WithPlainText_ReturnsTextPlain()
    {
        var textBytes = System.Text.Encoding.UTF8.GetBytes("Hello, this is plain text content.");

        var result = MimeTypeSniffer.GetMime(textBytes);

        result.Should().Be("text/plain");
    }
}

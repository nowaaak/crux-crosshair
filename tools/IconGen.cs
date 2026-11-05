#:property TargetFramework=net10.0-windows
#:property UseWPF=true
#:property PublishAot=false

using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

const string ThemePath = "src/Crux.App/Theme.xaml";
const string AssetsDirectory = "src/Crux.App/Assets";
const string DocsDirectory = "docs";
const double DesignSize = 64;
const double CornerRadius = 14;

int[] iconSizes = [16, 20, 24, 32, 40, 48, 64, 256];
var background = Color.FromRgb(23, 23, 23);
var releaseForeground = Color.FromRgb(96, 205, 255);
var devForeground = Color.FromRgb(22, 163, 74);

if (!File.Exists("Crux.sln"))
{
    Console.Error.WriteLine("Run this from the repository root: dotnet run tools/IconGen.cs");
    return 1;
}

var failure = (Exception?)null;
var thread = new Thread(() =>
{
    try
    {
        var geometry = LoadLogoGeometry(ThemePath);
        WriteIcon(Path.Combine(AssetsDirectory, "app.ico"), geometry, releaseForeground);
        WriteIcon(Path.Combine(AssetsDirectory, "app-dev.ico"), geometry, devForeground);
        Directory.CreateDirectory(DocsDirectory);
        File.WriteAllBytes(Path.Combine(DocsDirectory, "logo.png"), RenderPng(geometry, releaseForeground, 256));
    }
    catch (Exception exception)
    {
        failure = exception;
    }
});
thread.SetApartmentState(ApartmentState.STA);
thread.Start();
thread.Join();

if (failure is not null)
{
    Console.Error.WriteLine(failure);
    return 1;
}

Console.WriteLine("Wrote app.ico, app-dev.ico and docs/logo.png");
return 0;

Geometry LoadLogoGeometry(string themePath)
{
    XNamespace xaml = "http://schemas.microsoft.com/winfx/2006/xaml";
    var element = XDocument.Load(themePath).Root!
        .Elements()
        .FirstOrDefault(e => (string?)e.Attribute(xaml + "Key") == "LogoGeometry")
        ?? throw new InvalidOperationException($"LogoGeometry not found in {themePath}");
    var geometry = Geometry.Parse(element.Value.Trim());
    geometry.Freeze();
    return geometry;
}

byte[] RenderPng(Geometry geometry, Color foreground, int size)
{
    var visual = new DrawingVisual();
    using (var context = visual.RenderOpen())
    {
        context.PushTransform(new ScaleTransform(size / DesignSize, size / DesignSize));
        context.DrawRoundedRectangle(new SolidColorBrush(background), null, new Rect(0, 0, DesignSize, DesignSize), CornerRadius, CornerRadius);
        context.DrawGeometry(new SolidColorBrush(foreground), null, geometry);
        context.Pop();
    }

    var bitmap = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
    bitmap.Render(visual);
    var encoder = new PngBitmapEncoder();
    encoder.Frames.Add(BitmapFrame.Create(bitmap));
    using var stream = new MemoryStream();
    encoder.Save(stream);
    return stream.ToArray();
}

void WriteIcon(string path, Geometry geometry, Color foreground)
{
    var images = iconSizes.Select(size => (Size: size, Png: RenderPng(geometry, foreground, size))).ToList();
    using var stream = File.Create(path);
    using var writer = new BinaryWriter(stream);
    writer.Write((ushort)0);
    writer.Write((ushort)1);
    writer.Write((ushort)images.Count);

    var offset = 6 + 16 * images.Count;
    foreach (var image in images)
    {
        writer.Write((byte)(image.Size >= 256 ? 0 : image.Size));
        writer.Write((byte)(image.Size >= 256 ? 0 : image.Size));
        writer.Write((byte)0);
        writer.Write((byte)0);
        writer.Write((ushort)1);
        writer.Write((ushort)32);
        writer.Write(image.Png.Length);
        writer.Write(offset);
        offset += image.Png.Length;
    }

    foreach (var image in images)
    {
        writer.Write(image.Png);
    }
}

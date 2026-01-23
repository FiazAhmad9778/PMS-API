using PdfSharpCore.Fonts;
using System.Runtime.InteropServices;

namespace PMS.API.Application.Common.Helpers;

/// <summary>
/// Custom font resolver for PdfSharpCore that uses system fonts
/// </summary>
public class PdfFontResolver : IFontResolver
{
  public string DefaultFontName => "Arial";

  public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
  {
    // Ensure we never return null
    if (string.IsNullOrEmpty(familyName))
    {
      familyName = DefaultFontName;
    }

    // Map common font names to system fonts
    var fontName = familyName.ToLower();
    
    // For Arial, use the system font name
    if (fontName == "arial")
    {
      if (isBold && isItalic)
        return new FontResolverInfo("Arial#BoldItalic");
      if (isBold)
        return new FontResolverInfo("Arial#Bold");
      if (isItalic)
        return new FontResolverInfo("Arial#Italic");
      return new FontResolverInfo("Arial#Regular");
    }
    
    // Default fallback - use Arial if we can't resolve the requested font
    return new FontResolverInfo("Arial#Regular");
  }

  public byte[]? GetFont(string faceName)
  {
    if (string.IsNullOrEmpty(faceName))
    {
      // Use default font if faceName is null/empty
      faceName = "Arial#Regular";
    }

    // Try to load font from system fonts directory
    string fontPath = GetSystemFontPath(faceName);
    
    if (string.IsNullOrEmpty(fontPath))
    {
      // Try to find any Arial font as fallback
      fontPath = GetSystemFontPath("Arial#Regular");
      
      if (string.IsNullOrEmpty(fontPath))
      {
        // Last resort: try to find any font file in the fonts directory
        var fontsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
        if (Directory.Exists(fontsDirectory))
        {
          var allArialFiles = Directory.GetFiles(fontsDirectory, "*arial*", SearchOption.TopDirectoryOnly);
          if (allArialFiles.Length > 0)
          {
            fontPath = allArialFiles[0];
          }
        }
      }
    }

    if (string.IsNullOrEmpty(fontPath) || !File.Exists(fontPath))
    {
      throw new FileNotFoundException(
        $"Font file not found for '{faceName}'. " +
        $"Please ensure Arial fonts are installed. " +
        $"Fonts directory: {Environment.GetFolderPath(Environment.SpecialFolder.Fonts)}");
    }

    try
    {
      var fontData = File.ReadAllBytes(fontPath);
      
      if (fontData == null || fontData.Length == 0)
      {
        throw new InvalidOperationException(
          $"Font file is null or empty for '{faceName}' at path: {fontPath}");
      }

      return fontData;
    }
    catch (Exception ex) when (!(ex is FileNotFoundException || ex is InvalidOperationException))
    {
      throw new InvalidOperationException(
        $"Error reading font file '{faceName}' from path: {fontPath}. " +
        $"Inner exception: {ex.Message}", ex);
    }
  }

  private string GetSystemFontPath(string faceName)
  {
    if (string.IsNullOrEmpty(faceName))
    {
      return string.Empty;
    }

    // Extract font family and style from faceName (format: "Family#Style")
    var parts = faceName.Split('#');
    var family = parts[0];
    var style = parts.Length > 1 ? parts[1] : "Regular";

    // Map to Windows font paths
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
      var fontsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
      
      if (string.IsNullOrEmpty(fontsDirectory) || !Directory.Exists(fontsDirectory))
      {
        return string.Empty;
      }

      // Common Arial font file names on Windows - prioritize .ttf over .ttc
      var fontFiles = new[]
      {
        "arial.ttf",      // Regular
        "ARIAL.TTF",      // Regular (uppercase)
        "arialbd.ttf",    // Bold
        "ARIALBD.TTF",    // Bold (uppercase)
        "ariali.ttf",     // Italic
        "ARIALI.TTF",     // Italic (uppercase)
        "arialbi.ttf",   // Bold Italic
        "ARIALBI.TTF",   // Bold Italic (uppercase)
      };

      // Try to find the appropriate font file based on style
      foreach (var fontFile in fontFiles)
      {
        var fontPath = Path.Combine(fontsDirectory, fontFile);
        if (File.Exists(fontPath))
        {
          var lowerFontFile = fontFile.ToLower();
          // Match style to font file
          if (style.Contains("Bold") && style.Contains("Italic") && lowerFontFile.Contains("bi"))
            return fontPath;
          if (style.Contains("Bold") && lowerFontFile.Contains("bd") && !lowerFontFile.Contains("bi"))
            return fontPath;
          if (style.Contains("Italic") && lowerFontFile.Contains("i") && !lowerFontFile.Contains("bi") && !lowerFontFile.Contains("bd"))
            return fontPath;
          if (style == "Regular" && lowerFontFile == "arial.ttf")
            return fontPath;
        }
      }

      // Fallback: try .ttc files (TrueType Collection)
      var ttcFiles = new[] { "arial.ttc", "ARIAL.TTC" };
      foreach (var ttcFile in ttcFiles)
      {
        var fontPath = Path.Combine(fontsDirectory, ttcFile);
        if (File.Exists(fontPath))
          return fontPath;
      }

      // Last resort: return the first Arial .ttf file found
      foreach (var fontFile in fontFiles)
      {
        var fontPath = Path.Combine(fontsDirectory, fontFile);
        if (File.Exists(fontPath))
          return fontPath;
      }
    }

    return string.Empty;
  }
}


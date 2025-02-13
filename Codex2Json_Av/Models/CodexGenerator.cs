using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia.Controls;
using System.Threading.Tasks;

namespace Codex2Json_Av.Models
{
    public class MetadataAndContent
    {
        public Dictionary<string, string> Metadata { get; set; }
        public string Content { get; set; }
    }

    public class CodexGenerator
    {
        public MetadataAndContent ExtractMetadataAndContent(string filePath)
        {
            var metadata = new Dictionary<string, string>();
            var content = new List<string>();
            var inMetadata = false;
            var inContent = false;
            var readingTags = false;
            var readingAlias = false;

            foreach ( var line in File.ReadLines(filePath) )
            {
                if ( string.IsNullOrWhiteSpace(line) ) continue;
                if ( line.Trim() == "---" )
                {
                    if ( inMetadata )
                    {
                        inContent = true;
                    }
                    inMetadata = !inMetadata;
                    continue;
                }

                if ( inMetadata )
                {
                    if ( readingAlias && !line.Trim().StartsWith("-") )
                    {
                        readingAlias = false;
                    }
                    if ( readingTags && !line.Trim().StartsWith("-") )
                    {
                        readingTags = false;
                    }
                    if ( line.Trim() == "tags:" )
                    {
                        readingTags = true;
                        metadata["tags"] = "";
                    }
                    else if ( line.Trim() == "aliases:" )
                    {
                        readingAlias = true;
                        metadata["aliases"] = "";
                    }
                    else if ( readingAlias && line.Trim().StartsWith("-") )
                    {
                        if ( !string.IsNullOrEmpty(metadata["aliases"]) ) metadata["aliases"] += ", ";
                        metadata["aliases"] += line.Trim().Replace("-", "").Trim();
                    }
                    else if ( readingTags && line.Trim().StartsWith("-") )
                    {
                        if ( !string.IsNullOrEmpty(metadata["tags"]) ) metadata["tags"] += ", ";
                        metadata["tags"] += line.Trim().Replace("-", "").Trim();
                    }
                    else
                    {
                        var parts = line.Split(':', ( char ) 2).Select(x => x.Trim()).ToArray();
                        if ( parts.Length > 1 )
                        {
                            var key = parts[0];
                            var value = parts[1];
                            if ( key != "tags" )
                            {
                                metadata[key] = value;
                            }
                        }
                    }
                }
                else if ( inContent )
                {
                    content.Add(line.Trim());
                }
            }

            return new MetadataAndContent
            {
                Metadata = metadata,
                Content = string.Join(" ", content)
            };
        }

        public string GenerateJson(string sourceDir)
        {
            var allEntries = new List<MetadataAndContent>();
            foreach ( var path in Directory.GetFiles(sourceDir, "entry.md", SearchOption.AllDirectories) )
            {
                var entryData = ExtractMetadataAndContent(path);
                if ( entryData.Metadata.Any() )
                {
                    allEntries.Add(entryData);
                }
            }
            return JsonSerializer.Serialize(allEntries);
        }

        public async Task<string> Process(string sourceDir, Window parentWindow)
        {
            if ( string.IsNullOrEmpty(sourceDir) || !Directory.Exists(sourceDir) )
            {
                throw new ArgumentException("Source directory not provided or does not exist.");
            }

            var json = GenerateJson(sourceDir);
            string outputFile = Path.Combine(Directory.GetParent(sourceDir).FullName, "codex.json");
            await File.WriteAllTextAsync(outputFile, json);

            // Show a dialog instead of MessageBox
            var dialog = new Window
            {
                Content = new TextBlock { Text = "Codex JSON generated successfully.", Margin = new Avalonia.Thickness(20) },
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            await ShowSuccessDialog(parentWindow);

            return outputFile;
        }

        private async Task ShowSuccessDialog(Window parentWindow)
        {
            var dialog = new Window
            {
                Content = new TextBlock { Text = "Codex JSON generated successfully.", Margin = new Avalonia.Thickness(20) },
                Width = 300,
                Height = 150,
                Title = "Codex2Json Success",
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if ( parentWindow.IsVisible ) // ✅ Ensure the parent window is visible
            {
                await dialog.ShowDialog(parentWindow);
            }
            else
            {
                dialog.Show(); // Fallback in case parent is invisible
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Data;

[AttributeUsage(AttributeTargets.Class)]
internal sealed class FileAttribute : Attribute
{
    public readonly string DefaultPath;
    public readonly string Extension;

    public FileAttribute(string defaultPath, string extension = "xml")
    {
        DefaultPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + defaultPath;
        Extension = extension;
    }

    public static FileAttribute GetAttribute<TFile>()
        where TFile : FileBase<TFile>, new()
    {
        return typeof(TFile).GetCustomAttribute<FileAttribute>() ?? throw new InvalidOperationException("A class that inherits from FileBase must implement FileAttribute.");
    }
}

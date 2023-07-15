using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Data;

/// <summary>
/// The base for a collection of files
/// </summary>
/// <typeparam name="TCollection">The collection</typeparam>
/// <typeparam name="TFile">The files</typeparam>
public abstract class FileCollectionBase<TCollection, TFile> : IEnumerable<TFile>
    where TCollection : FileCollectionBase<TCollection, TFile>, new()
    where TFile : FileBase<TFile>, new()
{
    /// <summary>
    /// The save path of this collection
    /// </summary>
    public string SavePath { get => _savePath; }
    private readonly string _savePath = FileBase<TFile>.s_Attribute.DefaultPath;

    private readonly List<TFile> _list = new();

    /// <summary>
    /// Get a file by the name
    /// </summary>
    /// <param name="name">The filename</param>
    /// <returns>The file</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the file don't exist</exception>
    public virtual TFile this[string name] => _list.FirstOrDefault(file => file.Name == name) ?? throw new KeyNotFoundException("The file was not found in the collection");

    static FileCollectionBase()
    {
        string path = new TCollection().SavePath;
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    /// <summary>
    /// Add a file
    /// </summary>
    /// <param name="name">The name of the file</param>
    /// <returns>The file</returns>
    /// <exception cref="ArgumentException">Thrown when a file with this name already exist</exception>
    public virtual TFile Add(string name)
    {
        if (_list.Any(file => file.Name == name))
            throw new ArgumentException("The name already exist");

        TFile file = FileBase<TFile>.Create((SavePath.EndsWith('\\') ? SavePath : SavePath + "\\") + name);
        _list.Add(file);
        return file;
    }

    /// <summary>
    /// Delete a file of the collection
    /// </summary>
    /// <param name="name"></param>
    public virtual void Remove(string name)
    {
        this[name].Delete();
        _list.Remove(this[name]);
    }

    /// <summary>
    /// Load a collection from a folder
    /// </summary>
    /// <param name="loadPath">The folder (<see langword="null"/> is the default path of <typeparamref name="TFile"/>)</param>
    /// <returns>The collection</returns>
    public static TCollection LoadCollection()
    {
        TCollection collection = new();
        foreach (string path in Directory.EnumerateFiles(collection.SavePath))
        {
            TFile file = FileBase<TFile>.Load(path);
            collection._list.Add(file);
        }

        return collection;
    }

    public void ReloadCollection()
    {
        _list.Clear();
        foreach (string path in Directory.EnumerateFiles(SavePath))
        {
            TFile file = FileBase<TFile>.Load(path);
            _list.Add(file);
        }
    }

    IEnumerator<TFile> IEnumerable<TFile>.GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _list.GetEnumerator();
    }
}

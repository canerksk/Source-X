using System.Text;

namespace SphereServer.Scripting;

/// <summary>
/// Script parser for reading Sphere script files (.scp).
/// Based on Source-X CScript.
/// </summary>
public class ScriptParser : IDisposable
{
    private StreamReader? _reader;
    private string? _currentLine;
    private int _lineNumber;

    public ScriptParser(string filePath)
    {
        FilePath = filePath;
        _lineNumber = 0;
    }

    public string FilePath { get; }
    public int LineNumber => _lineNumber;
    public string? CurrentLine => _currentLine;

    public bool Open()
    {
        try
        {
            _reader = new StreamReader(FilePath, Encoding.UTF8);
            _lineNumber = 0;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Close()
    {
        _reader?.Close();
        _reader = null;
    }

    public bool ReadLine()
    {
        if (_reader == null)
            return false;

        string? line = _reader.ReadLine();
        if (line == null)
            return false;

        _lineNumber++;
        _currentLine = line.Trim();

        // Skip comments and empty lines
        if (string.IsNullOrWhiteSpace(_currentLine) || _currentLine.StartsWith("//"))
        {
            return ReadLine(); // Recursively read next line
        }

        // Remove inline comments
        int commentPos = _currentLine.IndexOf("//");
        if (commentPos >= 0)
        {
            _currentLine = _currentLine[..commentPos].Trim();
        }

        return true;
    }

    public bool ReadKey(out string key, out string value)
    {
        key = string.Empty;
        value = string.Empty;

        if (_currentLine == null)
            return false;

        int equalPos = _currentLine.IndexOf('=');
        if (equalPos < 0)
        {
            key = _currentLine.Trim();
            return true;
        }

        key = _currentLine[..equalPos].Trim();
        value = _currentLine[(equalPos + 1)..].Trim();
        return true;
    }

    public ScriptBlock? ReadBlock()
    {
        while (ReadLine())
        {
            if (_currentLine!.StartsWith('[') && _currentLine.EndsWith(']'))
            {
                string blockName = _currentLine[1..^1].Trim();
                return new ScriptBlock(blockName, this);
            }
        }

        return null;
    }

    public void Dispose()
    {
        Close();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Script block reader.
/// Based on Source-X CResourceDef.
/// </summary>
public class ScriptBlock
{
    private readonly ScriptParser _parser;
    private readonly Dictionary<string, string> _properties;

    public ScriptBlock(string name, ScriptParser parser)
    {
        Name = name;
        _parser = parser;
        _properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        ReadProperties();
    }

    public string Name { get; }
    public IReadOnlyDictionary<string, string> Properties => _properties;

    private void ReadProperties()
    {
        while (_parser.ReadLine())
        {
            string? line = _parser.CurrentLine;
            if (string.IsNullOrEmpty(line))
                continue;

            // New block starts
            if (line.StartsWith('['))
                break;

            if (_parser.ReadKey(out string key, out string value))
            {
                if (!string.IsNullOrEmpty(key))
                {
                    _properties[key] = value ?? string.Empty;
                }
            }
        }
    }

    public string? GetProperty(string key, string? defaultValue = null)
    {
        return _properties.TryGetValue(key, out string? value) ? value : defaultValue;
    }

    public int GetPropertyInt(string key, int defaultValue = 0)
    {
        if (_properties.TryGetValue(key, out string? value))
        {
            if (int.TryParse(value, out int result))
                return result;

            // Try hex
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ||
                value.StartsWith("0X", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(value[2..], System.Globalization.NumberStyles.HexNumber, null, out result))
                    return result;
            }
        }

        return defaultValue;
    }

    public bool GetPropertyBool(string key, bool defaultValue = false)
    {
        if (_properties.TryGetValue(key, out string? value))
        {
            if (bool.TryParse(value, out bool result))
                return result;

            // Check for 1/0
            if (int.TryParse(value, out int intValue))
                return intValue != 0;
        }

        return defaultValue;
    }

    public override string ToString()
    {
        return $"[{Name}] ({_properties.Count} properties)";
    }
}

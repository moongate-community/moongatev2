using System.Text.RegularExpressions;
using Moongate.UO.Data.Types;

namespace Moongate.UO.Data.Localization;

public sealed class StringEntry
{
    private string _text;

    public int Number { get; }

    public string Text
    {
        get => _text;
        set => _text = value ?? string.Empty;
    }

    public CliLocFlag Flag { get; set; }

    public StringEntry(int number, string text, byte flag)
    {
        Number = number;
        _text = text;
        Flag = (CliLocFlag)flag;
    }

    public StringEntry(int number, string text, CliLocFlag flag)
    {
        Number = number;
        _text = text;
        Flag = flag;
    }

    // Razor
    private static readonly Regex _regEx = new(
        @"~(\d+)[_\w]+~",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant
    );

    private string _fmtTxt;
    private static readonly object[] _args = { "", "", "", "", "", "", "", "", "", "", "" };

    public string Format(params object[] args)
    {
        if (_fmtTxt == null)
        {
            _fmtTxt = _regEx.Replace(_text, "{$1}");
        }

        for (var i = 0; i < args.Length && i < 10; i++)
        {
            _args[i + 1] = args[i];
        }

        return string.Format(_fmtTxt, _args);
    }

    public string SplitFormat(string argString)
    {
        if (_fmtTxt == null)
        {
            _fmtTxt = _regEx.Replace(_text, "{$1}");
        }

        var args = argString.Split('\t'); // adds an extra on to the args array

        for (var i = 0; i < args.Length && i < 10; i++)
        {
            _args[i + 1] = args[i];
        }

        return string.Format(_fmtTxt, _args);
    }

    public override string ToString()
        => $"{Number} - {Text} ({Flag})";
}

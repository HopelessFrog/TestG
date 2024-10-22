
namespace TestG.Models;

public class TestCase
{
    public bool Use { get; set; }
    public bool IsPositive { get; set; }
    public string ArgumentLine { get; set; }
    public bool IsPassed { get; set; }
    public string? Result { get; set; }
    public string? Expected { get; set; }
    public string? Diff { get; set; } = null;
    public string Eps { get; set; }
    public string TestHeader { get { return $"TEST {Hash} {(IsPositive ? "Positive" : "Negative")}\n{ArgumentLine}\n{Eps}\n{Expected}"; } }

    private string Hash => GetHashCode().ToString();

    public override bool Equals(object? obj)
    {
        return obj is TestCase @case &&
               Use == @case.Use &&
               IsPositive == @case.IsPositive &&
               ArgumentLine == @case.ArgumentLine &&
               IsPassed == @case.IsPassed &&
               Result == @case.Result &&
               Expected == @case.Expected &&
               Diff == @case.Diff &&
               Eps == @case.Eps &&
               TestHeader == @case.TestHeader;
    }

    public override int GetHashCode()
    {
        HashCode hash = new HashCode();

        hash.Add(IsPositive);
        hash.Add(ArgumentLine);
        hash.Add(Expected);
        hash.Add(Eps);

        return hash.ToHashCode();
    }

    public override string ToString()
    {
        string isNeg = IsPositive ? "Positive" : "Negative";
        string isPas = IsPassed ? "PASSED" : "!!!NOT PASSED";
        var hash = GetHashCode().ToString();
        return $"TEST {hash} {isNeg}\n{ArgumentLine}\n{Eps}\n{Expected}\n{Result}{(Diff is null ? "\n" : $"\n{Diff}\n")}{isPas}\n\n";
    }
}

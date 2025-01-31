using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Lineri.ESS.Core;


namespace Benchmarks;

public class Program
{
    static void Main(string[] args)
    {
        var v = BenchmarkRunner.Run<Program>();
    }
    
    [Benchmark]
    public void CheckIdentityToDictionaryL()
    {
        var v = new Audio[1280];
        var ld = new ListAudio(64);
        
        for (int i = 0; i < 1280; i++)
        {
            v[i] = new Audio();
        }
        
        for (int i = 0; i < 1280; i++)
        {
            ld.Add(v[i]);
        }

        var r = new Random();
        for(int i = 0; i < 1280; i++)
        {
            ld[r.Next(0, 1280)].Persist = true;
        }
    }
    [Benchmark]
    public void CheckIdentityToDictionaryD()
    {
        var v = new Audio[1280];
        var ld = new Dictionary<int, Audio>(64);
        
        for (int i = 0; i < 1280; i++)
        {
            v[i] = new Audio();
        }
        
        for (int i = 0; i < 1280; i++)
        {
            ld.Add(i, v[i]);
        }
        
        var r = new Random();
        for(int i = 0; i < 1280; i++)
        {
            ld[r.Next(0, 1280)].Persist = true;
        }
    }
}
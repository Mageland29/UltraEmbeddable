using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;

public static class Embed
{
    public static void SetupResources()
    {
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
    }
    private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
        try
        {
            string dllName = args.Name.Contains(',') ? args.Name.Substring(0, args.Name.IndexOf(',')) : args.Name.Replace(".dll", "");
            if (dllName.EndsWith("_resources")) return null;
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(dllName))
            {
                byte[] Data = new byte[stream.Length];
                stream.Read(Data, 0, Data.Length);
                return Assembly.Load(Decompress(Data));
            }
        }
        catch
        {
            return null;
        }
    }
    public static byte[] Decompress(byte[] data)
    {
        MemoryStream input = new MemoryStream(data);
        MemoryStream output = new MemoryStream();
        using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
        {
            dstream.CopyTo(output);
        }
        return output.ToArray();
    }
}
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using static Logger;

public static class Context
{
    public static ModuleDefMD module = null;
    public static string FileName = null;
    public static void LoadModule(string filename)
    {
        try
        {
            FileName = filename;
            byte[] data = File.ReadAllBytes(filename);
            ModuleContext modCtx = ModuleDef.CreateModuleContext();
            module = ModuleDefMD.Load(data, modCtx);
            Write("Module Loaded : " + module.Name, TypeMessage.Info);
            foreach (AssemblyRef dependance in module.GetAssemblyRefs())
            {
                Write($"Dependance : {dependance.Name}", TypeMessage.Info);
            }
        }
        catch
        {
            Write("Error for Loade Module", TypeMessage.Error);
        }
    }
    public static void SaveModule()
    {
        try
        {
            string filename = string.Concat(new string[] { Path.GetDirectoryName(FileName), "\\", Path.GetFileNameWithoutExtension(FileName), "_Embedded", Path.GetExtension(FileName) });
            if (module.IsILOnly)
            {
                ModuleWriterOptions writer = new ModuleWriterOptions(module);
                writer.MetaDataOptions.Flags = MetaDataFlags.PreserveAll;
                writer.MetaDataLogger = DummyLogger.NoThrowInstance;
                module.Write(filename, writer);
            }
            else
            {
                NativeModuleWriterOptions writer = new NativeModuleWriterOptions(module);
                writer.MetaDataOptions.Flags = MetaDataFlags.PreserveAll;
                writer.MetaDataLogger = DummyLogger.NoThrowInstance;
                module.NativeWrite(filename, writer);
            }
            Write("File Embedded Saved : " + filename, TypeMessage.Done);
        }
        catch (ModuleWriterException ex)
        {
            Write("Fail to save current module\n" + ex.ToString(), TypeMessage.Error);
        }
        Console.ReadLine();
    }
    public static byte[] Compress(byte[] data)
    {
        MemoryStream output = new MemoryStream();
        using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal))
        {
            dstream.Write(data, 0, data.Length);
        }
        return output.ToArray();
    }
    public static void RunPhase()
    {
        Write("Adding dlls in progress ...", TypeMessage.Debug);
        MethodDef cctor = module.GlobalType.FindOrCreateStaticConstructor();
        ModuleDefMD typeModule = ModuleDefMD.Load(typeof(Embed).Module);
        TypeDef typeDef = typeModule.ResolveTypeDef(MDToken.ToRID(typeof(Embed).MetadataToken));
        IEnumerable<IDnlibDef> members = InjectHelper.Inject(typeDef, module.GlobalType, module);
        MethodDef init = (MethodDef)members.Single(method => method.Name == "SetupResources");
        cctor.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, init));
        string[] refs = Directory.GetFiles(Path.GetDirectoryName(FileName), "*.dll");
        foreach (string reference in refs)
        {
            byte[] array = File.ReadAllBytes(reference);
            module.Resources.Add(new EmbeddedResource(Path.GetFileNameWithoutExtension(reference), Compress(array)));
        }
    }
    public static void Welcome()
    {
        Console.Title = "UltraEmbeddable Console 1.0";
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(@"Made By Sir-_-MaGeLanD#7358");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(@"                         __          __  _                                  _______    ");
        Console.WriteLine(@"                         \ \        / / | |                                |__   __|   ");
        Console.WriteLine(@"                          \ \  /\  / /__| | ___ ___  _ __ ___   ___           | | ___  ");
        Console.WriteLine(@"                           \ \/  \/ / _ \ |/ __/ _ \| '_ ` _ \ / _ \          | |/ _ \ ");
        Console.WriteLine(@"                            \  /\  /  __/ | (_| (_) | | | | | |  __/          | | (_) |");
        Console.WriteLine(@"                             \/  \/ \___|_|\___\___/|_| |_| |_|\___|          |_|\___/ ");
        Console.WriteLine(@"                   _    _ _ _             ______           _              _     _       _     _      ");
        Console.WriteLine(@"                  | |  | | | |           |  ____|         | |            | |   | |     | |   | |     ");
        Console.WriteLine(@"                  | |  | | | |_ _ __ __ _| |__   _ __ ___ | |__   ___  __| | __| | __ _| |__ | | ___ ");
        Console.WriteLine(@"                  | |  | | | __| '__/ _` |  __| | '_ ` _ \| '_ \ / _ \/ _` |/ _` |/ _` | '_ \| |/ _ \");
        Console.WriteLine(@"                  | |__| | | |_| | | (_| | |____| | | | | | |_) |  __/ (_| | (_| | (_| | |_) | |  __/"); 
        Console.WriteLine(@"                   \____/|_|\__|_|  \__,_|______|_| |_| |_|_.__/ \___|\__,_|\__,_|\__,_|_.__/|_|\___|");
        Console.WriteLine(Environment.NewLine);
        Console.WriteLine(Environment.NewLine);
        Console.WriteLine(Environment.NewLine);
    }
}

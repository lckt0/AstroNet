using dnlib.DotNet;
using KVM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace AstroNet
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        internal static string filepath;

        private void button1_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(filepath) || String.IsNullOrWhiteSpace(filepath))
            {
                MessageBox.Show("Enter valid path!", "Error!");
            }
            else
            {
                if (File.Exists(filepath))
                {
                    try
                    {
                        List<string> methodList = new List<string>();

                        foreach (var item in checkedListBox1.CheckedItems)
                        {
                            methodList.Add(item.ToString());
                        }

                        new KVMTask().Exceute(ModuleDefMD.Load(filepath), filepath + "_VM.exe", "", null, methodList);
                        MessageBox.Show("Done!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Error!");
                    }
                }
                else
                {
                    MessageBox.Show("File doesn't exist!", "Error!");
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            filepath = textBox1.Text;
            if (!String.IsNullOrEmpty(filepath) && !String.IsNullOrWhiteSpace(filepath) && File.Exists(filepath))
            {
                //button3.Invoke(null);
            }
        }
        private void textBox1DragEnter(object sender, DragEventArgs e)
        {
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    e.Effect = DragDropEffects.Copy;
                else
                    e.Effect = DragDropEffects.None;
            }

        }
        private void textBox1DragDrop(object sender, DragEventArgs e)
        {
            {
                try
                {
                    Array a = (Array)e.Data.GetData(DataFormats.FileDrop);
                    if (a != null)
                    {
                        string s = a.GetValue(0).ToString();
                        int lastoffsetpoint = s.LastIndexOf(".");
                        if (lastoffsetpoint != -1)
                        {
                            string Extension = s.Substring(lastoffsetpoint);
                            Extension = Extension.ToLower();
                            if (Extension == ".exe" || Extension == ".dll")
                            {
                                this.Activate();
                                textBox1.Text = s;
                                int lastslash = s.LastIndexOf("\\");
                            }
                        }
                    }
                }
                catch
                {

                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(filepath) || String.IsNullOrWhiteSpace(filepath))
            {
                MessageBox.Show("Enter valid path!", "Error!");
            }
            else
            {
                if (File.Exists(filepath))
                {
                    try
                    {
                        ModuleDefMD moduleDef = ModuleDefMD.Load(textBox1.Text);
                        foreach (TypeDef type in moduleDef.Types)
                        {
                            foreach (MethodDef method in type.Methods)
                            {
                                checkedListBox1.Items.Add(method.Name);
                            }
                        }
                        textBox1.Enabled = false;
                        button3.Enabled = false;
                        button1.Enabled = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Error!");
                    }
                }
                else
                {
                    MessageBox.Show("File doesn't exist!", "Error!");
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, true);
            }
        }
    }
}

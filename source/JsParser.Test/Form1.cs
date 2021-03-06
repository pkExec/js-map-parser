﻿using JsParser.UI.Properties;
using JsParserTest.Helpers;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace JsParser.Test
{
    public partial class Form1 : Form
    {
        private JsParser.Core.Infrastructure.JsParserService _jsParserService;

        public Form1()
        {
            InitializeComponent();

            _jsParserService = new JsParser.Core.Infrastructure.JsParserService(Settings.Default);
        }

        private void InitTree(string resName)
        {
            var codeProvider = new SimpleCodeProvider(txtSource, string.Empty, resName);

            var result = _jsParserService.Process(codeProvider);

            navigationTreeView1.UpdateTree(result, codeProvider);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var tests = Assembly.GetAssembly(GetType()).GetManifestResourceNames()
                .Where(name => name.EndsWith(".js")
                    || name.EndsWith(".htm")
                    || name.EndsWith(".aspx")
                    || name.EndsWith(".cshtml")
                    )
                .OrderBy(n => n);

            foreach (var testname in tests)
            {
                var nameparts = testname.Split(new[] { '.' });
                var name = nameparts.Skip(3).Aggregate((a, i) => a += "." + i);

                tvTests.Nodes.Add(new TreeNode(name)
                {
                    Tag = testname,
                });
            }

            tvTests.SelectedNode = tvTests.Nodes[0];
        }

        private void loadSelectedTest()
        {
            var selectedNode = tvTests.SelectedNode;
            if (selectedNode != null)
            {
                if (selectedNode.Text.StartsWith("file: "))
                {
                    var testCode = File.ReadAllText(selectedNode.Tag.ToString());
                    txtSource.Text = testCode;
                }
                else
                {
                    var resName = (string)selectedNode.Tag;
                    var testCode = TestsHelper.GetEmbeddedText(resName);
                    txtSource.Text = testCode;
                }
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var fileName = openFileDialog1.FileName;
                
                tvTests.Nodes.Insert(0, new TreeNode("file: " + Path.GetFileName(fileName))
                {
                    Tag = fileName,
                });

                tvTests.SelectedNode = tvTests.Nodes[0];
            }
        }

        private void scanDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                var path = folderBrowserDialog1.SelectedPath;
                var ff = new Form_ScanDir_Results(path);
                ff.Show();
            }
        }

        private void tvTests_AfterSelect(object sender, TreeViewEventArgs e)
        {
            loadSelectedTest();
        }

        private void txtSource_TextChanged(object sender, EventArgs e)
        {
            var selectedNode = tvTests.SelectedNode;
            if (selectedNode != null)
            {
                var resName = (string)selectedNode.Tag;
                InitTree(resName);
            }
        }
    }
}

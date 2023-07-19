using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace JsonFileEditor
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            ReadConfig();
        }
        private void ReadConfig()
        {
            var registryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\JsonEditor");
            if (registryKey != null)
            {
                var strPath = registryKey.GetValue("path");
                if (strPath != null)
                {
                    toolStripTextBoxFolder.Text = strPath.ToString();
                }
                var strExclude = registryKey.GetValue("exclude");
                if (strExclude != null)
                {
                    toolStripTextBoxExclude.Text = strExclude.ToString();
                }
            }
        }

        private void WriteConfig()
        {
            Microsoft.Win32.RegistryKey registryKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\JsonEditor");
            registryKey.SetValue("path", toolStripTextBoxFolder.Text);
            registryKey.SetValue("exclude", toolStripTextBoxExclude.Text);
            registryKey.Close();
        }

        private void OToolStripButton_Click(object sender, EventArgs e)
        {
            string strDataFolder = toolStripTextBoxFolder.Text;
            if (strDataFolder == "")
            {
                strDataFolder = Directory.GetCurrentDirectory();
            }
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = strDataFolder;
            DialogResult dgRes = folderBrowserDialog.ShowDialog();
            if (dgRes == DialogResult.OK)
            {
                toolStripTextBoxFolder.Text = folderBrowserDialog.SelectedPath;
                LoadDataFiles(folderBrowserDialog.SelectedPath);
            }

        }

        private void SToolStripButton_Click(object sender, EventArgs e)
        {
            string strFolder = toolStripTextBoxFolder.Text;
            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                if (col.Index == 0) { continue; }

                string strJsonFile = strFolder + "\\" + col.HeaderText + ".json";
                JObject jData = new JObject();
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.Cells[0].Value == null) { continue; }

                    string key = row.Cells[0].Value.ToString();
                    object objValue = row.Cells[col.Index].Value;
                    object jValue;
                    if (objValue == null) { jValue = ""; }
                    else
                    {
                        jValue = objValue.ToString();
                        if (objValue.ToString().Contains(Environment.NewLine))
                        {
                            string[] separatingStrings = { Environment.NewLine };
                            jValue = objValue.ToString().Split(separatingStrings, StringSplitOptions.None);
                        }
                    }
                    //add to json
                    jData.Add(new JProperty(key, jValue));
                }
                //write to file
                string strJsonData = Convert.ToString(jData);
                File.WriteAllText(strJsonFile, strJsonData, System.Text.Encoding.UTF8);
            }
        }

        private void LoadDataFiles(string strFolder)
        {
            dataGridView1.Columns.Clear();
            int intKey = dataGridView1.Columns.Add("key", "key");
            dataGridView1.Columns[intKey].Resizable = DataGridViewTriState.True;
            dataGridView1.Columns[intKey].Frozen = true;

            string[] jsonFiles = Directory.GetFiles(strFolder, "*.json");
            Dictionary<string, int> dictKeys = new Dictionary<string, int>();//value, intValue
            foreach (string file in jsonFiles)
            {
                if (!file.Contains(toolStripTextBoxExclude.Text))
                {
                    //col
                    string strColName = Path.GetFileNameWithoutExtension(file);
                    dataGridView1.Columns.Add(strColName, strColName);
                    //row
                    string strJSON = File.ReadAllText(file);
                    JObject objJson = JObject.Parse(strJSON);

                    //create table
                    foreach (KeyValuePair<string, JToken> token in objJson)
                    {
                        //read data
                        string key = token.Key;

                        DataGridViewRow row;
                        if (!dictKeys.ContainsKey(key))
                        {
                            row = new DataGridViewRow();
                            DataGridViewTextBoxCell cellValue = new DataGridViewTextBoxCell();
                            cellValue.Value = key;
                            row.Cells.Add(cellValue);
                            int intRow = dataGridView1.Rows.Add(row);
                            dictKeys.Add(key, intRow);
                        }
                    }

                    //write to table
                    foreach (KeyValuePair<string, JToken> token in objJson)
                    {
                        //read data
                        string key = token.Key;
                        string value;
                        if (token.Value == null) { value = ""; }
                        else
                        {
                            if (token.Value.Type == JTokenType.Array)
                            {
                                value = string.Join("\r\n", token.Value);
                            }
                            else
                            {
                                value = token.Value.ToString();
                            }
                        }

                        dataGridView1.Rows[dictKeys[key]].Cells[strColName].Value = value;
                    }
                }
            }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            WriteConfig();
        }

        private void dataGridView1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            var value = dataGridView1.CurrentCell.Value;
            if (value == null)
            {
                textBoxValue.Text = "";
            }
            else
            {
                textBoxValue.Text = dataGridView1.CurrentCell.Value.ToString();
            }
        }
    }
}

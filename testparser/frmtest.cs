using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BASeParser;
using System.Diagnostics;
namespace testparser
{
    public partial class frmtest : Form
    {
        private CParser mParser = new CParser();
        public frmtest()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnexec_Click(object sender, EventArgs e)
        {
            mParser.Expression = txtinput.Text;
            Object resultacquired = null;
            try
            {
                resultacquired = mParser.Execute();
                String stringresult = mParser.ResultToString(resultacquired);
                txtresults.Text += System.Environment.NewLine + stringresult;
            }
            catch (Exception err)
            {
                txtresults.Text += System.Environment.NewLine;
                txtresults.Select(txtresults.Text.Length, 0);
                txtresults.SelectedText = "Exception:" + err.Message + " Source: " + err.Source + System.Environment.NewLine +
                    "Stack trace:" + System.Environment.NewLine + err.StackTrace + System.Environment.NewLine;
                txtresults.SelectionFont= new Font(txtresults.SelectionFont,FontStyle.Bold);
                txtresults.Select(txtresults.Text.Length, 0);
                txtresults.ScrollToCaret();
                



            }
        }

        private void txtresults_TextChanged(object sender, EventArgs e)
        {

        }
        private static bool IsAlpha(String alphastring)
        {
            return alphastring.ToUpper().All((w) => (w >= 65 && w <= 90));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Debug.Print(IsAlpha(txtinput.Text).ToString());
        }
        public void SetText(String newtext)
        {
            Text = newtext;

        }

        private void frmtest_Load(object sender, EventArgs e)
        {
            mParser.Variables.Add("Form", this);
        }
    }
}

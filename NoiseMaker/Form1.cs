using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BASeParser;

namespace NoiseMaker
{
    public partial class Form1 : Form
    {
        private CParser mParser = new CParser();
        private CFunction mFunction;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            mFunction.Expression=txtexpression.Text;
            for (int i = 0; i < int.Parse(txtlength.Text); i++)
            {
                int retval = (int)mFunction.Invoke(new object[]{(object)i});
                Console.Beep(retval, 4);


            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            mFunction = new CFunction(mParser, "Soundfunc", "");
        }
    }
}

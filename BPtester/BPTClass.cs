using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BASeParser;
using System.Diagnostics; 
namespace BPtester
{
    /// <summary>
    /// Summary description for BPTClass
    /// </summary>
    [TestClass]
    public class BPTClass
    {
        public BPTClass()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestOperators()
        {
            CParser testparser = new CParser();

            String[] ExpressionsUse = {"12+5","{1,2,3}","14-12+5*Sqr(64)"};
            Object[] Expectedresults = {(object)17,(object)(new object[] {1f,2f,3f}).ToList(),14-12+5*8};
            int failcount=0;

            for (int i = 0; i < ExpressionsUse.Length; i++)
            {
               
                Object tempobject = testparser.Execute(ExpressionsUse[i]);
                if (Convert.ChangeType(tempobject, Expectedresults[i].GetType()).Equals( Expectedresults[i])
                    || (testparser.ResultToString(tempobject).Equals(testparser.ResultToString(Expectedresults[i])))
                    )
                //if (tempobject == Expectedresults[i])
                {
                    Debug.Print(ExpressionsUse[i] + " success. Result was " + testparser.ResultToString(tempobject));
                    

                }
                else
                {
                    Debug.Print(ExpressionsUse[i] + " failed. Result was " + testparser.ResultToString(tempobject) + "Expected result was " + testparser.ResultToString(Expectedresults[i]));

                    failcount++;
                    
                }


            }
            if (failcount > 0)
                Assert.Fail(failcount.ToString() + "/" + ExpressionsUse.Length + " tests failed.");


            
                
            
        }
    }
}

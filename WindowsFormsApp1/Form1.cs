using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Microsoft.SqlServer;
using System.Xml;


namespace WindowsFormsApp1
{
    public static class XmlDocumentExtensions
    {
        public static void IterateThroughAllNodes(
            this XmlDocument doc,
            Action<XmlNode> elementVisitor)
        {
            if (doc != null && elementVisitor != null)
            {
                foreach (XmlNode node in doc.ChildNodes)
                {
                    doIterateNode(node, elementVisitor);
                }
            }
        }

        private static void doIterateNode(
            XmlNode node,
            Action<XmlNode> elementVisitor)
        {
            elementVisitor(node);

            foreach (XmlNode childNode in node.ChildNodes)
            {
                doIterateNode(childNode, elementVisitor);
            }
        }
    }
    public partial class Form1 : Form
    {
        public static string SSISPackage = "";
        public static string HashString = "";

        public Form1()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog diag = new FolderBrowserDialog();
            if (diag.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtDirectory.Text = diag.SelectedPath;  //selected folder path

            }

        }
        public void ProcessNodes(XmlNode node, XmlNamespaceManager nsmgr, TextWriter txt) //recursive in case of sequence containers
        {
            foreach (XmlNode executable in node)
            {
                //XmlNodeList objectdata;

                string exec = executable.Attributes.GetNamedItem("DTS:CreationName").Value.ToString();
                switch (exec)
                {
                    case var _ when exec.Contains("ExecuteSQLTask")://Execute SQL Task
                        txt.WriteLine("--" + executable.Attributes.GetNamedItem("DTS:ObjectName").Value.ToString());
                        ProcessExecuteSQLTask(executable, nsmgr, txt);
                        break;
                    case var _ when exec.Contains("STOCK:SEQUENCE"):// "Sequence Container"
                        txt.WriteLine("--" + executable.Attributes.GetNamedItem("DTS:ObjectName").Value.ToString());
                        ProcessNodes(executable.SelectSingleNode("DTS:Executables", nsmgr), nsmgr, txt);
                        break;
                    case var _ when exec.Contains("SSIS.Pipeline.3"):// "Data Flow Task"
                        HashString = "";
                        txt.WriteLine("--" + executable.Attributes.GetNamedItem("DTS:ObjectName").Value.ToString());
                        ProcessDataFlowTask(executable, nsmgr, txt);
                        break;

                }

            }


        }

        static void ProcessExecuteSQLTask(XmlNode executable, XmlNamespaceManager nsmgr, TextWriter txt)
        {
            string SqlStatementSource = executable.SelectSingleNode("DTS:ObjectData", nsmgr).SelectSingleNode("SQLTask:SqlTaskData", nsmgr).Attributes.GetNamedItem("SQLTask:SqlStatementSource").Value.ToString();
            //if the SQL statement does not contain an EXEC statement then it is a regular SQL expression and it should be converted to a stored proc
            if (SqlStatementSource.Contains("exec") != true)
            {

                txt.WriteLine("SET ANSI_NULLS ON");
                txt.WriteLine("--USE " + executable.SelectSingleNode("DTS:ObjectData", nsmgr).SelectSingleNode("SQLTask:SqlTaskData", nsmgr).Attributes.GetNamedItem("SQLTask:Connection").Value.ToString());
                txt.WriteLine("GO");
                txt.WriteLine("SET QUOTED_IDENTIFIER ON");
                txt.WriteLine("GO");
                txt.WriteLine("/************************************************************************/");
                txt.WriteLine("/*	Created by:		            										*/");
                txt.WriteLine("/*   Creation date:	" + DateTime.Now.ToString("yyyy-MM-dd") + "	    	*/");
                txt.WriteLine("/*	Updated by:												            */");
                txt.WriteLine("/*	Update date													        */");
                txt.WriteLine("/*	SSIS Package:   " + SSISPackage.ToString() + "                      */");
                txt.WriteLine("/*	Description:    " + executable.Attributes.GetNamedItem("DTS:ObjectName").Value.ToString() + "  */");
                txt.WriteLine("/*	Version:															*/");
                txt.WriteLine("/*	v0                                              					*/");
                txt.WriteLine("/*	v1                                              					*/");
                txt.WriteLine("/*	add v1,v2 with change info											*/");
                txt.WriteLine("/************************************************************************/");
                txt.WriteLine("--" + SqlStatementSource.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " "));
                txt.WriteLine("CREATE PROC ssis.usp_" + SSISPackage.ToString() + "_" + executable.Attributes.GetNamedItem("DTS:ObjectName").Value.ToString().Replace(" ", "_").Replace("-", "_"));
                XmlNodeList ParameterBindingsList = executable.SelectSingleNode("DTS:ObjectData", nsmgr).SelectSingleNode("SQLTask:SqlTaskData", nsmgr).SelectNodes("SQLTask:ParameterBinding", nsmgr);
                if (ParameterBindingsList != null)
                {
                    foreach (XmlNode pb in ParameterBindingsList)
                    {
                        txt.WriteLine("--Parameter " +
                            pb.Attributes.GetNamedItem("SQLTask:ParameterName").Value.ToString() + " " +
                            pb.Attributes.GetNamedItem("SQLTask:DtsVariableName").Value.ToString() + " " +
                            pb.Attributes.GetNamedItem("SQLTask:ParameterDirection").Value.ToString()
                         );

                    }
                }

                txt.WriteLine("AS");
                if(executable.SelectSingleNode("DTS:PropertyExpression",nsmgr) != null )
                {
                    txt.WriteLine("--Variable SQL expression" 
                                + " " + executable.SelectSingleNode("DTS:PropertyExpression", nsmgr).Attributes.GetNamedItem("DTS:Name").Value.ToString()
                                + " " + executable.SelectSingleNode("DTS:PropertyExpression", nsmgr).InnerText.ToString() );
                }
                txt.WriteLine("BEGIN");
                
                txt.WriteLine(SqlStatementSource);
                txt.WriteLine("END");

            }
            //else
            //{
            //    txt.WriteLine("--"+ SqlStatementSource.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " "));
            //}
           
            //XmlNodeList ResultBindingsList = executable.SelectSingleNode("DTS:ObjectData", nsmgr).SelectSingleNode("SQLTask:SqlTaskData", nsmgr).SelectNodes("SQLTask:ResultBinding", nsmgr);
            //if (ResultBindingsList != null)
            //{
            //    foreach (XmlNode pb in ResultBindingsList)
            //    {
            //        txt.WriteLine("--Result " +
            //            pb.Attributes.GetNamedItem("SQLTask:ResultName").Value.ToString() + " " +
            //            pb.Attributes.GetNamedItem("SQLTask:DtsVariableName").Value.ToString()
            //         );

            //    }
            //}
        }
        static void ProcessDataFlowTask(XmlNode executable, XmlNamespaceManager nsmgr, TextWriter txt)
        {
            if (executable.SelectSingleNode("DTS:ObjectData", nsmgr).SelectSingleNode("pipeline", nsmgr).HasChildNodes)
            {
                XmlNodeList ComponentsList = executable.SelectSingleNode("DTS:ObjectData", nsmgr).SelectSingleNode("pipeline", nsmgr).SelectSingleNode("components", nsmgr).SelectNodes("component");
                if (ComponentsList != null)
                {
                    foreach (XmlNode comp in ComponentsList)
                    {
                        switch (comp.Attributes.GetNamedItem("componentClassID").Value.ToString())
                        {
                            //case "DTS.ManagedComponentWrapper.3":
                            //    //we have a multi hash component
                            //    if (comp.Attributes.GetNamedItem("contactInfo").Value.ToString() == "http://ssismhash.codeplex.com/")
                            //    {

                            //        XmlNodeList HashInputList = comp.SelectSingleNode("inputs").SelectSingleNode("input").SelectSingleNode("inputColumns").SelectNodes("inputColumn");
                            //        foreach (XmlNode prop in HashInputList)
                            //        {
                            //            HashString += "--,ISNULL([" + prop.Attributes.GetNamedItem("cachedName").Value.ToString() + "],N'')" + "\r\n";
                            //        }
                            //        HashString = "--,T_NewHashValue = UPPER(CONVERT(NVARCHAR(32),HashBytes('MD5', LTRIM(RTRIM(CONCAT(" + HashString.Substring(3);
                            //        HashString += "--)))),2))";
                            //        txt.WriteLine(HashString);
                            //        //XmlNodeList HashOutputList = comp.SelectSingleNode("outputs").SelectSingleNode("output").SelectSingleNode("outputColumns").SelectSingleNode("outputColumn").SelectSingleNode("properties").SelectNodes("property");
                            //        //foreach (XmlNode prop in HashOutputList)
                            //        //{
                            //        //    switch (prop.Attributes.GetNamedItem("name").Value.ToString())
                            //        //    {
                            //        //        case "InputColumnLineageIDs":
                            //        //            break;

                            //        //    }
                            //        //}
                            //    }
                            //    break;
                            case "DTSAdapter.OLEDBSource.3":
                                txt.WriteLine("--Source component name " + comp.Attributes.GetNamedItem("refId").Value.ToString());
                                XmlNodeList ConnectionsList = comp.SelectSingleNode("connections").SelectNodes("connection");
                                foreach (XmlNode cn in ConnectionsList)
                                {
                                    txt.WriteLine("--Source component Database Connection " + cn.Attributes.GetNamedItem("connectionManagerRefId").Value.ToString());
                                    txt.WriteLine("USE " + cn.Attributes.GetNamedItem("connectionManagerRefId").Value.ToString().Replace("]", "").Replace("Project.ConnectionManagers[", "").Replace("Project.ConnectionManagers[ ", ""));
                                }

                                XmlNodeList CompPropList = comp.SelectSingleNode("properties").SelectNodes("property");
                                foreach (XmlNode prop in CompPropList)
                                {
                                    switch (prop.Attributes.GetNamedItem("name").Value.ToString())
                                    {
                                        case "SqlCommand":
                                            if (prop.HasChildNodes)
                                            {
                                                txt.WriteLine("SET ANSI_NULLS ON");
                                                txt.WriteLine("GO");
                                                txt.WriteLine("SET QUOTED_IDENTIFIER ON");
                                                txt.WriteLine("GO");
                                                txt.WriteLine("/************************************************************************/");
                                                txt.WriteLine("/*	Created by:		            										*/");
                                                txt.WriteLine("/*   Creation date:	" + DateTime.Now.ToString("yyyy-MM-dd") + "	    	*/");
                                                txt.WriteLine("/*	Updated by:												            */");
                                                txt.WriteLine("/*	Update date													        */");
                                                txt.WriteLine("/*	SSIS Package:   " + SSISPackage.ToString() + "                      */");
                                                txt.WriteLine("/*	Description:    " + comp.Attributes.GetNamedItem("refId").Value.ToString() + "  */");
                                                txt.WriteLine("/*	Version:															*/");
                                                txt.WriteLine("/*	v0                                              					*/");
                                                txt.WriteLine("/*	v1                                              					*/");
                                                txt.WriteLine("/*	add v1,v2 with change info											*/");
                                                txt.WriteLine("/************************************************************************/");
                                                txt.WriteLine("CREATE PROC ssis.usp_" + SSISPackage.ToString() + "_" + comp.Attributes.GetNamedItem("name").Value.ToString().Replace(" ", "_"));
                                                if (prop.ChildNodes[0].Value.ToString().Contains("declare @MasterPackageId int = ?"))
                                                {
                                                    txt.WriteLine("@MasterPackageId int");
                                                }
                                                txt.WriteLine("AS");
                                                txt.WriteLine("BEGIN");
                                                txt.WriteLine(prop.ChildNodes[0].Value.ToString().Replace("declare @MasterPackageId int = ?", "--declare @MasterPackageId int = ?"));
                                                txt.WriteLine("END");

                                            }
                                            break;
                                        case "SqlCommandVariable":
                                            if (prop.HasChildNodes)
                                            {
                                                txt.WriteLine("--Source component SQL Command Variable " + prop.ChildNodes[0].Value.ToString());
                                            }
                                            break;
                                    }
                                }
                                break;

                        }



                    }
                }
            }

        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            if (txtDirectory.Text != "")
            {
                try
                {
                    string[] dirs = Directory.GetFiles(txtDirectory.Text, "*.dtsx", SearchOption.TopDirectoryOnly);
                    Console.WriteLine("The number of files starting with c is {0}.", dirs.Length);
                    foreach (string dir in dirs)
                    {
                        SSISPackage = Path.GetFileName(dir.ToString()).ToString().Replace(".dtsx", "");
        
                        System.IO.TextWriter writeFile = new StreamWriter(txtDirectoryToSave.Text + "\\Stored procs " + SSISPackage + ".txt", false);
                        writeFile.WriteLine("//////Start analysis of SSIS package " + dir.ToString() + "//////");
                        XmlDocument doc = new XmlDocument();
                        doc.Load(dir);
                        // Create an XmlNamespaceManager to resolve the default namespace.
                        XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
                        nsmgr.AddNamespace("DTS", "www.microsoft.com/SqlServer/Dts");
                        nsmgr.AddNamespace("SQLTask", "www.microsoft.com/sqlserver/dts/tasks/sqltask");
                        doc.IterateThroughAllNodes(
                            delegate (XmlNode node)
                            {
                                //writeFile.WriteLine("--" + node.Name);
                                switch(node.Name)
                                {
                                    case var _ when node.Name.Contains("DTS:Variable")://Execute SQL Task
                                        if(node.Attributes.GetNamedItem("DTS:ObjectName") != null)
                                        {
                                            writeFile.WriteLine("--Variable name: " + node.Attributes.GetNamedItem("DTS:ObjectName").Value.ToString() 
                                                                + " DataType: "     + node.SelectSingleNode("DTS:VariableValue", nsmgr).Attributes.GetNamedItem("DTS:DataType").Value.ToString() 
                                                                + " Value: "        + node.SelectSingleNode("DTS:VariableValue", nsmgr).InnerText.ToString());
                                         }
                                        break;
                                    case var _ when node.Name.Contains("DTS:EventHandler")://Execute SQL Task
                                        if (node.Attributes.GetNamedItem("DTS:refId") != null)
                                        {
                                            writeFile.WriteLine("--EventHandler name: " + node.Attributes.GetNamedItem("DTS:refId").Value.ToString());
                                        }
                                        break;
                                    case var _ when node.Name.Contains("DTS:PrecedenceConstraint") && node.ParentNode.ParentNode.Name.Contains("DTS:EventHandler")://Execute SQL Task
                                        if (node.Attributes.GetNamedItem("DTS:From") != null)
                                        {
                                            writeFile.WriteLine("--Precendence constraint From: " + node.Attributes.GetNamedItem("DTS:From").Value.ToString());
                                        }
                                        if (node.Attributes.GetNamedItem("DTS:To") != null)
                                        {
                                            writeFile.WriteLine("--Precendence constraint To: " + node.Attributes.GetNamedItem("DTS:To").Value.ToString());
                                        }
                                        if (node.Attributes.GetNamedItem("DTS:Expression") != null)
                                        {
                                            writeFile.WriteLine("--Precendence constraint Expression: " + node.Attributes.GetNamedItem("DTS:Expression").Value.ToString());
                                        }
                                        break;
                                    case var _ when node.Name.Contains("DTS:Executable")://Execute SQL Task
                                        if (node.Attributes.GetNamedItem("DTS:ObjectName") != null)
                                        {
                                            string objname = node.Attributes.GetNamedItem("DTS:ObjectName").Value.ToString();
                                            if (node.Attributes != null && node.Attributes.GetNamedItem("DTS:CreationName") != null)
                                            {
                                                //find first comma
                                                string exec = node.Attributes.GetNamedItem("DTS:CreationName").Value.ToString();
                                                int commapos = exec.IndexOf(",");
                                                if(commapos>0)
                                                {
                                                    writeFile.WriteLine("--Executable type: " + exec.Substring(0,commapos));
                                                }
                                                else
                                                {
                                                    writeFile.WriteLine("--Executable type: " + exec);
                                                }
                                                switch (exec)
                                                {
                                                    case var _ when exec.Contains("ExecuteSQLTask")://Execute SQL Task
                                                        writeFile.WriteLine("--SQL Task name: " + objname);
                                                        ProcessExecuteSQLTask(node, nsmgr, writeFile);
                                                        break;
                                                    case var _ when exec.Contains("STOCK:SEQUENCE"):// "Sequence Container"
                                                        //ProcessNodes(node.SelectSingleNode("DTS:Executables", nsmgr), nsmgr, writeFile);
                                                        writeFile.WriteLine("--Sequence container name: " + objname);
                                                        break;
                                                    case var _ when exec.Contains("SSIS.Pipeline.3"):// "Data Flow Task"
                                                        writeFile.WriteLine("--Data Flow Task name: " + objname);
                                                        HashString = "";
                                                        ProcessDataFlowTask(node, nsmgr, writeFile);
                                                        break;

                                                }
                                            }
                                        }
                                        break;
                                }
                                
                                //}// ...Do something with the node...
                            });
                        // Get DTSPackages to add a package
                        //XmlNode DTSExecutables = doc.SelectSingleNode("//DTS:Executable", nsmgr);
                        //ProcessNodes(DTSExecutables, nsmgr, writeFile);
                        //XmlNode DTSExecutables = doc.SelectSingleNode("//DTS:Executables", nsmgr);
                        //ProcessNodes(DTSExecutables, nsmgr, writeFile);
                        //XmlNode DTSEventHandlers = doc.SelectSingleNode("//DTS:EventHandlers", nsmgr);
                        //ProcessNodes(DTSEventHandlers, nsmgr, writeFile);

                        writeFile.WriteLine("//////End analysis of SSIS package " + dir.ToString() + "//////");
                        writeFile.Flush();
                        writeFile.Close();
                        writeFile = null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("The process failed: {0}", ex.ToString());
                }
            }

        }

        private void btnCheckTables_Click(object sender, EventArgs e)
        {
            string[] stringSeparators = new string[] { "\r\n" };

            string[] allLines = txtTables.Text.Split(stringSeparators, StringSplitOptions.None);
            int i = 0;
            string TablesInPackage = "";
            if (txtDirectory.Text != "" && txtTables.Text != "")
            {
                try
                {
                    string[] dirs = Directory.GetFiles(txtDirectory.Text, "*.dtsx", SearchOption.TopDirectoryOnly);
                    System.IO.TextWriter writeFile = new StreamWriter(txtDirectoryToSave.Text + "\\Packages DPO.txt", false);
                    foreach (string dir in dirs)
                    {
                        SSISPackage = Path.GetFileName(dir.ToString()).ToString().Replace(".dtsx", "");
                        Console.WriteLine("Processing " + SSISPackage.ToString());
                        foreach (string line in System.IO.File.ReadAllLines(dir))
                        {
                            foreach (string text in allLines)
                            {
                                if (text != "")
                                {
                                    Console.WriteLine("Searching for " + text.ToString());
                                    if (line.Contains(text) && text != "")
                                    {
                                        i = 1;
                                        writeFile.WriteLine(SSISPackage + ";" + text + "\r\n");

                                        TablesInPackage += text + "; ";
                                    }
                                }
                            }


                        }
                        if (i == 0)
                        {
                            writeFile.WriteLine(SSISPackage);


                        }
                        else
                        {
                            writeFile.WriteLine(SSISPackage + ";" + TablesInPackage);
                            //Console.WriteLine("Processing " + SSISPackage.ToString());
                        }
                        TablesInPackage = "";
                        i = 0;
                    }
                    writeFile.Flush();
                    writeFile.Close();
                    writeFile = null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("The process failed: {0}", ex.ToString());
                }
            }
        }

        private void btnBrowseLocationToSaveFiles_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog diag = new FolderBrowserDialog();
            if (diag.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtDirectoryToSave.Text = diag.SelectedPath;  //selected folder path

            }
        }
    }

}

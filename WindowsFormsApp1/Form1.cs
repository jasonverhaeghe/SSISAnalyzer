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
        public static List<string> DependencyConstraints = new List<string>();
        public static void GetSSISComponentSequence(List<string> dataSetTo,List<string> dataSetFrom, List<string> dataSetRevTo, List<string> dataSetRevFrom)
	    {
		    // Find the starting point of itinerary
            int s=-1;
            string start="";
            var FromList=dataSetFrom.Except(dataSetRevFrom).ToList();
		    for (int i=0; i<dataSetFrom.Count; i++)
		    {
                if (dataSetFrom[i].Equals(FromList[0]))
			    {
				    start = dataSetTo[i];
				    s=i;
                    break;
			    }
		    }
		    //// If we could not find a starting point, then something wrong with input
		    if (string.IsNullOrEmpty(start))
		    {
			    Console.Write("Invalid Input");
			    Console.Write("\n");
			    return;
		    }
            //txt.WriteLine("Sequence:" + dataSetFrom[s].ToString());
            //txt.WriteLine("Next sequence:" + dataSetTo[s].ToString());
            DependencyConstraints.Add(dataSetFrom[s].ToString());
            int found=-1;
            for (int i=0; i<dataSetTo.Count; i++)
		    {
			    if (dataSetFrom[i].Equals(dataSetTo[s]))
			    {
				    DependencyConstraints.Add(dataSetFrom[i].ToString());
                    //txt.WriteLine("Sequence:" + dataSetFrom[i].ToString());
                    //txt.WriteLine("Next sequence:" + dataSetTo[i].ToString());
                    found++;
                    s=i;
                    i=-1;
			    }
                if (found==dataSetFrom.Count)
                {
                    break;
                }
		    }
            DependencyConstraints.Add(dataSetTo[dataSetTo.Count-1].ToString());
                    
	    }
        // Another simple way would be to create a class which has a constructor to hold the three strings
       
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
        private void ReadXMLToTxt(XmlNode root, XmlNamespaceManager nsmgr, TextWriter txt)
        {
            if (root is XmlElement)
            {
                ProcessNodes(root,  nsmgr,  txt);
                foreach (XmlNode childNode in root.ChildNodes)
                {
                    ReadXMLToTxt(childNode,  nsmgr,  txt);
                
                 }
                //if (root.HasChildNodes)
                //    ReadXMLToTxt(root.FirstChild,  nsmgr,  txt);
                //if (root.NextSibling != null)
                //    ReadXMLToTxt(root.NextSibling,  nsmgr,  txt);
            }
            else if (root is XmlText)
            {}
            else if (root is XmlComment)
            {}
        }
        private void ReadXMLToXml(XmlNode root, XmlNamespaceManager nsmgr, XmlWriter doc)
        {
            if (root is XmlElement)
            {
                ProcessNodesXml(root,  nsmgr,  doc);
                foreach (XmlNode childNode in root.ChildNodes)
                {
                    //doc.WriteStartElement("Level");
                    ReadXMLToXml(childNode,  nsmgr,  doc);
                    //doc.WriteEndElement();
                 }
            }
            else if (root is XmlText)
            {}
            else if (root is XmlComment)
            {}
        }
        public void ProcessNodes(XmlNode node, XmlNamespaceManager nsmgr, TextWriter txt) //recursive in case of sequence containers
        {
            
                switch(node.Name)
                {
                    case var _ when node.Name.Contains("DTS:Variable")://Variable
                        if(node.Attributes.GetNamedItem("DTS:ObjectName") != null)
                        {
                            txt.WriteLine("--Variable name: " + node.Attributes.GetNamedItem("DTS:ObjectName").Value.ToString() 
                                                + " DataType: "     + node.SelectSingleNode("DTS:VariableValue", nsmgr).Attributes.GetNamedItem("DTS:DataType").Value.ToString() 
                                                + " Value: "        + node.SelectSingleNode("DTS:VariableValue", nsmgr).InnerText.ToString());
                            }
                        break;
                    case var _ when node.Name.Contains("DTS:PackageParameter")://Parameter
                        if(node.Attributes.GetNamedItem("DTS:ObjectName") != null)
                        {
                            txt.WriteLine("--Parameter name: " + node.Attributes.GetNamedItem("DTS:ObjectName").Value.ToString() 
                                                + " DataType: "     + node.SelectSingleNode("DTS:Property", nsmgr).Attributes.GetNamedItem("DTS:DataType").Value.ToString() 
                                                + " Value: "        + node.SelectSingleNode("DTS:Property", nsmgr).InnerText.ToString());
                            }
                        break;
                    case var _ when node.Name.Contains("DTS:EventHandler")://Eventhandler
                        if (node.Attributes.GetNamedItem("DTS:refId") != null)
                        {
                            txt.WriteLine("--EventHandler name: " + node.Attributes.GetNamedItem("DTS:refId").Value.ToString());
                        }
                        break;
                    case var _ when node.Name.Contains("DTS:PrecedenceConstraint") && node.ParentNode.ParentNode.Name.Contains("DTS:EventHandler")://Precedenceconstraint
                        if (node.Attributes.GetNamedItem("DTS:From") != null)
                        {
                            txt.WriteLine("--Precendence constraint From: " + node.Attributes.GetNamedItem("DTS:From").Value.ToString());
                        }
                        if (node.Attributes.GetNamedItem("DTS:To") != null)
                        {
                            txt.WriteLine("--Precendence constraint To: " + node.Attributes.GetNamedItem("DTS:To").Value.ToString());
                        }
                        if (node.Attributes.GetNamedItem("DTS:Expression") != null)
                        {
                            txt.WriteLine("--Precendence constraint Expression: " + node.Attributes.GetNamedItem("DTS:Expression").Value.ToString());
                        }
                        break;
                    case var _ when node.Name.Contains("DTS:Executable")://Executable
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
                                    txt.WriteLine("--Executable type: " + exec.Substring(0,commapos));
                                }
                                else
                                {
                                    txt.WriteLine("--Executable type: " + exec);
                                }
                                switch (exec)
                                {
                                    case var _ when exec.Contains("ExecuteSQLTask")://Execute SQL Task
                                        txt.WriteLine("--SQL Task name: " + objname);
                                        ProcessExecuteSQLTask(node, nsmgr, txt);
                                        break;
                                    case var _ when exec.Contains("STOCK:SEQUENCE"):// "Sequence Container"
                                        //ProcessNodes(node.SelectSingleNode("DTS:Executables", nsmgr), nsmgr, txt);
                                        txt.WriteLine("--Sequence container name: " + objname);
                                        break;
                                    case var _ when exec.Contains("SSIS.Pipeline.3"):// "Data Flow Task"
                                        txt.WriteLine("--Data Flow Task name: " + objname);
                                        HashString = "";
                                        ProcessDataFlowTask(node, nsmgr, txt);
                                        break;

                                }
                            }
                        }
                        break;
 

            }
               

        }
        public void ProcessNodesXml(XmlNode node, XmlNamespaceManager nsmgr, XmlWriter txt) //recursive in case of sequence containers
        {
            
                switch(node.Name)
                {
                    case var _ when node.Name.Equals("DTS:Variable") ://&& node.ParentNode.ParentNode.Name.Equals("DTS:Varia")://Variable
                        if(node.Attributes.GetNamedItem("DTS:ObjectName") != null)
                        {
                             txt.WriteElementString("Variable",node.Attributes.GetNamedItem("DTS:ObjectName").Value.ToString() 
                                                + " DataType: "     + node.SelectSingleNode("DTS:VariableValue", nsmgr).Attributes.GetNamedItem("DTS:DataType").Value.ToString() 
                                                + " Value: "        + node.SelectSingleNode("DTS:VariableValue", nsmgr).InnerText.ToString());
                        }
                        break;
                    case var _ when node.Name.Equals("DTS:PackageParameter")://Parameter
                        if(node.Attributes.GetNamedItem("DTS:ObjectName") != null)
                        {
                             txt.WriteElementString("Parameter",node.Attributes.GetNamedItem("DTS:ObjectName").Value.ToString() 
                                                + " DataType: "     + node.SelectSingleNode("DTS:Property", nsmgr).Attributes.GetNamedItem("DTS:DataType").Value.ToString() 
                                                + " Value: "        + node.SelectSingleNode("DTS:Property", nsmgr).InnerText.ToString());
                            
                         }
                        break;
                    case var _ when node.Name.Equals("DTS:EventHandler")://Eventhandler
                        if (node.Attributes.GetNamedItem("DTS:refId") != null)
                        {
                            txt.WriteElementString("EventHandler" , node.Attributes.GetNamedItem("DTS:refId").Value.ToString());
                        }
                        break;
                    case var _ when node.Name.Equals("DTS:PrecedenceConstraint") && node.ParentNode.ParentNode.Name.Contains("DTS:EventHandler")://Precedenceconstraint
                        if (node.Attributes.GetNamedItem("DTS:From") != null)
                        {
                            txt.WriteElementString("From" , node.Attributes.GetNamedItem("DTS:From").Value.ToString());
                        }
                        if (node.Attributes.GetNamedItem("DTS:To") != null)
                        {
                            txt.WriteElementString("To" , node.Attributes.GetNamedItem("DTS:To").Value.ToString());
                        }
                        if (node.Attributes.GetNamedItem("DTS:Expression") != null)
                        {
                            txt.WriteElementString("Expression" , node.Attributes.GetNamedItem("DTS:Expression").Value.ToString());
                        }
                        break;
                    case var _ when node.Name.Equals("DTS:Executable")://Executable
                        if (node.Attributes.GetNamedItem("DTS:ObjectName") != null)
                        {
                            string objname = node.Attributes.GetNamedItem("DTS:ObjectName").Value.ToString();
                            if (node.Attributes != null && node.Attributes.GetNamedItem("DTS:CreationName") != null)
                            {
                                //find first comma
                                string exec = node.Attributes.GetNamedItem("DTS:CreationName").Value.ToString();
                                //int commapos = exec.IndexOf(",");
                                //if(commapos>0)
                                //{
                                //    txt.WriteElementString("ExecutableType", exec.Substring(0,commapos));
                                //}
                                //else
                                //{
                                //    txt.WriteElementString("ExecutableType", exec);
                                //}
                                switch (exec)
                                {
                                    case var _ when exec.Contains("ExecuteSQLTask")://Execute SQL Task
                                        txt.WriteElementString("ExecutableType", "ExecuteSQLTask"); 
                                        txt.WriteStartElement("SQLTask");
                                        txt.WriteAttributeString("Name" , objname);
                                        ProcessExecuteSQLTaskXml(node, nsmgr, txt);
                                        txt.WriteEndElement();
                                        break;
                                    case var _ when exec.Contains("STOCK:SEQUENCE"):// "Sequence Container"
                                        //ProcessNodes(node.SelectSingleNode("DTS:Executables", nsmgr), nsmgr, txt);
                                        txt.WriteElementString("ExecutableType", "SequenceContainer"); 
                                        txt.WriteStartElement("SequenceContainer");
                                        txt.WriteAttributeString("Name" , objname);
                                        txt.WriteEndElement();
                                        break;
                                    case var _ when exec.Contains("SSIS.Pipeline.3"):// "Data Flow Task"
                                        txt.WriteElementString("ExecutableType", "DataFlowTask"); 
                                        txt.WriteStartElement("DataFlowTask");
                                        txt.WriteAttributeString("Name" , objname);
                                        HashString = "";
                                        ProcessDataFlowTaskXml(node, nsmgr, txt);
                                        txt.WriteEndElement();
                                        break;

                                }
                            }
                        }
                        break;
                        

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
        static void ProcessExecuteSQLTaskXml(XmlNode executable, XmlNamespaceManager nsmgr, XmlWriter txt)
        {
            string SqlStatementSource = executable.SelectSingleNode("DTS:ObjectData", nsmgr).SelectSingleNode("SQLTask:SqlTaskData", nsmgr).Attributes.GetNamedItem("SQLTask:SqlStatementSource").Value.ToString();
            //if the SQL statement does not contain an EXEC statement then it is a regular SQL expression and it should be converted to a stored proc
            XmlNodeList ParameterBindingsList = executable.SelectSingleNode("DTS:ObjectData", nsmgr).SelectSingleNode("SQLTask:SqlTaskData", nsmgr).SelectNodes("SQLTask:ParameterBinding", nsmgr);
                
            txt.WriteElementString("SQLStatementSource",SqlStatementSource);
            
            string SQLStatement="";
            
            if (SqlStatementSource.Contains("exec") != true)
            {
                SQLStatement=SQLStatement + "\r\n" + "SET ANSI_NULLS ON";
                SQLStatement=SQLStatement + "\r\n" + "--USE " + executable.SelectSingleNode("DTS:ObjectData", nsmgr).SelectSingleNode("SQLTask:SqlTaskData", nsmgr).Attributes.GetNamedItem("SQLTask:Connection").Value.ToString();
                SQLStatement=SQLStatement + "\r\n" + "GO";
                SQLStatement=SQLStatement + "\r\n" + "SET QUOTED_IDENTIFIER ON";
                SQLStatement=SQLStatement + "\r\n" + "GO";
                SQLStatement=SQLStatement + "\r\n" + "/************************************************************************/";
                SQLStatement=SQLStatement + "\r\n" + "/*	Created by:		            										*/";
                SQLStatement=SQLStatement + "\r\n" + "/*   Creation date:	" + DateTime.Now.ToString("yyyy-MM-dd") + "	    	*/";
                SQLStatement=SQLStatement + "\r\n" + "/*	Updated by:												            */";
                SQLStatement=SQLStatement + "\r\n" + "/*	Update date													        */";
                SQLStatement=SQLStatement + "\r\n" + "/*	SSIS Package:   " + SSISPackage.ToString() + "                      */";
                SQLStatement=SQLStatement + "\r\n" + "/*	Description:    " + executable.Attributes.GetNamedItem("DTS:ObjectName").Value.ToString() + "  */";
                SQLStatement=SQLStatement + "\r\n" + "/*	Version:															*/";
                SQLStatement=SQLStatement + "\r\n" + "/*	v0                                              					*/";
                SQLStatement=SQLStatement + "\r\n" + "/*	v1                                              					*/";
                SQLStatement=SQLStatement + "\r\n" + "/*	add v1,v2 with change info											*/";
                SQLStatement=SQLStatement + "\r\n" + "/************************************************************************/";
                SQLStatement=SQLStatement + "\r\n" + "--" + SqlStatementSource.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");
                SQLStatement=SQLStatement + "\r\n" + "CREATE PROC ssis.usp_" + SSISPackage.ToString() + "_" + executable.Attributes.GetNamedItem("DTS:ObjectName").Value.ToString().Replace(" ", "_").Replace("-", "_");
                if (ParameterBindingsList != null)
                {
                    foreach (XmlNode pb in ParameterBindingsList)
                    {
                        SQLStatement=SQLStatement + "\r\n" + "--Parameter " +
                            pb.Attributes.GetNamedItem("SQLTask:ParameterName").Value.ToString() + " " +
                            pb.Attributes.GetNamedItem("SQLTask:DtsVariableName").Value.ToString() + " " +
                            pb.Attributes.GetNamedItem("SQLTask:ParameterDirection").Value.ToString()
                            ;
                    }
                }
                SQLStatement=SQLStatement + "\r\n" + "AS";
                if(executable.SelectSingleNode("DTS:PropertyExpression",nsmgr) != null )
                {
                    SQLStatement=SQLStatement + "\r\n" + "--Variable SQL expression" 
                                + " " + executable.SelectSingleNode("DTS:PropertyExpression", nsmgr).Attributes.GetNamedItem("DTS:Name").Value.ToString()
                                + " " + executable.SelectSingleNode("DTS:PropertyExpression", nsmgr).InnerText.ToString();
                }
                SQLStatement=SQLStatement + "\r\n" + "BEGIN";
                SQLStatement=SQLStatement + "\r\n" + SqlStatementSource;
                SQLStatement=SQLStatement + "\r\n" + "END";
                txt.WriteElementString("SuggestedStoredProc",SQLStatement);
            }
            //else
            //{
            //    txt.WriteLine("--"+ SqlStatementSource.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " "));
            //}
           // ParameterBindingsList = executable.SelectSingleNode("DTS:ObjectData", nsmgr).SelectSingleNode("SQLTask:SqlTaskData", nsmgr).SelectNodes("SQLTask:ParameterBinding", nsmgr);
            if (ParameterBindingsList != null)
            {
                txt.WriteStartElement("ParameterBindingList");
                foreach (XmlNode pb in ParameterBindingsList)
                {
                    txt.WriteElementString("ParameterBinding" ,
                        pb.Attributes.GetNamedItem("SQLTask:ParameterName").Value.ToString() + " " +
                        pb.Attributes.GetNamedItem("SQLTask:DtsVariableName").Value.ToString() + " " +
                        pb.Attributes.GetNamedItem("SQLTask:ParameterDirection").Value.ToString()
                        );

                }
                txt.WriteEndElement();
            }
            XmlNodeList ResultBindingsList = executable.SelectSingleNode("DTS:ObjectData", nsmgr).SelectSingleNode("SQLTask:SqlTaskData", nsmgr).SelectNodes("SQLTask:ResultBinding", nsmgr);
            if (ResultBindingsList != null)
            {
                txt.WriteStartElement("ResultbindingList");
                foreach (XmlNode pb in ResultBindingsList)
                {
                    txt.WriteElementString("Resultbinding" ,
                        pb.Attributes.GetNamedItem("SQLTask:ResultName").Value.ToString() + " " +
                        pb.Attributes.GetNamedItem("SQLTask:DtsVariableName").Value.ToString()
                     );

                }
                txt.WriteEndElement();
            }
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
        static void ProcessDataFlowTaskXml(XmlNode executable, XmlNamespaceManager nsmgr, XmlWriter txt)
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
                            
                            case "DTSAdapter.OLEDBSource.3":
                                txt.WriteElementString("SourceComponent", comp.Attributes.GetNamedItem("refId").Value.ToString());
                                XmlNodeList ConnectionsList = comp.SelectSingleNode("connections").SelectNodes("connection");
                                foreach (XmlNode cn in ConnectionsList)
                                {
                                    string Connection="";
                                    Connection=Connection + "\r\n" + "--Source component Database Connection " + cn.Attributes.GetNamedItem("connectionManagerRefId").Value.ToString();
                                    Connection=Connection + "\r\n" + "USE " + cn.Attributes.GetNamedItem("connectionManagerRefId").Value.ToString().Replace("]", "").Replace("Project.ConnectionManagers[", "").Replace("Project.ConnectionManagers[ ", "");
                                    txt.WriteElementString("Connection",Connection);
                                }

                                XmlNodeList CompPropList = comp.SelectSingleNode("properties").SelectNodes("property");
                                 foreach (XmlNode prop in CompPropList)
                                {
                                    string SQLStatement="";
                                
                                    switch (prop.Attributes.GetNamedItem("name").Value.ToString())
                                    {
                                        case "SqlCommand":
                                            txt.WriteStartElement("SqlCommand");
                                            if (prop.HasChildNodes)
                                            {
                                                SQLStatement=SQLStatement + "\r\n" + "SET ANSI_NULLS ON";
                                                SQLStatement=SQLStatement + "\r\n" + "GO";
                                                SQLStatement=SQLStatement + "\r\n" + "SET QUOTED_IDENTIFIER ON";
                                                SQLStatement=SQLStatement + "\r\n" + "GO";
                                                SQLStatement=SQLStatement + "\r\n" + "/************************************************************************/";
                                                SQLStatement=SQLStatement + "\r\n" + "/*	Created by:		            										*/";
                                                SQLStatement=SQLStatement + "\r\n" + "/*   Creation date:	" + DateTime.Now.ToString("yyyy-MM-dd") + "	    	*/";
                                                SQLStatement=SQLStatement + "\r\n" + "/*	Updated by:												            */";
                                                SQLStatement=SQLStatement + "\r\n" + "/*	Update date													        */";
                                                SQLStatement=SQLStatement + "\r\n" + "/*	SSIS Package:   " + SSISPackage.ToString() + "                      */";
                                                SQLStatement=SQLStatement + "\r\n" + "/*	Description:    " + comp.Attributes.GetNamedItem("refId").Value.ToString() + "  */";
                                                SQLStatement=SQLStatement + "\r\n" + "/*	Version:															*/";
                                                SQLStatement=SQLStatement + "\r\n" + "/*	v0                                              					*/";
                                                SQLStatement=SQLStatement + "\r\n" + "/*	v1                                              					*/";
                                                SQLStatement=SQLStatement + "\r\n" + "/*	add v1,v2 with change info											*/";
                                                SQLStatement=SQLStatement + "\r\n" + "/************************************************************************/";
                                                SQLStatement=SQLStatement + "\r\n" + "CREATE PROC ssis.usp_" + SSISPackage.ToString() + "_" + comp.Attributes.GetNamedItem("name").Value.ToString().Replace(" ", "_");
                                                if (prop.ChildNodes[0].Value.ToString().Contains("declare @MasterPackageId int = ?"))
                                                {
                                                    SQLStatement=SQLStatement + "\r\n" + "@MasterPackageId int";
                                                }
                                                SQLStatement=SQLStatement + "\r\n" + "AS";
                                                SQLStatement=SQLStatement + "\r\n" + "BEGIN";
                                                SQLStatement=SQLStatement + "\r\n" + prop.ChildNodes[0].Value.ToString().Replace("declare @MasterPackageId int = ?", "--declare @MasterPackageId int = ?");
                                                SQLStatement=SQLStatement + "\r\n" + "END";

                                            }
                                            txt.WriteElementString("SuggestedStoredProc",SQLStatement + "\r\n");
                                            txt.WriteEndElement();
                                            break;
                                        case "SqlCommandVariable":
                                            if (prop.HasChildNodes)
                                            {
                                                txt.WriteElementString("SqlCommandVariable",prop.ChildNodes[0].Value.ToString() + "\r\n");
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
        
        
        private void btnProcessXml_Click(object sender, EventArgs e)
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
                        XmlWriterSettings settings = new XmlWriterSettings();  
                        settings.Indent = true;
                        settings.IndentChars = ("\t");
                        settings.CloseOutput = true;  
                        settings.OmitXmlDeclaration =true;  
                        //settings.NewLineOnAttributes = true;
  
                        using (XmlWriter docxml = XmlWriter.Create(txtDirectoryToSave.Text + "\\Analysis document" + SSISPackage + ".xml",settings))  
                        { 
                        
                            docxml.WriteStartElement("Package");                        
                            XmlDocument doc = new XmlDocument();
                            doc.Load(dir);
                            // Create an XmlNamespaceManager to resolve the default namespace.
                            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
                            nsmgr.AddNamespace("DTS", "www.microsoft.com/SqlServer/Dts");
                            nsmgr.AddNamespace("SQLTask", "www.microsoft.com/sqlserver/dts/tasks/sqltask");
                            //first determine the order of the different containers/steps
                            List<string> dataSetTo = new List<string>();
		                    List<string> dataSetFrom = new List<string>();
		                    List<string> dataSetRevTo = new List<string>();
		                    List<string> dataSetRevFrom = new List<string>();
		                    XmlNode PrecendenceConstraintsList= doc.SelectSingleNode("DTS:Executable",nsmgr).SelectSingleNode("DTS:PrecedenceConstraints",nsmgr);
                            foreach (XmlNode nd in PrecendenceConstraintsList.ChildNodes)
                            {
                                if (nd.Attributes.GetNamedItem("DTS:From") != null && nd.Attributes.GetNamedItem("DTS:To") != null)
                                {
                                    dataSetFrom.Add(nd.Attributes.GetNamedItem("DTS:From").Value.ToString());
                                    dataSetTo.Add(nd.Attributes.GetNamedItem("DTS:To").Value.ToString());
                                    dataSetRevFrom.Add(nd.Attributes.GetNamedItem("DTS:To").Value.ToString());
                                    dataSetRevTo.Add(nd.Attributes.GetNamedItem("DTS:From").Value.ToString());
                                 }
                            }
                            GetSSISComponentSequence(new List<string>(dataSetTo),new List<string>(dataSetFrom),new List<string>(dataSetRevTo),new List<string>(dataSetRevFrom));
                            docxml.WriteStartElement("Parameters");
                            //docxml.WriteElementString("Start", "Start analysis of PackageParameters"); 
                            XmlNode root = doc.SelectSingleNode("DTS:Executable",nsmgr).SelectSingleNode("DTS:PackageParameters",nsmgr); 
                            ReadXMLToXml(root,nsmgr,docxml);
                            //docxml.WriteElementString("End", "End analysis of PackageParameters"); 
                            docxml.WriteEndElement();
                            docxml.WriteStartElement("Variables");
                            //docxml.WriteElementString("Start", "Start analysis of Variables"); 
                            root = doc.SelectSingleNode("DTS:Executable",nsmgr).SelectSingleNode("DTS:Variables",nsmgr); 
                            ReadXMLToXml(root,nsmgr,docxml);
                            //docxml.WriteElementString("End", "End analysis of Variables"); 
                            docxml.WriteEndElement();
                            docxml.WriteStartElement("Executables");
                            for(int i=0;i<DependencyConstraints.Count;i++)
                            {
                                docxml.WriteStartElement("Executable");
                                docxml.WriteAttributeString("Name",DependencyConstraints[i].ToString());
                                root = doc.SelectSingleNode("DTS:Executable",nsmgr).SelectSingleNode("DTS:Executables",nsmgr).SelectSingleNode("DTS:Executable[@DTS:refId='"+DependencyConstraints[i].ToString()+"']",nsmgr); 
                                ReadXMLToXml(root,nsmgr,docxml);  
                                docxml.WriteEndElement();
                            
                            }
                            docxml.WriteEndElement();
                            docxml.WriteStartElement("PrecedenceConstraints");
                            for(int i=0;i<DependencyConstraints.Count;i++)
                            {
                                  if(i<DependencyConstraints.Count-1)
                                 {
                                    docxml.WriteElementString("Sequence",DependencyConstraints[i].ToString()+ " " + DependencyConstraints[i+1].ToString());
                                 }
                                       
                            }
                            root = doc.SelectSingleNode("DTS:Executable",nsmgr).SelectSingleNode("DTS:PrecedenceConstraints",nsmgr); 
                            ReadXMLToXml(root,nsmgr,docxml);
                            docxml.WriteEndElement();
                            docxml.WriteStartElement("EventHandlers");
                            root = doc.SelectSingleNode("DTS:Executable",nsmgr).SelectSingleNode("DTS:EventHandlers",nsmgr); 
                            ReadXMLToXml(root,nsmgr,docxml);
                            docxml.WriteEndElement();
                            docxml.WriteEndElement();
                            docxml.Flush();
                            docxml.Close();
                            docxml.Dispose();
                        }
                       
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("The process failed: {0}", ex.ToString());
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
        
                        System.IO.TextWriter writeFile = new StreamWriter(txtDirectoryToSave.Text + "\\Analysis document " + SSISPackage + ".txt", false);
                        writeFile.WriteLine("//////Start analysis of SSIS package " + dir.ToString() + "//////");
                        XmlDocument doc = new XmlDocument();
                        doc.Load(dir);
                        // Create an XmlNamespaceManager to resolve the default namespace.
                        XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
                        nsmgr.AddNamespace("DTS", "www.microsoft.com/SqlServer/Dts");
                        nsmgr.AddNamespace("SQLTask", "www.microsoft.com/sqlserver/dts/tasks/sqltask");
                        //first determine the order of the different containers/steps
                        List<string> dataSetTo = new List<string>();
		                List<string> dataSetFrom = new List<string>();
		                List<string> dataSetRevTo = new List<string>();
		                List<string> dataSetRevFrom = new List<string>();
		                XmlNode PrecendenceConstraintsList= doc.SelectSingleNode("DTS:Executable",nsmgr).SelectSingleNode("DTS:PrecedenceConstraints",nsmgr);
                        foreach (XmlNode nd in PrecendenceConstraintsList.ChildNodes)
                        {
                            if (nd.Attributes.GetNamedItem("DTS:From") != null && nd.Attributes.GetNamedItem("DTS:To") != null)
                            {
                                dataSetFrom.Add(nd.Attributes.GetNamedItem("DTS:From").Value.ToString());
                                dataSetTo.Add(nd.Attributes.GetNamedItem("DTS:To").Value.ToString());
                                dataSetRevFrom.Add(nd.Attributes.GetNamedItem("DTS:To").Value.ToString());
                                dataSetRevTo.Add(nd.Attributes.GetNamedItem("DTS:From").Value.ToString());
                             }
                        }
                        GetSSISComponentSequence(new List<string>(dataSetTo),new List<string>(dataSetFrom),new List<string>(dataSetRevTo),new List<string>(dataSetRevFrom));
                        writeFile.WriteLine("//////Start analysis of PackageParameters//////");
                        XmlNode root = doc.SelectSingleNode("DTS:Executable",nsmgr).SelectSingleNode("DTS:PackageParameters",nsmgr); 
                        ReadXMLToTxt(root,nsmgr,writeFile);
                        writeFile.WriteLine("//////End analysis of PackageParameters//////");
                        writeFile.WriteLine("//////Start analysis of Variables//////");
                        root = doc.SelectSingleNode("DTS:Executable",nsmgr).SelectSingleNode("DTS:Variables",nsmgr); 
                        ReadXMLToTxt(root,nsmgr,writeFile);
                        writeFile.WriteLine("//////End analysis of Variables//////");
                        for(int i=0;i<DependencyConstraints.Count;i++)
                        {
                            writeFile.WriteLine("//////Start analysis of SSIS component " + DependencyConstraints[i].ToString() + "//////");
                            root = doc.SelectSingleNode("DTS:Executable",nsmgr).SelectSingleNode("DTS:Executables",nsmgr).SelectSingleNode("DTS:Executable[@DTS:refId='"+DependencyConstraints[i].ToString()+"']",nsmgr); 
                            ReadXMLToTxt(root,nsmgr,writeFile);
                            writeFile.WriteLine("//////End analysis of SSIS component " + DependencyConstraints[i].ToString() + "//////");
                                            
                        }
                        root = doc.SelectSingleNode("DTS:Executable",nsmgr).SelectSingleNode("DTS:PrecedenceConstraints",nsmgr); 
                        ReadXMLToTxt(root,nsmgr,writeFile);
                        root = doc.SelectSingleNode("DTS:Executable",nsmgr).SelectSingleNode("DTS:EventHandlers",nsmgr); 
                        ReadXMLToTxt(root,nsmgr,writeFile);

                        
                       
                        
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

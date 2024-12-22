using Crocus.AppManager;
using Crocus.Common;
using Crocus.DataManager;
using System;
using System.Data;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace bncmc_payroll.admin
{
    public partial class util_QueryAnalizer : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            CommonLogic.SetMySiteName(this, "Admin :: SQL Query Analizar", true, true, true);

            if (!Page.IsPostBack)
            {
                pnlGrid.Visible = false;
                TreeNode tn = new TreeNode();
                tn.SelectAction = TreeNodeSelectAction.None;
                tn.Text = "Database";
                tn.Value = "0";
                tvSQLServer.Nodes.Add(tn);
                DataTable Dt = DataConn.GetTable("SELECT ID, ParentID, Name from fn_GetDBObjects();");
                PopulateNodes(tn.ChildNodes, 0, Dt);
                ChkdNode(tvSQLServer.Nodes);
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            Label lblNote = new Label();
            Label lblBlank = new Label();
            GridView Grdview1 = new GridView();
            try
            {
                DataSet Ds = new DataSet();
                string sSeleted = hidSelectedText.Value;
                string sQry = "";

                if (sSeleted != "")
                    sQry = sSeleted;
                else
                    sQry = text.Value.Trim();

                string sCommand=sQry.Substring(0,6);

                if ((sCommand.ToUpper() == "INSERT")||(sCommand.ToUpper() == "UPDATE")||(sCommand.ToUpper() == "DELETE"))     /*  IF Query is for SELECT */
                {
                    try
                    {
                        lblNote = new Label();
                        lblNote.EnableViewState = false;
                        if (DataConn.ExecuteSQL(sQry) == 0)
                            lblNote.Text = sCommand + " Query Executed Successfully..";
                        else
                            lblNote.Text = "Error Executing  " + sCommand + " Query";
                        pnlGrid.Controls.Add(lblNote);
                    }
                    catch { }
                }
                else /*  IF Query is for Insert/UPDATE/DELETE */
                {
                    try
                    {

                        Ds = DataConn.GetDS(sQry, false, true);
                        for (int i = 0; i <= Ds.Tables.Count - 1; i++)
                        {
                            if (Ds.Tables[i].Rows.Count > 0)
                            {
                                Grdview1 = new GridView();
                                Grdview1.HeaderStyle.BackColor = System.Drawing.Color.Gray;
                                Grdview1.HeaderStyle.ForeColor = System.Drawing.Color.White;
                                Grdview1.EnableTheming = false;
                                Grdview1.AutoGenerateColumns = true;
                                Grdview1.HeaderStyle.BorderStyle = BorderStyle.Solid;
                                Grdview1.HeaderStyle.BorderColor = System.Drawing.Color.Black;
                                Grdview1.BorderColor = System.Drawing.Color.Black;
                                Grdview1.ShowFooter = false;
                                //Grdview1.FooterStyle.BackColor = System.Drawing.Color.Gray;
                                Grdview1.DataSource = Ds.Tables[i];
                                Grdview1.DataBind();
                                lblNote = new Label();
                                lblNote.BackColor = System.Drawing.Color.GhostWhite;
                                lblNote.EnableViewState = false;

                                lblNote.Text = "Query Executed Successfully..   (" + Ds.Tables[i].Rows.Count + " row(s) affected)";
                                lblBlank.EnableViewState = false;
                                lblBlank.Text = "";
                                pnlGrid.Controls.Add(Grdview1);
                                pnlGrid.Controls.Add(lblNote);
                                pnlGrid.Controls.Add(lblBlank);

                                pnlGrid.Visible = true;
                            }
                        }
                    }
                    catch (Exception ex) { AlertBox(ex.Message, "", ""); }

                    
                }

            }
            catch
            { lblNote.Text = "Error Executing Query .."; }
        }

        private void AlertBox(string strMsg, string strredirectpg, string pClose)
        {
            ScriptManager.RegisterStartupScript((Page)this, GetType(), "show", Commoncls.AlertBoxContent(strMsg, strredirectpg, pClose), true);
        }
        
        #region "Generate SQL Server Object Hierarchy"
        private void PopulateNodes(TreeNodeCollection nodes, int TreeID, DataTable Dt)
        {
            try
            {
                DataRow[] rst_Records = Dt.Select("ParentID=" + TreeID);
                if (rst_Records.Length > 0)
                    foreach (DataRow dr in rst_Records)
                    {
                        TreeNode tn = new TreeNode();
                        tn.SelectAction = TreeNodeSelectAction.None;
                        tn.Text = dr["Name"].ToString();
                        tn.Value = dr["ID"].ToString();
                        nodes.Add(tn);
                        PopulateNodes(tn.ChildNodes, Localization.ParseNativeInt(dr["ID"].ToString()), Dt);
                        tn.CollapseAll();
                    }
            }
            catch
            { }
        }

        protected void ChkdNode(TreeNodeCollection nodes)
        {
            string strQry = string.Empty;
            TreeNode tn = nodes[0];
            tn.SelectAction = TreeNodeSelectAction.None;
            string sRec = "";
            sRec = "SELECT ID from fn_GetDBObjects();";

            using (DataTable Dt_Rec = DataConn.GetTable(sRec))
            {
                for (int i = 0; i < tn.ChildNodes.Count; i++)
                {
                    int ModulID = 0;
                    DataRow[] rst_ModuleID = Dt_Rec.Select("ID=" + tn.ChildNodes[i].Value);
                    if (rst_ModuleID.Length > 0)
                        foreach (DataRow r in rst_ModuleID)
                        { ModulID = Localization.ParseNativeInt(r["ID"].ToString()); break; }
                    else
                        ModulID = 0;

                    if (ModulID == Localization.ParseNativeInt(tn.ChildNodes[i].Value))
                    { tn.ChildNodes[i].Checked = true; }
                }
            }

        }

        #endregion

    }
}
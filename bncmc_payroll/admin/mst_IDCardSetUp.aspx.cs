using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Crocus.Common;
using System.Data;
using Crocus.AppManager;
using Crocus.DataManager;

namespace bncmc_payroll.admin
{
    public partial class mst_IDCardSetUp : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            CommonLogic.SetMySiteName(this, "Admin - ID Card Image", true, true, true);
            viewimg();
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            string sPath = string.Empty;
            if (!System.IO.Directory.Exists(Server.MapPath("..\\" + "IDS_Imgpath") + "\\"))
                System.IO.Directory.CreateDirectory(Server.MapPath("..\\" + "IDS_Imgpath"));
            //if (System.IO.Directory.Exists(sPath) == false) System.IO.Directory.CreateDirectory(sPath);
            if (FileUpload1.FileContent.Length > 0)
                try
                {
                    if (FileUpload1.PostedFile.ContentLength > 0 || FileUpload1.FileName.Length > 0)
                    {
                        sPath = Server.MapPath("..\\" + "IDS_Imgpath") + "\\" + "H_1.gif";
                        FileUpload1.SaveAs(sPath);

                    }
                }
                catch (Exception ex)
                {
                    Commoncls.TraceError(ex.Message);
                }

            if (FileUpload2.FileContent.Length > 0)
                try
                {
                    if (FileUpload2.PostedFile.ContentLength > 0 || FileUpload2.FileName.Length > 0)
                    {
                        sPath = Server.MapPath("..\\" + "IDS_Imgpath") + "\\" + "V_1.gif";
                        FileUpload2.SaveAs(sPath);

                    }
                }
                catch (Exception ex)
                {
                    Commoncls.TraceError(ex.Message);
                }



            //if (FileUpload1.HasFile)
            //    Commoncls.Uploadfile(FileUpload1, "IDS_Imgpath", "Image_Medium", 1, "H");
            //if (FileUpload2.HasFile)
            //    Commoncls.Uploadfile(FileUpload2, "IDS_Imgpath", "Image_Medium", 1, "V");

            viewimg();
        }

        protected void viewimg()
        {
            //string sPath = System.Web.Hosting.HostingEnvironment.MapPath("~") + AppSettings.AppConfig("Institute_Logo") + "\\" + iDr["InsDtlID"].ToString() + "_2.gif";
            string sPath = System.Web.Hosting.HostingEnvironment.MapPath("~") + "IDs_Imgpath" + "\\" + "H_1.gif";
            if (System.IO.File.Exists(sPath))
                imgH.ImageUrl = "../" + "IDS_Imgpath" + "/" + "H_1.gif";
            //".." + (AppSettings.AppConfig("IDS_Imgpath") + "/" +  "H_1.gif").Replace("\\\\", "/");

            string sPath1 = System.Web.Hosting.HostingEnvironment.MapPath("~") + "IDS_Imgpath" + "\\" + "V_1.gif";
            if (System.IO.File.Exists(sPath))
                imgV.ImageUrl = "../" + "IDS_Imgpath" + "/" + "V_1.gif";
            //".." + (AppSettings.AppConfig("IDS_Imgpath") + "/" +  "V_1.gif").Replace("\\\\", "/");

        }
    }
}
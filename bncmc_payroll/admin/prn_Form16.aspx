<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="prn_Form16.aspx.cs" Inherits="bncmc_payroll.admin.prn_Form16" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
     <script type="text/javascript">
         function printME() {
             document.getElementById("btnPrint").style.display = "none";
             window.print();
             document.getElementById("btnPrint").style.display = "";
         }        
    </script>
    <style type="text/css">
        /*Print and Cancel button always visible*/
.visibleDiv, #bottomRight
{
    position: fixed;
    width: 250px;
    border-radius: 0.4em 0.4em 0.4em 0.4em;
    vertical-align: middle;
    text-align: right;
    float:right;
}

#bottomRight
{
    bottom: 1px;
    right: 10px;
}

#bottomRight_Text
{
    bottom: 1px;
    right: 10px;
}
/**/

		.LineRight
		{
			border-right:0.5px solid #424242
		}
		.LineLeft
		{
			border-left:0.5px solid #424242
		}
		.LineBottom
		{
			border-bottom:0.5px solid #424242
		}
		.LineTop
		{
			border-top:0.5px solid #424242
		}
		.LineAll
		{
			border:0.5px solid #424242
		}

		.textBold
		{
			font-weight:bold;
			text-align:Center;
			font-size:15pt;
		}
		
		 .table
        {
            border-top: 1px solid #e5eff8;
            border-right: 1px solid #e5eff8;
            margin: 1em auto;
            border-collapse: collapse;
            color: Black;
            background-color:#fff;
        }
        
		 .table tr th
        {
            border-top: 1px solid #e5eff8;
            text-align: center;
            font: bold  "Century Gothic" , "Trebuchet MS" ,Arial,Helvetica,sans-serif;
            background-color:#335C91;
            color:#fff;
             border-right: 1px solid #e5eff8;
        }


        .table tr.odd td
        {
            background: #f7fbff;
             border-right: 1px solid #e5eff8;
        }


        .table td
        {
            border-bottom: 1px solid #e5eff8;
            border-left: 1px solid #e5eff8;
            padding: .3em;
            text-align: left;
             border-right: 1px solid #e5eff8;
        }
	</style>
</head>
<body style="background-color:White;">
    
    <form id="form1" runat="server" >
        <div>
            <asp:Literal ID="ltrContent" runat="server" />
             <br/><br/><br/>
            <div id="bottomRight">
            <table width="100%" cellpadding="2" cellspacing="2" border="0" >
                <tr>
                    <td colspan="3" align="right">
                        <input type="button" class="button" id="btnPrint" value= "Print" onclick="printME();" /> &nbsp;
                    </td>
                </tr>
            </table>
             </div>
        </div>
    </form>
</body>
</html>

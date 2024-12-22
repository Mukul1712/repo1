<%@ Page Language="C#" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="default.aspx.cs" Inherits="bncmc_payroll._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Bhiwandi Nizampur City Municipal Corporation</title>
    <link href="admin/css/button.css" rel="stylesheet" type="text/css" />
		<style type="text/css">
			* { font-family: "Helvetica Neue", Helvetica, Arial, sans-serif; }
			
			body {
				margin: 0;
				pading: 0;
				color: #fff;
				background: url('admin/images/bg-login.png') repeat #1b1b1b;
				font-size: 14px;
				text-shadow: #050505 0 -1px 0;
				font-weight: bold;
			}
			
			li {
				list-style: none;
			}
			
			#dummy {
				position: absolute;
				top: 0;
				left: 0;
				border-bottom: solid 3px #777973;
				height: 250px;
				width: 100%;
				background: url('admin/images/bg-login-top.png') repeat #fff;
				z-index: 1;
			}
			
			#dummy2 {
				position: absolute;
				top: 0;
				left: 0;
				border-bottom: solid 2px #545551;
				height: 252px;
				width: 100%;
				background: transparent;
				z-index: 2;
			}
			
			#login-wrapper {
				margin: 0 0 0 -200px;
				width: 350px;
				text-align: center;
				z-index: 99;
				position: absolute;
				top: 0;
				left: 50%;
			}
			
			#login-top {
				height: 150px;
				padding-top: 110px;
				padding-left: 70px;
				text-align: center;
			}
			
			label {
				width: 90px;
				float: left;
				padding: 8px;
				line-height: 14px;
				margin-top: -4px;
			}
			
			input.text-input {
				width: 200px;
				float: right;
				-moz-border-radius: 4px;
                -webkit-border-radius: 4px;
				border-radius: 4px;
				background: #fff;
				border: solid 1px transparent;
				color: #555;
				padding: 8px;
				font-size: 13px;
			}
			
			input.button {
				float: right;
				padding: 6px 10px;
				color: #fff;
				font-size: 14px;
				background: -webkit-gradient(linear, 0% 0%, 0% 100%, from(#a4d04a), to(#459300));
				text-shadow: #050505 0 -1px 0;
				background-color: #459300;
				-moz-border-radius: 4px;
                -webkit-border-radius: 4px;
				border-radius: 4px;
				border: solid 1px transparent;
				font-weight: bold;
				cursor: pointer;
				letter-spacing: 1px;
			}
			
			input.button:hover {
				background: -webkit-gradient(linear, 0% 0%, 0% 100%, from(#a4d04a), to(#a4d04a), color-stop(80%, #76b226));
				text-shadow: #050505 0 -1px 2px;
				background-color: #a4d04a;
				color: #fff;
			}
			
			div.error {
				padding: 8px;
				background: rgba(52, 4, 0, 0.4);
				-moz-border-radius: 8px;
                -webkit-border-radius: 8px;
				border-radius: 8px;
				border: solid 1px transparent;
				margin: 6px 0;
			}
			

            table.form {
	            width:100%;
            }

            table.form td {
	            padding:2px 0px;
            }

		</style>
</head>

<body class="special-page">
    <form id="form1" runat="server">
        <uc_ajax:ToolkitScriptManager runat="Server" EnablePartialRendering="true" ID="ScriptManager1" />
          <!-- Begin of #container -->
		    <div id="login-wrapper" class="png_bg">
			    <div id="login-top">
				    <img src="admin/images/logo.gif" alt="Bhiwandi Nizampur City Municipal Corporation" title="Bhiwandi Nizampur City Municipal Corporation" />
			    </div>
			
			    <div id="login-content">
                    <p>
					    <label>Login Type</label>
                        <asp:RadioButtonList ID="rdbUserType" runat="server" RepeatDirection="Horizontal" CellPadding="2" CellSpacing="2">
                            <asp:ListItem Text="Payroll" Value="1" Selected="True"/>
                            <asp:ListItem Text="Employee" Value="2" />
                        </asp:RadioButtonList>
                    </p>
                    <p>
					    <label>Username</label>
                        <input type="text" name="txtUserName" value="" class="text-input" runat="server" id="txtUserName" tabindex="1" />
                    </p>
					<br style="clear: both;" />
					
                    <p>
					    <label>Password</label>
                        <input type="password" name="txtpassword" id="txtpassword" value="" class="text-input" runat="server" tabindex="2"/>
                    </p>

                    <br style="clear: both;" />

                    

					
                    <p style="padding-right:70px;">
                        <asp:Button ID="btnLogin" runat="server" Text="Log in" CssClass="button"  OnClientClick="submitform()" OnClick="btnLogin_Click" tabindex="3" />
                        <asp:HiddenField ID="hfFormValidated" Value='false' runat="server" />
                    </p>
			    </div>
		    </div>
		    <div id="dummy"></div>
		    <div id="dummy2"></div>
             <asp:Panel ID="pnl_Month" runat="server" style="display:none">
                <div id="div_SettingHead"style="width: 450px; height: 50px; background-color:#5E610B;
                    border: 2px solid #000000;text-align:center;cursor:move;" >
                    <h3>SELECT MONTH & YEAR</h3>
                </div>
                <div style="overflow: auto; width: 450px; height: 100px; background-color: #3A2F0B;color:White;border: 2px solid #000000">
					    <table width="70%" border="0" cellpadding="2" cellspacing="0" style="padding: 30px 10px 10px 10px;" class="form">
                            <tr>
                                <td width="10%">&nbsp;</td>
                                <td width="30%" style="font-size:15px;">Login Year :</td>
                                <td width="60%" >
                                    <asp:DropDownList ID="ddl_Year"  runat="server" /> <%--OnSelectedIndexChanged="ddl_Year_SelectedIndexChanged" AutoPostBack="true"--%>
                                    <uc_ajax:CascadingDropDown ID="ccd_Year" runat="server" Category="Year" TargetControlID="ddl_Year" 
                                        LoadingText="Loading Year..."  ServiceMethod="BindYears"
                                        ServicePath="~/ws/FillCombo.asmx"/>
                                </td>
                            </tr>

                            <tr>
                                <td width="10%">&nbsp;</td>
                                <td width="30%" style="font-size:15px;">Login Month :</td>
                                <td width="60%" >
                                    <asp:DropDownList ID="ddl_Month"  runat="server" />
                                    <uc_ajax:CascadingDropDown ID="ccd_Month" runat="server" Category="Month" TargetControlID="ddl_Month" 
                                        LoadingText="Loading Month..." PromptText="-- ALL MONTH --" ServiceMethod="BindMonth" ParentControlID="ddl_Year"
                                        ServicePath="~/ws/FillCombo.asmx"/>
                                </td>
                            </tr>
                        </table>
                </div>

                <div style="width: 450px; background-color: #5E610B; text-align: center; border: 2px solid #000000;color:White;">
                    <asp:Button ID="btnSave" runat="server" Text="Select" CssClass="btn btn-blue"
                        OnClick="btnSave_Click" />
                    <asp:Button ID="btncancelSettings" runat="server"  CssClass="btn btn-blue" Text="Cancel"/>
                </div>
      
            </asp:Panel>

            <asp:Button id="btnShowPopup" runat="server" style="display:none" />
            <uc_ajax:ModalPopupExtender BackgroundCssClass="modalBackground" runat="server" PopupControlID="pnl_Month" 
                ID="MPE_Month" CancelControlID="btncancelSettings" Drag="false" PopupDragHandleControlID="div_SettingHead" 
                TargetControlID="btnShowPopup"/>
          <!--! end of #container -->
    </form>

    <script type="text/javascript">
        document.getElementById('<%= txtUserName.ClientID %>').focus();
    </script>

    <script type="text/javascript">

        function submitform() {
            var myUserName = document.getElementById('<%= txtUserName.ClientID %>');
            var myPwd = document.getElementById('<%= txtpassword.ClientID %>');
            var hfFormValidated = document.getElementById('<%= hfFormValidated.ClientID %>');
            if ((myUserName.value == '') && (myPwd.value == '')) {
                alert('Please Enter UserName and Password');
                hfFormValidated.value = false;
                return;
            }
            else if (myUserName.value == '') {
                alert('Please Enter UserName');
                hfFormValidated.value = false;
                return;
            }
            else if (myPwd.value == '') {
                alert('Please Enter Password');
                hfFormValidated.value = false;
                return;
            }
            else
                hfFormValidated.value = true;
        }
    </script>
</body>
</html>

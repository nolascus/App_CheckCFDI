<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="App_CheckCFDI_AspNet.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
        Archivo ZIP :
        <asp:FileUpload ID="XMLZipFile" runat="server" />
&nbsp;<asp:Button ID="btnProcesar" runat="server" onclick="btnProcesar_Click" 
            Text="Procesar Archivo ZIP" />
        <br />
        <asp:GridView ID="GridView1" runat="server">
        </asp:GridView>
        <asp:Literal ID="lblSalida" runat="server"></asp:Literal>
    
    </div>
    </form>
</body>
</html>

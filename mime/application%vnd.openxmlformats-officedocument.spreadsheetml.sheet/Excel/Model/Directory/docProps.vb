Imports Microsoft.VisualBasic.MIME.Office.Excel.XML.docProps

Public Class docProps : Inherits Directory

    Public Property core As core
    Public Property app As XML.docProps.app
    Public Property custom As custom

    Sub New(ROOT$)
        Call MyBase.New(ROOT)
    End Sub

    ''' <summary>
    ''' 有些文件可能是会不存在的，所以在这里就不抛出错误了，直接返回Nothing
    ''' </summary>
    Protected Overrides Sub _loadContents()
        core = (Folder & "/core.xml").LoadXml(Of core)(ThrowEx:=False)
        custom = (Folder & "/custom.xml").LoadXml(Of custom)(ThrowEx:=False)
        app = (Folder & "/app.xml").LoadXml(Of XML.docProps.app)(ThrowEx:=False)
    End Sub

    Protected Overrides Function _name() As String
        Return NameOf(docProps)
    End Function
End Class
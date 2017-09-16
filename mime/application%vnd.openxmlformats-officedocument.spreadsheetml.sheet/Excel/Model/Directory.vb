Imports System.Text
Imports Microsoft.VisualBasic.MIME.Office.Excel.XML._rels
Imports Microsoft.VisualBasic.MIME.Office.Excel.XML.docProps

Public Class _rels : Inherits Directory

    Sub New(ROOT$)
        Call MyBase.New(ROOT)
    End Sub

    Public Property rels As rels

    Protected Overrides Sub _loadContents()
        rels = (Folder & "/.rels").LoadXml(Of rels)(Encoding.UTF8)
    End Sub

    Protected Overrides Function _name() As String
        Return NameOf(Excel._rels)
    End Function
End Class

Public Class docProps : Inherits Directory

    Public Property core As core
    Public Property app As XML.docProps.app
    Public Property custom As custom

    Sub New(ROOT$)
        Call MyBase.New(ROOT)
    End Sub

    Protected Overrides Sub _loadContents()
        core = (Folder & "/core.xml").LoadXml(Of core)(Encoding.UTF8)
        custom = (Folder & "/custom.xml").LoadXml(Of custom)(Encoding.UTF8)
        app = (Folder & "/app.xml").LoadXml(Of XML.docProps.app)(Encoding.UTF8)
    End Sub

    Protected Overrides Function _name() As String
        Return NameOf(docProps)
    End Function
End Class

Public Class xl : Inherits Directory

    Sub New(ROOT$)
        Call MyBase.New(ROOT)
    End Sub

    Protected Overrides Sub _loadContents()

    End Sub

    Protected Overrides Function _name() As String
        Return NameOf(xl)
    End Function
End Class

Public MustInherit Class Directory

    Public ReadOnly Property Folder As String

    Sub New(ROOT$)
        Folder = $"{ROOT}/{_name()}"
        Call _loadContents()
    End Sub

    Protected MustOverride Function _name() As String
    Protected MustOverride Sub _loadContents()

    Public Overrides Function ToString() As String
        Return Folder
    End Function
End Class
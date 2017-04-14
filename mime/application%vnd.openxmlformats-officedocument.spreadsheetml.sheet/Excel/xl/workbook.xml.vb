Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace xl

    Public Class workbook
        Public Property fileVersion As fileVersion
        Public Property bookViews As workbookView()
        Public Property sheets As sheet()
        Public Property calcPr As calcPr
    End Class

    Public Structure calcPr
        <XmlAttribute> Public Property calcId As String
    End Structure

    Public Structure sheet
        <XmlAttribute> Public Property name As String
        <XmlAttribute> Public Property sheetId As String
        <XmlAttribute> Public Property rid As String
    End Structure

    Public Structure workbookView
        <XmlAttribute> Public Property xWindow As String
        <XmlAttribute> Public Property yWindow As String
        <XmlAttribute> Public Property windowWidth As String
        <XmlAttribute> Public Property windowHeight As String
        <XmlAttribute> Public Property activeTab As String
    End Structure

    Public Structure fileVersion

        <XmlAttribute> Public Property appName As String
        <XmlAttribute> Public Property lastEdited As String
        <XmlAttribute> Public Property lowestEdited As String
        <XmlAttribute> Public Property rupBuild As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure
End Namespace



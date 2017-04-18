Imports System.Xml.Serialization

Namespace xl.worksheets

    Public Class worksheet
        Public Property sheetData As row()
    End Class

    Public Structure dimension
        <XmlAttribute> Public Property ref As String
    End Structure

    Public Structure row
        <XmlAttribute> Public Property r As String
        <XmlAttribute> Public Property spans As String
        <XmlAttribute> Public Property ht As String
        <XmlAttribute> Public Property customHeight As String
        <XmlAttribute> Public Property customFormat As String
        <XmlElement("c")> Public Property columns As c()
    End Structure

    Public Structure c
        <XmlAttribute> Public Property r As String
        <XmlAttribute> Public Property s As String
        <XmlAttribute> Public Property t As String
        Public Property v As String
    End Structure
End Namespace
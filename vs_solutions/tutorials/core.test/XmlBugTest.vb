Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel

Module XmlBugTest

    Sub Main()

        Dim xml = New XmlCommentBug With {
            .sss1 = "sfsfsfsf",
            .sss2 = False,
            .sss3 = "sssss", .sss4 = 4342423,
            .sss5 = New textNode With {.largeText = "aaaa
            sd
fs
fs
fsd
f
sdfsdddsfdf
sd
fsd
f
sd
fsd
f
sd
fsd
ffffffffffffffffff  <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


aaa"},
            .sss6 = {"eeeeee", "asdasda", "gggggggggggg"},
            .list = {New propertyType With {.s = "sffsdfsd", .t = Now}, New propertyType With {.s = "eeeeee", .t = "8888888"}},
            .list2 = .list
        }.GetXml

        ' removes xml comment
        xml = xml.StringReplace("<![-]{2}.+?[-]{2}>", "").Replace("utf-16", "utf-8").Replace(<xmlns>xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"</xmlns>.Value, "")

        ' reload
        Dim reloadObject As XmlCommentBug = xml.LoadFromXml(Of XmlCommentBug)


        Pause()
    End Sub
End Module

Public Class XmlCommentBug : Inherits XmlDataModel

    Public Property sss1 As String
    Public Property sss2 As Boolean
    Public Property sss3 As String
    Public Property sss4 As Long
    Public Property sss5 As textNode
    <XmlElement>
    Public Property sss6 As String()

    <XmlElement("list222")>
    Public Property list As propertyType()
    <XmlElement("list333")>
    Public Property list2 As propertyType()
End Class

Public Class textNode
    Public Property largeText As String
End Class

Public Class propertyType
    Public Property s As String
    Public Property t As String
End Class
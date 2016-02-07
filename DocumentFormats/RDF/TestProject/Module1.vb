Imports Microsoft.VisualBasic.DocumentFormat.RDF

Module Module1

    Sub Main()

        Dim xeee As New ee With {.Resource = "http://www.w3school.com.cn/RDF", .author = "David", .homepage = "http://www.w3school.com.cn"}

        Call New RDF With {.Entity = xeee}.SaveAsXml("x:\test_rdf.xml")

        Dim x As New RDFD With {.CDList = {New RDFD.CD With {.Artist = ""}}}
        Call x.SaveAsXml("x:\test2.rdf.xml")
    End Sub
End Module

Public Class RDF
    Public Property Entity As ee
End Class

Public Class ee : Inherits RDFEntity
    Public Property author As String
    Public Property homepage As String
End Class

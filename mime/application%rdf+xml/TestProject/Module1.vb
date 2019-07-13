#Region "Microsoft.VisualBasic::b2d749f01506c88d078a05e62622c4d4, mime\application%rdf+xml\TestProject\Module1.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Module Module1
    ' 
    '     Sub: Main
    ' 
    ' Class RDF
    ' 
    '     Properties: Entity
    ' 
    ' Class ee
    ' 
    '     Properties: author, homepage
    ' 
    ' /********************************************************************************/

#End Region

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

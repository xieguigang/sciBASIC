Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class mesh

    Public Property vertices As Point3D()
    Public Property triangles As triangle()

    Public Function GetSurfaces() As Drawing3D.Surface()
        Dim out As New List(Of Drawing3D.Surface)

        For Each t As triangle In triangles
            out += New Drawing3D.Surface With {
                .vertices = {
                    vertices(t.v1), vertices(t.v2), vertices(t.v3)
                }
            }
        Next

        Return out
    End Function

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class

Public Class triangle

    <XmlAttribute> Public Property v1 As Integer
    <XmlAttribute> Public Property v2 As Integer
    <XmlAttribute> Public Property v3 As Integer

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class

Public Class component

    <XmlAttribute>
    Public Property objectid As Integer
End Class
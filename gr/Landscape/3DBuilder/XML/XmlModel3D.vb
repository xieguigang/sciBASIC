Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' ``3D/3dmodel.model`` xml file.
''' </summary>
''' 
<XmlRoot("model")> Public Class XmlModel3D

    <XmlAttribute>
    Public Property unit As String
    Public Property resources As resources
    Public Property build As build

    Public Function GetSurfaces() As IEnumerable(Of Drawing3D.Surface)
        Dim out As New List(Of Drawing3D.Surface)

        On Error Resume Next

        For Each obj As SeqValue(Of [object]) In resources _
            .objects _
            .Where(AddressOf NotNull) _
            .SeqIterator

            Dim base As base = resources _
                .basematerials _
                .basematerials(obj)

            out += (+obj) _
                .mesh _
                .GetSurfaces(base)
        Next

        Return out
    End Function
End Class

Public Class build

    <XmlElement("item")> Public Property items As item()

    Public Overrides Function ToString() As String
        Return items.GetJson
    End Function
End Class

Public Class item

    <XmlAttribute> Public Property objectid As Integer
    <XmlAttribute> Public Property transform As Double()

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class
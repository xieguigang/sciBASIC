#Region "Microsoft.VisualBasic::a632281d279f73325aaf4f54d6a1fcd7, gr\Landscape\3DBuilder\XML\XmlModel3D.vb"

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

    '     Class XmlModel3D
    ' 
    '         Properties: build, resources, unit
    ' 
    '         Function: GetSurfaces
    ' 
    '     Class build
    ' 
    '         Properties: items
    ' 
    '         Function: ToString
    ' 
    '     Class item
    ' 
    '         Properties: objectid, transform
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Vendor_3mf.XML

    ''' <summary>
    ''' ``3D/3dmodel.model`` xml file.
    ''' </summary>
    ''' 
    <XmlRoot("model")> Public Class XmlModel3D

        <XmlAttribute>
        Public Property unit As String
        Public Property resources As resources
        Public Property build As build

        Public Function GetSurfaces() As IEnumerable(Of Surface)
            Dim out As New List(Of Surface)
            Dim objects As [object]() = resources _
                .objects _
                .Where(AddressOf NotNull) _
                .ToArray
            Dim materials As Brush() = resources _
                .basematerials _
                .basematerials _
                .Select(Function(b) b.displaycolor.TranslateColor) _
                .Select(Function(c) New SolidBrush(c)) _
                .ToArray

            On Error Resume Next

            For Each obj As [object] In objects
                If obj.pindex Is Nothing Then
                    ' 使用三角面自己的资源编号
                    out += obj.mesh.GetSurfaces(materials)
                Else
                    ' 使用总编号
                    Dim base As base = resources _
                        .basematerials _
                        .basematerials(CInt(obj.pindex))

                    out += obj _
                        .mesh _
                        .GetSurfaces(base)
                End If
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
End Namespace

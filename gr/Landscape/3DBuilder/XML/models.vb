#Region "Microsoft.VisualBasic::b0cc1091df879e9e45e2d892c87e5eb6, gr\Landscape\3DBuilder\XML\models.vb"

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

    '     Class mesh
    ' 
    '         Properties: triangles, vertices
    ' 
    '         Function: (+2 Overloads) GetSurfaces, ToString
    ' 
    '     Class triangle
    ' 
    '         Properties: p1, pid, v1, v2, v3
    ' 
    '         Function: ToString
    ' 
    '     Class component
    ' 
    '         Properties: objectid
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Vendor_3mf.XML

    Public Class mesh

        Public Property vertices As Point3D()
        Public Property triangles As triangle()

        Public Function GetSurfaces(base As base) As Surface()
            Dim out As New List(Of Surface)
            Dim color As Color = base.displaycolor.TranslateColor
            Dim b As New SolidBrush(color)

            For Each t As triangle In triangles
                out += New Surface With {
                    .vertices = {
                        vertices(t.v1), vertices(t.v2), vertices(t.v3)
                    },
                    .brush = b
                }
            Next

            Return out
        End Function

        ''' <summary>
        ''' <see cref="triangle.p1"/>
        ''' </summary>
        ''' <param name="materials"></param>
        ''' <returns></returns>
        Public Function GetSurfaces(materials As Brush()) As Surface()
            Dim out As New List(Of Surface)

            For Each t As triangle In triangles
                Dim i As Integer = CInt(t.p1)

                out += New Surface With {
                    .vertices = {
                        vertices(t.v1), vertices(t.v2), vertices(t.v3)
                    },
                    .brush = materials(i)
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

        ''' <summary>
        ''' 当前的这个三角形面的所属组件的编号
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property pid As String
        ''' <summary>
        ''' 颜色索引值
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property p1 As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

    Public Class component

        <XmlAttribute>
        Public Property objectid As Integer
    End Class
End Namespace

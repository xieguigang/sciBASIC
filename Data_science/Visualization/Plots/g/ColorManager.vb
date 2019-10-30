Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors

Namespace Graphic

    Public MustInherit Class ColorProfile

        Protected colors As Color()

        Sub New(colorSchema As String)
            colors = Designer.GetColors(colorSchema)
        End Sub

        Public MustOverride Function GetColor(item As NamedValue(Of Double)) As Color

    End Class

    Public Class CategoryColorProfile : Inherits ColorProfile

        ReadOnly category As Dictionary(Of String, String)
        ReadOnly categoryIndex As Index(Of String)

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="category">[term => category label]</param>
        ''' <param name="colorSchema$"></param>
        Sub New(category As Dictionary(Of String, String), colorSchema$)
            Call MyBase.New(colorSchema)

            Me.category = category
            Me.categoryIndex = category.Values.Indexing

            If colors.Length < categoryIndex.Count Then
                colors = Designer.CubicSpline(colors, n:=categoryIndex.Count)
            End If
        End Sub

        Public Overrides Function GetColor(item As NamedValue(Of Double)) As Color
            Dim category As String = Me.category(item.Name)
            Dim i As Integer = categoryIndex(category)

            Return colors(i)
        End Function
    End Class

    ''' <summary>
    ''' scale(color_schema_term_name)
    ''' </summary>
    Public Class ValueScaleColorProfile : Inherits ColorProfile

        Dim valueRange As DoubleRange
        Dim indexRange As DoubleRange

        Public Sub New(data As IEnumerable(Of NamedValue(Of Double)), colorSchema$, level%)
            MyBase.New(colorSchema)

            With data.ToArray
                valueRange = .Select(Function(item) item.Value).ToArray
                colors = Designer.CubicSpline(colors, n:=level)
                indexRange = {0, .Length - 1}
            End With
        End Sub

        Public Overrides Function GetColor(item As NamedValue(Of Double)) As Color
            Dim index As Integer = valueRange.ScaleMapping(item.Value, indexRange)
            Dim color As Color = colors(index)

            Return color
        End Function
    End Class
End Namespace
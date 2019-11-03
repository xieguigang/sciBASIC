Imports System.Drawing
Imports System.Runtime.CompilerServices
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
            Me.categoryIndex = category.Values.Distinct.Indexing

            If colors.Length < categoryIndex.Count Then
                colors = Designer.CubicSpline(colors, n:=categoryIndex.Count)
            End If
        End Sub

        Public Overloads Function GetColor(termName As String) As Color
            Dim category As String = Me.category(termName)
            Dim i As Integer = categoryIndex.IndexOf(x:=category)

            Return colors(i)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function GetColor(item As NamedValue(Of Double)) As Color
            Return GetColor(termName:=item.Name)
        End Function
    End Class

    ''' <summary>
    ''' scale(color_schema_term_name)
    ''' </summary>
    Public Class ValueScaleColorProfile : Inherits ColorProfile

        Dim valueRange As DoubleRange
        Dim indexRange As DoubleRange
        Dim logarithm%

        Public Sub New(data As IEnumerable(Of NamedValue(Of Double)), colorSchema$, level%, Optional logarithm% = 0)
            MyBase.New(colorSchema)

            With data.ToArray
                valueRange = .Select(Function(item)
                                         If logarithm > 0 Then
                                             Return Math.Log(item.Value, logarithm)
                                         Else
                                             Return item.Value
                                         End If
                                     End Function) _
                             .ToArray
                indexRange = {0, .Length - 1}
                colors = Designer.CubicSpline(colors, n:=level)
            End With
        End Sub

        Public Overrides Function GetColor(item As NamedValue(Of Double)) As Color
            Dim termValue# = If(logarithm > 0, Math.Log(item.Value, logarithm), item.Value)
            Dim index As Integer = valueRange.ScaleMapping(termValue, indexRange)
            Dim color As Color = colors(index)

            Return color
        End Function
    End Class
End Namespace
Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace Drawing2D.Colors.Scaler

    ''' <summary>
    ''' mapping term to color, used for category group plot
    ''' </summary>
    Public Class CategoryColorProfile : Inherits ColorProfile

        ''' <summary>
        ''' the raw term inputs
        ''' </summary>
        ReadOnly category As Dictionary(Of String, String)
        ''' <summary>
        ''' [0...N]
        ''' </summary>
        ReadOnly categoryIndex As Index(Of String)

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="category">[term => category label]</param>
        ''' <param name="colorSchema">
        ''' Should be the term name for create color set via <see cref="Designer.GetColors(String)"/>
        ''' </param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(category As Dictionary(Of String, String), colorSchema$)
            Call Me.New(category, Designer.GetColors(colorSchema))
        End Sub

        Sub New(terms As IEnumerable(Of String), colorSet As String)
            Call Me.New(terms.Distinct.ToDictionary(Function(s) s), Designer.GetColors(colorSet))
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(terms As IEnumerable(Of String), colors As IEnumerable(Of String))
            Call Me.New(
                category:=terms.Distinct.ToDictionary(Function(s) s),
                colorSchema:=colors.Select(Function(c) c.TranslateColor)
            )
        End Sub

        Sub New(category As Dictionary(Of String, String), colorSchema As IEnumerable(Of Color))
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

End Namespace
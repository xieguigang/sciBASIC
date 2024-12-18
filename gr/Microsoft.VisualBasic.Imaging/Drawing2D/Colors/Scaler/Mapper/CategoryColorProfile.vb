#Region "Microsoft.VisualBasic::56058cb588624dad53b7c19ecc2e50a2, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Colors\Scaler\Mapper\CategoryColorProfile.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 93
    '    Code Lines: 56 (60.22%)
    ' Comment Lines: 21 (22.58%)
    '    - Xml Docs: 90.48%
    ' 
    '   Blank Lines: 16 (17.20%)
    '     File Size: 3.51 KB


    '     Class CategoryColorProfile
    ' 
    '         Properties: size
    ' 
    '         Constructor: (+5 Overloads) Sub New
    '         Function: (+2 Overloads) GetColor, GetTermColors
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq

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
        ''' get the size of the category collection
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property size As Integer
            Get
                Return categoryIndex.Count
            End Get
        End Property

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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(terms As IEnumerable(Of String), colors As IEnumerable(Of Color))
            Call Me.New(category:=terms.Distinct.ToDictionary(Function(s) s), colorSchema:=colors)
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

        Public Iterator Function GetTermColors() As IEnumerable(Of NamedValue(Of Color))
            For Each termMap As SeqValue(Of String) In categoryIndex
                Yield New NamedValue(Of Color)(termMap.value, colors(termMap))
            Next
        End Function
    End Class

End Namespace

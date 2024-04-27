#Region "Microsoft.VisualBasic::61a9040f4673b88bb9915963f316feea, G:/GCModeller/src/runtime/sciBASIC#/gr/Microsoft.VisualBasic.Imaging//Drawing2D/Colors/Scaler/Mapper/CategoryColorProfile.vb"

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

    '   Total Lines: 67
    '    Code Lines: 38
    ' Comment Lines: 17
    '   Blank Lines: 12
    '     File Size: 2.45 KB


    '     Class CategoryColorProfile
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: (+2 Overloads) GetColor
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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

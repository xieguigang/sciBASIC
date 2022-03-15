#Region "Microsoft.VisualBasic::3e3d9250e48d51b96de211eff0a7a5d0, sciBASIC#\Data_science\Visualization\Plots\g\ColorManager.vb"

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

    '   Total Lines: 99
    '    Code Lines: 70
    ' Comment Lines: 8
    '   Blank Lines: 21
    '     File Size: 3.56 KB


    '     Class ColorProfile
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class CategoryColorProfile
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+2 Overloads) GetColor
    ' 
    '     Class ValueScaleColorProfile
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetColor
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports stdNum = System.Math

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
                                             Return stdNum.Log(item.Value, logarithm)
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
            Dim termValue# = If(logarithm > 0, stdNum.Log(item.Value, logarithm), item.Value)
            Dim index As Integer = valueRange.ScaleMapping(termValue, indexRange)
            Dim color As Color

            If index >= colors.Length - 1 Then
                color = colors.Last
            ElseIf index <= 0 Then
                color = colors.First
            Else
                color = colors(index)
            End If

            Return color
        End Function
    End Class
End Namespace

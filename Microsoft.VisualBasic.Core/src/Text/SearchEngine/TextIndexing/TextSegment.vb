#Region "Microsoft.VisualBasic::ca8f970cd2596664119fd3e393d3bb94, Microsoft.VisualBasic.Core\src\Text\SearchEngine\TextIndexing\TextSegment.vb"

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

    '   Total Lines: 65
    '    Code Lines: 40
    ' Comment Lines: 15
    '   Blank Lines: 10
    '     File Size: 2.01 KB


    '     Class TextSegment
    ' 
    '         Properties: Array, Index, Segment
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    '         Operators: (+2 Overloads) Like
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.DynamicProgramming.Levenshtein
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace Text.Search

    ''' <summary>
    ''' 文本之中的一个片段
    ''' </summary>
    Public Class TextSegment

        Dim _text As String

        ''' <summary>
        ''' 当前的这个文本片段的字符串值
        ''' </summary>
        ''' <returns></returns>
        Public Property Segment As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return _text
            End Get
            Set(value As String)
                Dim ascii%() = value _
                    .Select(AddressOf AscW) _
                    .ToArray

                _Array = ascii
                _text = value
            End Set
        End Property

        ''' <summary>
        ''' ASCII值
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Array As Integer()
        ''' <summary>
        ''' 在原始文本之中的左端起始位置
        ''' </summary>
        ''' <returns></returns>
        Public Property Index As Integer

        Sub New(Optional value As String = "")
            Segment = value
        End Sub

        Public Overrides Function ToString() As String
            Return Segment
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Narrowing Operator CType(segment As TextSegment) As String
            Return segment.Segment
        End Operator

        Public Shared Operator Like(segment As TextSegment, text$) As DistResult
            With LevenshteinDistance.ComputeDistance(segment.Array, text)
                .Reference = segment._text
                Return .ByRef
            End With
        End Operator
    End Class
End Namespace

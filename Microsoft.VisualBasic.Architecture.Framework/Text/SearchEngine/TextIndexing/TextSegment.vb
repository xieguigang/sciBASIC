#Region "Microsoft.VisualBasic::fcf5073122d62673c8635937bbfd5b05, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Text\SearchEngine\TextIndexing\TextSegment.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

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
            Get
                Return _text
            End Get
            Set(value As String)
                Dim ascii As Integer() = value.ToArray(Function(c) AscW(c))
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

        Public Overloads Shared Narrowing Operator CType(segment As TextSegment) As String
            Return segment.Segment
        End Operator
    End Class
End Namespace

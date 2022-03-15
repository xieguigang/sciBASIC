#Region "Microsoft.VisualBasic::cb177ee6173b1bf8e0d76e56f54a267a, sciBASIC#\Data\word2vec\utils\Tokenizer.vb"

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

    '   Total Lines: 78
    '    Code Lines: 40
    ' Comment Lines: 26
    '   Blank Lines: 12
    '     File Size: 2.48 KB


    '     Class Tokenizer
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: hasMoreTokens, nextToken, size, (+2 Overloads) ToString
    ' 
    '         Sub: add
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

Namespace utils

    ''' <summary>
    ''' Created by fangy on 13-12-13.
    ''' 分割和输出记号
    ''' </summary>
    Public Class Tokenizer

        '    private String[] tokens;
        '    int currentIndex;
        Private tokens As IList(Of String)
        Private tokenIter As IEnumerator(Of String)

        Public Sub New()
            tokens = New LinkedList(Of String)()
            tokenIter = tokens.GetEnumerator()
        End Sub

        ''' <summary>
        ''' 分割一段文本，得到一列记号 </summary>
        ''' <param name="text"> 文本 </param>
        ''' <param name="delim"> 分割符 </param>
        Public Sub New(text As String, delim As String)
            tokens = text.StringSplit(delim, True).AsList
            tokenIter = tokens.GetEnumerator()
        End Sub

        ''' <summary>
        ''' 获得记号的数量 </summary>
        ''' <returns> 数量 </returns>
        Public Function size() As Integer
            Return tokens.Count
        End Function

        ''' <summary>
        ''' 遍历记号时，查询是否还有记号未遍历 </summary>
        ''' <returns> 若还有记号未遍历，则返回true，否则返回false。 </returns>
        Public Function hasMoreTokens() As Boolean
            Return tokenIter.MoveNext
        End Function

        ''' <summary>
        ''' 遍历记号时获得下一个之前未遍历的记号 </summary>
        ''' <returns> 记号 </returns>
        Public Function nextToken() As String
            Return tokenIter.Current
        End Function

        ''' <summary>
        ''' 向原有记号序列的末尾添加一个记号 </summary>
        ''' <param name="token"> 待添加的记号 </param>
        Public Sub add(token As String)
            If token Is Nothing Then
                Return
            End If

            tokens.Add(token)
        End Sub

        Public Overrides Function ToString() As String
            Return ToString(" ")
        End Function

        ''' <summary>
        ''' 以分割符连接记号并输出 </summary>
        ''' <param name="delim"> 分割符 </param>
        ''' <returns> 记号由分割符连接的字符串 </returns>
        Public Overloads Function ToString(delim As String) As String
            If tokens.Count < 1 Then
                Return ""
            Else
                Return tokens.JoinBy(delim)
            End If
        End Function
    End Class
End Namespace

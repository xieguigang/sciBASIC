#Region "Microsoft.VisualBasic::4e23cd0fd15bffb54ec401d29d33dca3, sciBASIC#\Data\word2vec\utils\Counter.vb"

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

    '   Total Lines: 96
    '    Code Lines: 53
    ' Comment Lines: 27
    '   Blank Lines: 16
    '     File Size: 2.96 KB


    '     Class Counter
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: [get], keySet, size, ToString
    ' 
    '         Sub: (+2 Overloads) add, remove
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace utils

    ''' <summary>
    ''' 计数器
    ''' 最初代码来自Ansj的tree-split包中的love.cq.util;
    ''' @author fangy </summary>
    Public Class Counter(Of tT)

        Dim hm As Dictionary(Of tT, Counter) = Nothing

        Public Sub New()
            hm = New Dictionary(Of tT, Counter)()
        End Sub

        Public Sub New(initialCapacity As Integer)
            hm = New Dictionary(Of tT, Counter)(initialCapacity)
        End Sub

        ''' <summary>
        ''' 增加一个元素，并增加其计数 </summary>
        ''' <param name="t"> 元素 </param>
        ''' <param name="n"> 计数 </param>
        Public Sub add(t As tT, n As Integer)
            If Not hm.ContainsKey(t) Then
                hm.Add(t, New Counter(0))
            End If

            hm(t).Add(n)
        End Sub

        ''' <summary>
        ''' 增加一个元素，计数默认增加1 </summary>
        ''' <param name="t"> 元素 </param>
        Public Sub add(t As tT)
            add(t, 1)
        End Sub

        ''' <summary>
        ''' 获得某个元素的计数 </summary>
        ''' <param name="t"> 待查询的元素 </param>
        ''' <returns> 数目 </returns>
        Public Function [get](t As tT) As Integer
            Dim count = hm.GetValueOrNull(t)

            If count Is Nothing Then
                Return 0
            Else
                Return count.Value()
            End If
        End Function

        ''' <summary>
        ''' 获取哈希表中键的个数 </summary>
        ''' <returns> 键的数量 </returns>
        Public Function size() As Integer
            Return hm.Count
        End Function

        ''' <summary>
        ''' 删除一个元素 </summary>
        ''' <param name="t"> 元素 </param>
        Public Sub remove(t As tT)
            hm.Remove(t)
        End Sub

        ''' <summary>
        ''' 输出已构建好的哈希计数表 </summary>
        ''' <returns> 哈希表 </returns>
        Public Function keySet() As IEnumerable(Of tT)
            Return hm.Keys
        End Function

        ''' <summary>
        ''' 将计数器转换为字符串 </summary>
        ''' <returns> 字符串 </returns>
        Public Overrides Function ToString() As String
            Dim iterator As IEnumerator(Of KeyValuePair(Of tT, Counter)) = SetOfKeyValuePairs(Of tT, Counter)(hm).GetEnumerator()
            Dim sb As StringBuilder = New StringBuilder()
            Dim [next] As KeyValuePair(Of tT, Counter) = Nothing

            While iterator.MoveNext()
                [next] = iterator.Current
                sb.Append([next].Key)
                sb.Append(vbTab)
                sb.Append([next].Value)
                sb.Append(vbLf)
            End While

            Return sb.ToString()
        End Function
    End Class
End Namespace

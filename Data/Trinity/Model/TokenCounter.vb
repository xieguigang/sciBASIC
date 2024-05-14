#Region "Microsoft.VisualBasic::5e305f3889508f6706e44654d0d0dcfe, Data\Trinity\Model\TokenCounter.vb"

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

    '   Total Lines: 84
    '    Code Lines: 43
    ' Comment Lines: 27
    '   Blank Lines: 14
    '     File Size: 2.63 KB


    '     Class TokenCounter
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

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Serialization.JSON
Imports int = Microsoft.VisualBasic.ComponentModel.Counter

Namespace Model

    ''' <summary>
    ''' 计数器
    ''' 最初代码来自Ansj的tree-split包中的love.cq.util;
    ''' @author fangy </summary>
    Public Class TokenCounter(Of tT)

        Dim hm As Dictionary(Of tT, int) = Nothing

        Public Sub New()
            hm = New Dictionary(Of tT, int)()
        End Sub

        Public Sub New(initialCapacity As Integer)
            hm = New Dictionary(Of tT, int)(initialCapacity)
        End Sub

        ''' <summary>
        ''' 增加一个元素，并增加其计数 </summary>
        ''' <param name="t"> 元素 </param>
        ''' <param name="n"> 计数 </param>
        Public Sub add(t As tT, n As Integer)
            If Not hm.ContainsKey(t) Then
                hm.Add(t, New int(0))
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
            Return hm.ToDictionary(Function(w) w.Key, Function(w) CInt(w.Value)).GetJson
        End Function
    End Class
End Namespace

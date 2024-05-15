#Region "Microsoft.VisualBasic::4a1c009f242c865e8db147a045a75fe4, Microsoft.VisualBasic.Core\src\ComponentModel\Algorithm\BinarySearchFunction.vb"

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

    '   Total Lines: 111
    '    Code Lines: 76
    ' Comment Lines: 22
    '   Blank Lines: 13
    '     File Size: 3.68 KB


    '     Class BinarySearchFunction
    ' 
    '         Properties: size
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: BinarySearch
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel.Algorithm

    ''' <summary>
    ''' 精确查找某一个对象
    ''' </summary>
    ''' <typeparam name="K"></typeparam>
    ''' <typeparam name="T"></typeparam>
    Public Class BinarySearchFunction(Of K, T)

        ReadOnly sequence As (index As Integer, key As K, T)()
        ReadOnly order As Comparison(Of K)
        ReadOnly fuzzy As Boolean = False

        Friend ReadOnly rawOrder As T()

        ''' <summary>
        ''' negative index value means read from reverse seqeucne
        ''' </summary>
        ''' <param name="i"></param>
        ''' <returns></returns>
        Default Public ReadOnly Property Item(i As Integer) As T
            Get
                If i < 0 Then
                    Return rawOrder(rawOrder.Length + i)
                Else
                    Return rawOrder(i)
                End If
            End Get
        End Property

        ''' <summary>
        ''' the length of the input sequence data
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property size As Integer
            Get
                Return rawOrder.Length
            End Get
        End Property

        Sub New(source As IEnumerable(Of T),
                key As Func(Of T, K),
                compares As Comparison(Of K),
                Optional allowFuzzy As Boolean = False)

            order = compares
            rawOrder = source.ToArray
            sequence = rawOrder _
                .Select(Function(d, i) (i, key(d), d)) _
                .DoCall(Function(data)
                            Return New QuickSortFunction(Of K, (index As Integer, K, T))(compares).QuickSort(
                                list:=data,
                                key:=Function(i) i.Item2
                            )
                        End Function)
            fuzzy = allowFuzzy
        End Sub

        ''' <summary>
        ''' should be reorder in asc
        ''' </summary>
        ''' <param name="target"></param>
        ''' <returns>Returns ``-1`` means no search result</returns>
        Public Function BinarySearch(target As K) As Integer
            Dim x As (index As Integer, K, T)
            Dim min% = 0
            Dim max% = sequence.Length - 1
            Dim index%
            Dim key As K

            If max = -1 Then
                ' no elements
                Return -1
            ElseIf max = 0 Then
                ' one element
                If 0 = order(sequence(0).Item2, target) Then
                    Return 0
                Else
                    ' 序列只有一个元素，但是不相等，则返回-1，否则后面的while会无限死循环
                    Return -1
                End If
            End If

            Do While max <> (min + 1)
                index = (max - min) / 2 + min
                x = sequence(index)
                key = x.Item2

                If 0 = order(target, key) Then
                    Return x.index
                ElseIf order(target, key) > 0 Then
                    min = index
                Else
                    max = index
                End If
            Loop

            If 0 = order(sequence(min).key, target) Then
                Return sequence(min).index
            ElseIf 0 = order(sequence(max).key, target) Then
                Return sequence(max).index
            ElseIf fuzzy Then
                Return x.index
            Else
                Return -1
            End If
        End Function
    End Class
End Namespace

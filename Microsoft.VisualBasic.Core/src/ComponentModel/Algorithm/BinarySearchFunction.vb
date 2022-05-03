#Region "Microsoft.VisualBasic::ccd8c5edeb41241f44c48cb110f3def2, sciBASIC#\Microsoft.VisualBasic.Core\src\ComponentModel\Algorithm\BinarySearchFunction.vb"

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

'   Total Lines: 87
'    Code Lines: 68
' Comment Lines: 8
'   Blank Lines: 11
'     File Size: 2.89 KB


'     Class BinarySearchFunction
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: BinarySearch
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel.Algorithm

    Public Class BinarySearchFunction(Of K, T)

        ReadOnly sequence As (index As Integer, key As K, T)()
        ReadOnly order As Comparison(Of K)
        ReadOnly rawOrder As T()

        Default Public ReadOnly Property Item(i As Integer) As T
            Get
                If i = -1 Then
                    Return Nothing
                Else
                    Return rawOrder(i)
                End If
            End Get
        End Property

        Sub New(source As IEnumerable(Of T), key As Func(Of T, K), compares As Comparison(Of K))
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
            Else
                Return -1
            End If
        End Function
    End Class
End Namespace

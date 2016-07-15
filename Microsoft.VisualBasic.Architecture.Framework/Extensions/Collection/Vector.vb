#Region "Microsoft.VisualBasic::e867052510be20c32dbd681ce33c17eb, ..\Microsoft.VisualBasic.Architecture.Framework\Extensions\Collection\Vector.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq.Extensions

Public Module VectorExtensions

    <Extension>
    Public Sub Memset(Of T)(ByRef array As T(), o As T, len As Integer)
        If array Is Nothing OrElse array.Length < len Then
            array = New T(len - 1) {}
        End If

        For i As Integer = 0 To len - 1
            array(i) = o
        Next
    End Sub

    <Extension>
    Public Sub Memset(ByRef s As String, c As Char, len As Integer)
        s = New String(c, len)
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="start">0 base</param>
    ''' <param name="length"></param>
    ''' <returns></returns>
    <Extension> Public Function Midv(Of T)(source As IEnumerable(Of T), start As Integer, length As Integer) As T()
        If source.IsNullOrEmpty Then
            Return New T() {}
        ElseIf source.Count < length Then
            Return source.ToArray
        End If

        Dim array As T() = source.ToArray
        Dim ends As Integer = start + length

        If ends > array.Length Then
            length -= array.Length - ends
        End If

        Dim buf As T() = New T(length - 1) {}
        Call System.Array.ConstrainedCopy(array, start, buf, Scan0, buf.Length)
        Return buf
    End Function

    <Extension> Public Function LoadDblArray(path As String) As Double()
        Dim array As String() = IO.File.ReadAllLines(path)
        Dim n As Double() = array.ToArray(Function(x) Val(x))
        Return n
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="delimiter">和字符串的Split函数一样，这里作为delimiter的元素都不会出现在结果之中</param>
    ''' <returns></returns>
    <Extension> Public Function Split(Of T)(source As IEnumerable(Of T), delimiter As Func(Of T, Boolean)) As T()()
        Dim array As T() = source.ToArray
        Dim list As New List(Of T())
        Dim tmp As New List(Of T)

        For i As Integer = 0 To array.Length - 1
            Dim x As T = array(i)
            If delimiter(x) = True Then
                Call list.Add(tmp.ToArray)
                Call tmp.Clear()
            Else
                Call tmp.Add(x)
            End If
        Next

        Return list.ToArray
    End Function

    ''' <summary>
    ''' 查找出列表之中符合条件的所有的索引编号
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="array"></param>
    ''' <param name="condi"></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function GetIndexes(Of T)(array As T(), condi As Func(Of T, Boolean)) As IEnumerable(Of Integer)
        For i As Integer = 0 To array.Length - 1
            If condi(array(i)) Then
                Yield i
            End If
        Next
    End Function
End Module

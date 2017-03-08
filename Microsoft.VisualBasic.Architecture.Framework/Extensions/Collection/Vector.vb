#Region "Microsoft.VisualBasic::f7e2f29bf69a097626c970532f88c956, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Collection\Vector.vb"

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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Linq.Extensions

Public Module VectorExtensions

    Public Function LengthEquals(Of T)(n%, any As Boolean, ParamArray array As IEnumerable(Of T)()) As Boolean
        Dim c%() = array.Select(Function(s) s.Count).ToArray
        Dim equals = c.Where(Function(x) x = n).ToArray

        If any Then
            Return equals.Length > 0
        Else
            Return equals.Length = array.Length
        End If
    End Function

    ''' <summary>
    ''' 用来生成map数据的，
    ''' + 当两个向量长度相同，会不进行任何处理，即两个向量之间，元素都可以一一对应，
    ''' + 但是当某一个向量的长度为1的时候，就会将该向量补齐，因为此时会是一对多的关系
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function PairData(Of T)(a As T(), b As T()) As IEnumerable(Of Map(Of T, T))
        If a.Length = 1 AndAlso b.Length > 1 Then
            ' 补齐a
            a = a(0).CopyVector(b.Length)
        ElseIf a.Length > 1 AndAlso b.Length = 1 Then
            ' 补齐b
            b = b(0).CopyVector(a.Length)
        ElseIf a.Length <> b.Length Then
            ' 无法计算
            Throw New Exception("Both a and b their length should be equals or one of them should be length=1!")
        End If

        For i As Integer = 0 To a.Length - 1
            Yield New Map(Of T, T) With {
                .Key = a(i),
                .Maps = b(i)
            }
        Next
    End Function

    ''' <summary>
    ''' 在一个一维数组中搜索指定对象，并返回其首个匹配项的索引。
    ''' </summary>
    ''' <typeparam name="T">数组元素的类型。</typeparam>
    ''' <param name="array">要搜索的从零开始的一维数组。</param>
    ''' <param name="o">要在 array 中查找的对象。</param>
    ''' <returns>如果在整个 array 中找到 value 的第一个匹配项，则为该项的从零开始的索引；否则为 -1。</returns>
    <Extension>
    Public Function IndexOf(Of T)(array As T(), o As T) As Integer
        Return System.Array.IndexOf(array, value:=o)
    End Function

    <Extension>
    Public Function GetMinIndex(values As List(Of Double)) As Integer
        Dim min As Double = Double.MaxValue
        Dim minIndex As Integer = 0
        For i As Integer = 0 To values.Count - 1
            If values(i) < min Then
                min = values(i)
                minIndex = i
            End If
        Next

        Return minIndex
    End Function

    <Extension>
    Public Function GetMaxIndex(values As List(Of Double)) As Integer
        Dim max As Double = Double.MinValue
        Dim maxIndex As Integer = 0
        For i As Integer = 0 To values.Count - 1
            If values(i) > max Then
                max = values(i)
                maxIndex = i
            End If
        Next

        Return maxIndex
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="range"></param>
    ''' <param name="sites"></param>
    ''' <returns></returns>
    ''' 
    <Extension>
    Public Function InsideAny(range As IntRange, sites As IEnumerable(Of Integer)) As Boolean
        For Each x% In sites
            If range.IsInside(x) Then
                Return True
            End If
        Next

        Return False
    End Function

    ''' <summary>
    ''' 从后往前访问集合之中的元素，请注意请不要使用Linq查询表达式，尽量使用``list``或者``array``
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source">请不要使用Linq查询表达式，尽量使用``list``或者``array``</param>
    ''' <param name="index"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Last(Of T)(source As IEnumerable(Of T), index As Integer) As T
        Return source(source.Count - index)
    End Function

    ''' <summary>
    ''' 取出在x元素之后的所有元素
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="x"></param>
    ''' <returns></returns>
    <Extension>
    Public Function After(Of T)(source As IEnumerable(Of T), x As T) As IEnumerable(Of T)
        Return source.After(Function(o) x.Equals(o))
    End Function

    ''' <summary>
    ''' 取出在判定条件成立的元素之后的所有元素
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="predicate"></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function After(Of T)(source As IEnumerable(Of T), predicate As Predicate(Of T)) As IEnumerable(Of T)
        Dim isAfter As Boolean = False

        For Each x As T In source
            If isAfter Then
                Yield x
            Else
                If predicate(x) Then
                    isAfter = True
                End If
            End If
        Next
    End Function

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

    ''' <summary>
    ''' Each line in the text file should be a <see cref="Double"/> type numeric value.
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns></returns>
    <Extension> Public Function LoadDblArray(path As String) As Double()
        Dim array As String() = IO.File.ReadAllLines(path)
        Dim n As Double() = array.ToArray(AddressOf Val)
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

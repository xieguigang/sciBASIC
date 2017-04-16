#Region "Microsoft.VisualBasic::0628306624cd153dac563a9eb7633507, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Collection\Matrix.vb"

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
Imports Microsoft.VisualBasic.Language

Public Module MatrixExtensions

    ''' <summary>
    ''' 生成一个有m行n列的矩阵，但是是使用数组来表示的
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="m"></param>
    ''' <param name="n"></param>
    ''' <returns></returns>
    Public Function MAT(Of T)(m As Integer, n As Integer) As T()()
        Dim newMAT As T()() = New T(m - 1)() {}

        For i As Integer = 0 To m - 1
            newMAT(i) = New T(n - 1) {}
        Next

        Return newMAT
    End Function

    ''' <summary>
    ''' Convert the data collection into a matrix value.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source">The elements number in each collection should be agreed!(要求集合之中的每一列之中的数据的元素数目都相等)</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function ToMatrix(Of T)(source As IEnumerable(Of IEnumerable(Of T))) As T(,)
        Dim width As Integer = source.First.Count
        Dim array As IEnumerable(Of T)() = source.ToArray
        Dim height As Integer = array.Length
        Dim MAT As T(,) = New T(height - 1, width - 1) {}

        For i As Integer = 0 To height - 1
            Dim row As T() = array(i).ToArray

            For j As Integer = 0 To width - 1
                MAT(i, j) = row(j)
            Next
        Next

        Return MAT
    End Function

    ''' <summary>
    ''' Convert the matrix data into a collection of collection data type.(将矩阵对象转换为集合的集合的类型)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="MAT"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function ToVectorList(Of T)(MAT As T(,)) As List(Of T())
        Return MAT.RowIterator.AsList
    End Function

    ''' <summary>
    ''' Iterates each row data in a matrix type array.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="MAT"></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function RowIterator(Of T)(MAT As T(,)) As IEnumerable(Of T())
        Dim width As Integer = MAT.GetLength(1)
        Dim height As Integer = MAT.GetLength(0)
        Dim Temp As T()

        For i As Integer = 0 To height - 1
            Temp = New T(width - 1) {}

            For j As Integer = 0 To width - 1
                Temp(j) = MAT(i, j)
            Next

            Yield Temp
        Next
    End Function
End Module

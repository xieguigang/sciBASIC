#Region "Microsoft.VisualBasic::89b92c1ad139283aee322604f715bad3, Microsoft.VisualBasic.Core\Extensions\Collection\Matrix.vb"

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

    ' Module MatrixExtensions
    ' 
    '     Function: DATA, MAT, (+2 Overloads) Matrix, RowIterator, ToFloatMatrix
    '               ToMatrix, ToVectorList
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Public Module MatrixExtensions

    ''' <summary>
    ''' 将数据集之中的虽有属性值取出来构建一个矩阵
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    Public Iterator Function Matrix(Of T, DataSet As DynamicPropertyBase(Of T))(data As IEnumerable(Of DataSet)) As IEnumerable(Of T())
        With data.ToArray
            Dim allFields = .Select(Function(x) x.Properties.Keys) _
                            .IteratesALL _
                            .Distinct _
                            .ToArray

            For Each x As DataSet In .ByRef
                ' 利用属性名列表做subset，得到每一个数据对象的属性向量
                Yield x.ItemValue(allFields)
            Next
        End With
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Matrix(Of DataSet As DynamicPropertyBase(Of Double))(data As IEnumerable(Of DataSet)) As IEnumerable(Of Double())
        Return Matrix(Of Double, DataSet)(data)
    End Function

    '<MethodImpl(MethodImplOptions.AggressiveInlining)>
    '<Extension>
    'Public Function Matrix(Of DataSet As DynamicPropertyBase(Of String))(data As IEnumerable(Of DataSet)) As IEnumerable(Of String())
    '    Return Matrix(Of String, DataSet)(data)
    'End Function

    ''' <summary>
    ''' Converts a <see cref="DataTable"/> to a 2-dimensional array
    ''' </summary>
    ''' <param name="table">A System.Data.DataTable containing data to cluster</param>
    ''' <returns>A 2-dimensional array containing data to cluster</returns>
    <Extension> Public Function ToFloatMatrix(table As DataTable) As Double(,)
        Dim rowCount As Integer = table.Rows.Count
        Dim fieldCount As Integer = table.Columns.Count
        Dim dataPoints As Double(,)
        Dim fieldValue As Double = 0.0
        Dim row As DataRow

        dataPoints = New Double(rowCount - 1, fieldCount - 1) {}

        For rowPosition As Integer = 0 To rowCount - 1
            row = table.Rows(rowPosition)

            For fieldPosition As Integer = 0 To fieldCount - 1
                Try
                    fieldValue = Double.Parse(row(fieldPosition).ToString())
                Catch ex As Exception
                    Dim msg$ = $"Invalid row at {rowPosition.ToString()} and field {fieldPosition.ToString()}"
                    ex = New InvalidCastException(msg, ex)
                    ex.PrintException

                    Throw ex
                End Try

                dataPoints(rowPosition, fieldPosition) = fieldValue
            Next
        Next

        Return dataPoints
    End Function

    ''' <summary>
    ''' Gets property field data from a generic data frame
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="dataframe"></param>
    ''' <param name="property$"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function DATA(Of T)(dataframe As IEnumerable(Of DynamicPropertyBase(Of T)), property$) As T()
        Return dataframe.Select(Function(d) d([property])).ToArray
    End Function

    ''' <summary>
    ''' 生成一个有m行n列的矩阵，但是是使用数组来表示的
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="m"></param>
    ''' <param name="n"></param>
    ''' <returns></returns>
    Public Function MAT(Of T)(m%, n%) As T()()
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
        Dim array As IEnumerable(Of T)() = source.ToArray
        Dim width As Integer = array(Scan0).Count
        Dim height As Integer = array.Length
        Dim M As T(,) = New T(height - 1, width - 1) {}

        For i As Integer = 0 To height - 1
            Dim V As T() = array(i).ToArray

            For j As Integer = 0 To width - 1
                M(i, j) = V(j)
            Next
        Next

        Return M
    End Function

    ''' <summary>
    ''' Convert the matrix data into a collection of collection data type.(将矩阵对象转换为集合的集合的类型)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="MAT"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
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

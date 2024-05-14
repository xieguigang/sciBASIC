#Region "Microsoft.VisualBasic::58f4ab51943fb0027d75627444edaecb, Microsoft.VisualBasic.Core\src\Extensions\Collection\Matrix.vb"

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

    '   Total Lines: 252
    '    Code Lines: 148
    ' Comment Lines: 64
    '   Blank Lines: 40
    '     File Size: 9.10 KB


    '     Module MatrixExtensions
    ' 
    '         Function: DATA, DimensionSizeOf, GetCol, GetRow, GetSize
    '                   (+2 Overloads) Matrix, Rectangle, RowIterator, SetCol, SetRow
    '                   ToFloatMatrix, ToMatrix, ToVectorList
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel.Collection

    <HideModuleName>
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
        <Extension>
        Public Function ToFloatMatrix(table As DataTable) As Double(,)
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
        ''' An extension method shortcut to the function <see cref="RectangularArray.Matrix"/>.
        ''' </summary>
        ''' <param name="type"></param>
        ''' <param name="m%"></param>
        ''' <param name="n%"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Rectangle(type As Type, m%, n%) As Array
            Return RectangularArray.Matrix(type, m, n)
        End Function

        ''' <summary>
        ''' Measure the dimension size of the rectangle array.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="rect"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function DimensionSizeOf(Of T)(rect As T()()) As Size
            Return New Size With {
                .Width = rect(Scan0).Length,
                .Height = rect.Length
            }
        End Function

        ''' <summary>
        ''' Convert the data collection into a matrix value.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source">The elements number in each collection should be agreed!(要求集合之中的每一列之中的数据的元素数目都相等)</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Extension>
        Public Function ToMatrix(Of T)(source As IEnumerable(Of IEnumerable(Of T))) As T(,)
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

#Region "Matrix Accessor Helper"

        ''' <summary>
        ''' Get matrix width and height values.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="m"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetSize(Of T)(m As T(,)) As Size
            Dim w As Integer = m.GetLength(1)
            Dim h As Integer = m.GetLength(0)

            Return New Size With {
                .Width = w,
                .Height = h
            }
        End Function

        ''' <summary>
        ''' Iterates each row data in a matrix type array.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="MAT"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function RowIterator(Of T)(MAT As T(,)) As IEnumerable(Of T())
            Dim size As Size = MAT.GetSize
            Dim temp As T()

            For i As Integer = 0 To size.Height - 1
                temp = New T(size.Width - 1) {}

                For j As Integer = 0 To size.Width - 1
                    temp(j) = MAT(i, j)
                Next

                Yield temp
            Next
        End Function

        <Extension>
        Public Iterator Function GetRow(Of T)(m As T(,), i%) As IEnumerable(Of T)
            Dim size As Size = m.GetSize

            For j As Integer = 0 To size.Width - 1
                Yield m(i, j)
            Next
        End Function

        <Extension>
        Public Function SetRow(Of T)(m As T(,), i%, data As IEnumerable(Of T)) As T(,)
            Dim size As Size = m.GetSize
            Dim j% = Scan0

            For Each x As T In data
                m(i, j) = x
                j += 1

                If j = size.Width Then
                    Exit For
                End If
            Next

            Return m
        End Function

        <Extension>
        Public Iterator Function GetCol(Of T)(m As T(,), j%) As IEnumerable(Of T)
            Dim size As Size = m.GetSize

            For i As Integer = 0 To size.Height - 1
                Yield m(i, j)
            Next
        End Function

        <Extension>
        Public Function SetCol(Of T)(m As T(,), j%, data As IEnumerable(Of T)) As T(,)
            Dim size As Size = m.GetSize
            Dim i% = Scan0

            For Each x As T In data
                m(i, j) = x
                i += 1

                If i = size.Height Then
                    Exit For
                End If
            Next

            Return m
        End Function
#End Region
    End Module
End Namespace

﻿#Region "Microsoft.VisualBasic::7cadea009ff9045e8b1cbd414488c1b4, Data_science\Mathematica\Math\DataFrame\Correlation\Builder.vb"

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

    '   Total Lines: 130
    '    Code Lines: 87 (66.92%)
    ' Comment Lines: 31 (23.85%)
    '    - Xml Docs: 96.77%
    ' 
    '   Blank Lines: 12 (9.23%)
    '     File Size: 5.53 KB


    ' Module Builder
    ' 
    '     Function: Correlation, (+4 Overloads) MatrixBuilder
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Runtime.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Statistics.Hypothesis
Imports Microsoft.VisualBasic.Math.Statistics.Hypothesis.Mantel

Public Module Builder

    ''' <summary>
    ''' 一个通用的距离矩阵创建函数
    ''' </summary>
    ''' <typeparam name="DataSet"></typeparam>
    ''' <param name="data"></param>
    ''' <param name="eval"></param>
    ''' <param name="type"></param>
    ''' <returns></returns>
    <Extension>
    Public Function MatrixBuilder(Of DataSet As {INamedValue, DynamicPropertyBase(Of Double)})(data As IEnumerable(Of DataSet), eval As Func(Of Double(), Double(), Double), type As DataType) As DistanceMatrix
        Return data.MatrixBuilder(Function(x, y) (eval(x, y), 0), type)
    End Function

    ''' <summary>
    ''' Create a new pearson correlation matrix
    ''' </summary>
    ''' <typeparam name="DataSet"></typeparam>
    ''' <param name="data"></param>
    ''' <param name="eval"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Correlation(Of DataSet As INamedValue)(data As Enumeration(Of DataSet), eval As Func(Of DataSet, Double())) As CorrelationMatrix
        Dim allData = data.AsEnumerable.ToArray
        ' extract the matrix data
        Dim mat As Double()() = allData.Select(eval).ToArray
        Dim corr = mat.GetCorrelations(AddressOf Mantel.Pearson)
        Dim keys As Index(Of String) = allData.Keys.Indexing

        Return New CorrelationMatrix(keys, corr.cor, corr.pvalue)
    End Function

    ''' <summary>
    ''' 一个通用的距离矩阵创建函数
    ''' </summary>
    ''' <typeparam name="DataSet"></typeparam>
    ''' <param name="data"></param>
    ''' <param name="eval"></param>
    ''' <param name="type"></param>
    ''' <returns></returns>
    <Extension>
    Public Function MatrixBuilder(Of DataSet As {INamedValue, DynamicPropertyBase(Of Double)})(data As IEnumerable(Of DataSet), eval As Func(Of Double(), Double(), (Double, Double)), type As DataType) As DataMatrix
        Dim allData As DataSet() = data.ToArray
        Dim names As String() = allData.PropertyNames
        Dim vector As Func(Of DataSet, Double()) =
            Function(d)
                Return d.Properties.Takes(names).ToArray
            End Function

        Return allData.MatrixBuilder(vector, eval, type)
    End Function

    ''' <summary>
    ''' 一个通用的距离矩阵创建函数
    ''' </summary>
    ''' <typeparam name="DataSet"></typeparam>
    ''' <param name="eval">evaluate the (correlation,pvalue)</param>
    ''' <param name="type"></param>
    ''' <returns></returns>
    <Extension>
    Public Function MatrixBuilder(Of DataSet As INamedValue)(allData As DataSet(),
                                                             vector As Func(Of DataSet, Double()),
                                                             eval As Func(Of Double(), Double(), (Double, Double)),
                                                             type As DataType) As DataMatrix
        Dim keys As String() = allData.Keys
        Dim evalData = allData _
            .SeqIterator _
            .AsParallel _
            .Select(Function(d)
                        Dim vec As New List(Of Double)
                        Dim vec2 As New List(Of Double)
                        Dim x As Double() = vector(d)
                        Dim y As Double()
                        Dim export As (cor As Double, pvalue As Double)

                        For Each row As DataSet In allData
                            y = vector(row)
                            export = eval(x, y)
                            vec += export.cor
                            vec2 += export.pvalue
                        Next

                        Return (d.i, cor:=vec.ToArray, pval:=vec2.ToArray)
                    End Function) _
            .OrderBy(Function(d) d.i) _
            .ToArray
        Dim matrix As Double()() = evalData _
            .Select(Function(d) d.cor) _
            .ToArray
        Dim nameIndex = keys.Indexing

        If keys.Length <> nameIndex.Count Then
            Throw New InvalidDataContractException("there are some duplicated names of your data matrix, please makes the label of each data object be unique!")
        End If

        If type = DataType.Correlation Then
            Return New CorrelationMatrix(
                names:=nameIndex,
                matrix:=matrix,
                pvalue:=evalData.Select(Function(d) d.pval).ToArray
            )
        Else
            Return New DistanceMatrix(
                names:=nameIndex,
                matrix:=matrix,
                isDistance:=(type = DataType.Distance)
            )
        End If
    End Function

    <Extension>
    Public Function MatrixBuilder(df As DataFrame, eval As Func(Of Double(), Double(), (Double, Double)), type As DataType) As DataMatrix
        Return df _
            .NumericMatrix _
            .ToArray _
            .MatrixBuilder(vector:=Function(i) i.value, eval, type)
    End Function
End Module

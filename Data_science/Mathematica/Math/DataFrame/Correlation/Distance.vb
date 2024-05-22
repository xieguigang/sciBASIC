#Region "Microsoft.VisualBasic::c90b5fdb9f93e2d678a9ab25cc2b6777, Data_science\Mathematica\Math\DataFrame\Correlation\Distance.vb"

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

    '   Total Lines: 49
    '    Code Lines: 30 (61.22%)
    ' Comment Lines: 13 (26.53%)
    '    - Xml Docs: 92.31%
    ' 
    '   Blank Lines: 6 (12.24%)
    '     File Size: 2.02 KB


    ' Module Distance
    ' 
    '     Function: Correlation, Euclidean, Similarity
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Statistics.Hypothesis

Public Module Distance

    ''' <summary>
    ''' 使用欧式距离构建出一个距离矩阵
    ''' </summary>
    ''' <typeparam name="DataSet"></typeparam>
    ''' <param name="data"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Euclidean(Of DataSet As {INamedValue, DynamicPropertyBase(Of Double)})(data As IEnumerable(Of DataSet)) As DistanceMatrix
        Return data.MatrixBuilder(AddressOf EuclideanDistance, type:=DataType.Distance)
    End Function

    <Extension>
    Public Function Correlation(Of DataSet As {INamedValue, DynamicPropertyBase(Of Double)})(data As IEnumerable(Of DataSet), Optional spearman As Boolean = False) As CorrelationMatrix
        Dim cor As Func(Of Double(), Double(), (Double, Double))

        If spearman Then
            cor = Function(x, y)
                      Return (Correlations.Spearman(x, y), 0)
                  End Function
        Else
            cor = AddressOf Mantel.Pearson
        End If

        Return data.MatrixBuilder(cor, type:=DataType.Correlation)
    End Function

    ''' <summary>
    ''' cos similarity
    ''' </summary>
    ''' <typeparam name="DataSet"></typeparam>
    ''' <param name="data"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Similarity(Of DataSet As {INamedValue, DynamicPropertyBase(Of Double)})(data As IEnumerable(Of DataSet)) As DistanceMatrix
        Return data.MatrixBuilder(Function(x, y) SSM(New Vector(x), New Vector(y)), type:=DataType.Similarity)
    End Function
End Module

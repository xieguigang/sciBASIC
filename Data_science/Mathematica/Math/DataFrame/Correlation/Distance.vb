#Region "Microsoft.VisualBasic::b15a79483ded86b9ea182c2261fd4e5b, Data_science\Mathematica\Math\DataFrame\Correlation\Distance.vb"

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

    ' Module Distance
    ' 
    '     Function: Correlation, Euclidean
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Public Module Distance

    ''' <summary>
    ''' 使用欧式距离构建出一个距离矩阵
    ''' </summary>
    ''' <typeparam name="DataSet"></typeparam>
    ''' <param name="data"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Euclidean(Of DataSet As {INamedValue, DynamicPropertyBase(Of Double)})(data As IEnumerable(Of DataSet)) As DistanceMatrix
        Return data.MatrixBuilder(AddressOf EuclideanDistance, True)
    End Function

    <Extension>
    Public Function Correlation(Of DataSet As {INamedValue, DynamicPropertyBase(Of Double)})(data As IEnumerable(Of DataSet), Optional spearman As Boolean = False) As DistanceMatrix
        Dim cor As Func(Of Double(), Double(), Double)

        If spearman Then
            cor = AddressOf Correlations.Spearman
        Else
            cor = AddressOf Correlations.GetPearson
        End If

        Return data.MatrixBuilder(cor, isDistance:=False)
    End Function
End Module

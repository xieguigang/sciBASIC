#Region "Microsoft.VisualBasic::NumericTableExtensions, Data_science\DataMining\PaCMAP\NumericTableExtensions.vb"

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
    ' along with this program.  If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports System.Data
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data
Imports PaCMAPEstimator = Microsoft.VisualBasic.DataMining.PaCMAP.PaCMAP

''' <summary>
''' PaCMAP 降维算法的统一二维表入口。
''' </summary>
Public Module NumericTableExtensions

    ''' <summary>
    ''' 使用 PaCMAP 对统一二维表<see cref="NumericTable"/>之中的样本特征进行降维，
    ''' 并返回以嵌入坐标为特征的新二维表。
    ''' 
    ''' ```vb
    ''' Dim y = x.pacmap(dims:=2)
    ''' Dim z = y.kmeans(k:=3)
    ''' ```
    ''' </summary>
    ''' <param name="source">经过预处理之后的纯数值二维表</param>
    ''' <param name="dims">投影之后的维度数量（至少为 2）</param>
    ''' <param name="neighbors">近邻对的数量</param>
    ''' <param name="iterations">优化迭代次数</param>
    ''' <param name="init">嵌入坐标的初始化方式</param>
    ''' <returns>以嵌入坐标（``dim_1..dim_n``）为特征的新二维表</returns>
    <Extension>
    Public Function pacmap(source As NumericTable,
                           Optional dims As Integer = 2,
                           Optional neighbors As Integer = 10,
                           Optional iterations As Integer = 450,
                           Optional init As String = "random") As NumericTable

        If source Is Nothing Then
            Throw New ArgumentNullException(NameOf(source))
        End If
        If source.nfeatures = 0 Then
            Throw New InvalidConstraintException("the source table has no feature column for PaCMAP embedding!")
        End If

        Dim rows As Double()() = source.NumericRows()
        Dim n As Integer = rows.Length
        Dim width As Integer = source.nfeatures
        Dim input As Double(,) = New Double(n - 1, width - 1) {}

        For i As Integer = 0 To n - 1
            For j As Integer = 0 To width - 1
                input(i, j) = rows(i)(j)
            Next
        Next

        ' 库调用时保持静默，避免向宿主程序的控制台输出迭代日志
        Dim estimator As New PaCMAPEstimator(
            nDimensions:=dims,
            numNeighbourPairs:=neighbors,
            numIterations:=iterations
        ) With {
            .Silent = True
        }
        Dim result As Double(,) = estimator.Fit(input, init)
        Dim embedding As Double()() = New Double(n - 1)() {}

        For i As Integer = 0 To n - 1
            Dim vector As Double() = New Double(dims - 1) {}

            For j As Integer = 0 To dims - 1
                vector(j) = result(i, j)
            Next

            embedding(i) = vector
        Next

        Return source.ToEmbeddingTable(embedding)
    End Function
End Module

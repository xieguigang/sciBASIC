#Region "Microsoft.VisualBasic::NumericTableExtensions, Data_science\DataMining\UMAP\NumericTableExtensions.vb"

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
Imports UmapEstimator = Microsoft.VisualBasic.DataMining.UMAP.Umap

''' <summary>
''' UMAP 降维算法的统一二维表入口。
''' </summary>
Public Module NumericTableExtensions

    ''' <summary>
    ''' 使用 UMAP 对统一二维表<see cref="NumericTable"/>之中的样本特征进行降维，
    ''' 并返回以嵌入坐标为特征的新二维表。
    ''' 
    ''' ```vb
    ''' Dim y = x.umap(dims:=2)
    ''' Dim z = y.kmeans(k:=3)
    ''' ```
    ''' </summary>
    ''' <param name="source">经过预处理之后的纯数值二维表</param>
    ''' <param name="dims">嵌入的维度数量</param>
    ''' <param name="neighbors">最近邻的数量</param>
    ''' <param name="epochs">自定义的 SGD 迭代次数，为 Nothing 时使用默认的 epochs 启发式</param>
    ''' <param name="minDist">嵌入点之间的最小距离</param>
    ''' <param name="spread">嵌入点的有效尺度</param>
    ''' <returns>以嵌入坐标（``dim_1..dim_n``）为特征的新二维表</returns>
    <Extension>
    Public Function umap(source As NumericTable,
                         Optional dims As Integer = 2,
                         Optional neighbors As Integer = 15,
                         Optional epochs As Integer? = Nothing,
                         Optional minDist As Double = 0.1,
                         Optional spread As Double = 1) As NumericTable

        If source Is Nothing Then
            Throw New ArgumentNullException(NameOf(source))
        End If
        If source.nsamples = 0 OrElse source.nfeatures = 0 Then
            Throw New InvalidConstraintException("the source table has no sample/feature data for UMAP embedding!")
        End If

        Dim estimator As New UmapEstimator(
            dimensions:=dims,
            numberOfNeighbors:=neighbors,
            customNumberOfEpochs:=epochs,
            minDist:=minDist,
            spread:=spread
        )

        ' UMAP 的 GetNEpochs() 为私有成员，这里使用 InitializeFit 的返回值作为迭代步数
        Dim nEpochs As Integer = estimator.InitializeFit(source.NumericRows())

        Call estimator.Step(nEpochs, tqdm_wrap:=False)

        Return source.ToEmbeddingTable(estimator.GetEmbedding())
    End Function
End Module

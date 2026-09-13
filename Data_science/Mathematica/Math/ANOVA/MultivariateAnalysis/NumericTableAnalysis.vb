#Region "Microsoft.VisualBasic::NumericTableAnalysis, Data_science\Mathematica\Math\ANOVA\MultivariateAnalysis\NumericTableAnalysis.vb"

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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data

''' <summary>
''' 多变量分析（PCA / PLS-DA / OPLS-DA）的统一二维表入口。
''' 
''' 所有的入口方法都直接接收 <see cref="NumericTable"/>，内部会构造
''' 一个**全新的** <see cref="StatisticsObject"/> 之后再调用现有的分析算法，
''' 因此可以安全地进行链式调用。
''' </summary>
Public Module NumericTableAnalysis

    ''' <summary>
    ''' 主成分分析（PCA）。
    ''' 
    ''' X 为表的全部特征列；PCA 不需要响应变量，因此 <paramref name="y"/> 可以留空。
    ''' </summary>
    ''' <param name="table">经过预处理之后的纯数值二维表</param>
    ''' <param name="maxPC">需要计算的主成分的最大数量</param>
    ''' <param name="cutoff">主成分迭代计算的收敛阈值</param>
    ''' <param name="y">响应/分组变量所在的标签列名，PCA 不需要</param>
    ''' <param name="scale">X 矩阵的缩放方法</param>
    ''' <param name="transform">X 矩阵的变换方法</param>
    ''' <returns></returns>
    <Extension>
    Public Function pca(table As NumericTable,
                        Optional maxPC As Integer = 5,
                        Optional cutoff As Double = 0.0000001,
                        Optional y As String = Nothing,
                        Optional scale As ScaleMethod = ScaleMethod.AutoScale,
                        Optional transform As TransformMethod = TransformMethod.None) As MultivariateAnalysisResult

        Dim ds As StatisticsObject = table.AsStatisticsObject(y, scale, transform)

        Return ds.PrincipalComponentAnalysis(maxPC, cutoff)
    End Function

    ''' <summary>
    ''' 偏最小二乘判别分析（PLS-DA）。
    ''' 
    ''' X 为表的全部特征列，y 为由参数 <paramref name="y"/> 指定的标签列
    ''' （缺省为表中的第一个标签列）。响应变量是必须的。
    ''' </summary>
    ''' <param name="table">经过预处理之后的纯数值二维表</param>
    ''' <param name="component">
    ''' 指定的潜变量数量，小于 1 的时候由内部的交叉验证自动确定最优的因子数量
    ''' </param>
    ''' <param name="y">响应/分组变量所在的标签列名，缺省为第一个标签列</param>
    ''' <param name="scale">X 矩阵的缩放方法</param>
    ''' <param name="transform">X 矩阵的变换方法</param>
    ''' <returns></returns>
    <Extension>
    Public Function plsda(table As NumericTable,
                          Optional component As Integer = -1,
                          Optional y As String = Nothing,
                          Optional scale As ScaleMethod = ScaleMethod.AutoScale,
                          Optional transform As TransformMethod = TransformMethod.None) As MultivariateAnalysisResult

        Dim ds As StatisticsObject = table.AsStatisticsObject(y, scale, transform)

        Call RequireResponse(ds, NameOf(plsda))

        Return PLS.PartialLeastSquares(ds, component)
    End Function

    ''' <summary>
    ''' 正交偏最小二乘判别分析（OPLS-DA）。
    ''' 
    ''' X 为表的全部特征列，y 为由参数 <paramref name="y"/> 指定的标签列
    ''' （缺省为表中的第一个标签列）。响应变量是必须的。
    ''' </summary>
    ''' <param name="table">经过预处理之后的纯数值二维表</param>
    ''' <param name="component">
    ''' 指定的正交成分数量，小于 1 的时候由内部的交叉验证自动确定
    ''' </param>
    ''' <param name="y">响应/分组变量所在的标签列名，缺省为第一个标签列</param>
    ''' <param name="scale">X 矩阵的缩放方法</param>
    ''' <param name="transform">X 矩阵的变换方法</param>
    ''' <returns></returns>
    <Extension>
    Public Function oplsda(table As NumericTable,
                           Optional component As Integer = -1,
                           Optional y As String = Nothing,
                           Optional scale As ScaleMethod = ScaleMethod.AutoScale,
                           Optional transform As TransformMethod = TransformMethod.None) As MultivariateAnalysisResult

        Dim ds As StatisticsObject = table.AsStatisticsObject(y, scale, transform)

        Call RequireResponse(ds, NameOf(oplsda))

        Return OPLS.OrthogonalProjectionsToLatentStructures(ds, component)
    End Function

    ''' <summary>
    ''' 判别分析需要响应变量，当表之中不存在任何可用的响应变量的时候给出明确的报错信息
    ''' </summary>
    Private Sub RequireResponse(ds As StatisticsObject, caller As String)
        If ds.YVariables Is Nothing OrElse ds.YVariables.Length = 0 Then
            Throw New InvalidOperationException(
                $"the '{caller}' analysis requires a response variable, but the given table has no label column for use as the response variable y!"
            )
        End If
    End Sub
End Module

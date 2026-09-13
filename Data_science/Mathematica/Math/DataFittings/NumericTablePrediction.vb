#Region "Microsoft.VisualBasic::NumericTablePrediction, Data_science\Mathematica\Math\DataFittings\NumericTablePrediction.vb"

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
''' 将回归模型的预测结果写回到统一二维表 <see cref="NumericTable"/> 的标签矩阵之中，
''' 从而可以方便地把预测值与原始的观测值放在同一张表里面进行对比。
''' </summary>
Public Module NumericTablePrediction

    ''' <summary>
    ''' 计算回归模型对表中每一个样本的预测值，并且写入指定的标签列
    ''' （缺省的列名为 ``prediction``）。
    ''' 
    ''' 当 <paramref name="withResidual"/> 为 True 的时候，还会额外写入一列
    ''' ``residual``，其数值为 ``预测值 - 观测值``。
    ''' </summary>
    ''' <param name="table">需要写入预测结果的二维表（**会被就地修改**）</param>
    ''' <param name="fitted">拟合出来的回归模型</param>
    ''' <param name="name">预测值写入的标签列名称</param>
    ''' <param name="withResidual">是否同时写入残差列 ``residual``</param>
    ''' <param name="y">
    ''' 观测值所在的标签列名，缺省为第一个标签列；
    ''' 只有在 <paramref name="withResidual"/> 为 True 的时候才会被使用
    ''' </param>
    ''' <returns>返回当前表对象以支持链式调用</returns>
    <Extension>
    Public Function SetPrediction(table As NumericTable,
                                  fitted As IFitted,
                                  Optional name As String = "prediction",
                                  Optional withResidual As Boolean = False,
                                  Optional y As String = Nothing) As NumericTable

        If table Is Nothing Then
            Throw New ArgumentNullException(NameOf(table))
        End If
        If fitted Is Nothing Then
            Throw New ArgumentNullException(NameOf(fitted))
        End If

        Dim n As Integer = table.nsamples
        Dim xm As Double()() = table.NumericRows
        Dim pred As Double() = New Double(n - 1) {}

        For i As Integer = 0 To n - 1
            pred(i) = fitted.GetY(xm(i))
        Next

        Call table.SetLabel(name, pred)

        If withResidual Then
            Dim obs As Double() = ObservedVector(table, y)
            Dim res As Double() = New Double(n - 1) {}

            For i As Integer = 0 To n - 1
                res(i) = pred(i) - obs(i)
            Next

            Call table.SetLabel("residual", res)
        End If

        Return table
    End Function

    ''' <summary>
    ''' 在源表的一个**深拷贝副本**上面计算预测值并且写入标签列，
    ''' 源表对象不会被修改。
    ''' </summary>
    ''' <param name="fitted">拟合出来的回归模型</param>
    ''' <param name="source">预测所使用的源数据表</param>
    ''' <param name="name">预测值写入的标签列名称</param>
    ''' <param name="withResidual">是否同时写入残差列 ``residual``</param>
    ''' <param name="y">观测值所在的标签列名，缺省为第一个标签列</param>
    ''' <returns>写入了预测结果的表副本</returns>
    <Extension>
    Public Function PredictTable(fitted As IFitted,
                                 source As NumericTable,
                                 Optional name As String = "prediction",
                                 Optional withResidual As Boolean = False,
                                 Optional y As String = Nothing) As NumericTable

        If fitted Is Nothing Then
            Throw New ArgumentNullException(NameOf(fitted))
        End If
        If source Is Nothing Then
            Throw New ArgumentNullException(NameOf(source))
        End If

        Return source.Clone().SetPrediction(fitted, name, withResidual, y)
    End Function

    ''' <summary>
    ''' 读取观测值向量：指定了 <paramref name="y"/> 的时候读取对应的标签列，
    ''' 否则自动读取第一个标签列
    ''' </summary>
    Private Function ObservedVector(table As NumericTable, y As String) As Double()
        Dim name As String = y

        If String.IsNullOrEmpty(name) Then
            If table.labelNames Is Nothing OrElse table.labelNames.Length = 0 OrElse table.labels Is Nothing Then
                Throw New ArgumentException(
                    "writing the residual column requires an observed value, but the given table has no label column for use as y!",
                    NameOf(table)
                )
            End If

            name = table.labelNames(0)
        End If

        Return table.GetLabel(name)
    End Function
End Module

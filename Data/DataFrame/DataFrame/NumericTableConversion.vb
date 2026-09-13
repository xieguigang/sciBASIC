#Region "Microsoft.VisualBasic::NumericTableConversion, Data\DataFrame\DataFrame\NumericTableConversion.vb"

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
''' <see cref="NumericTable"/> 与 <see cref="DataFrame"/> 之间的相互转换。
''' 
''' 其中标签列统一以 <see cref="NumericTableIO.DefaultLabelPrefix"/> 作为列名前缀参与转换，
''' 因此 ``table -> dataframe -> table`` 的往返是无损的：行名、特征列、标签列、
''' 表名与描述文本都可以完整地还原。
''' </summary>
Public Module NumericTableConversion

    ''' <summary>
    ''' 将二维表转换为数据帧对象：
    ''' 
    ''' 1. 全部特征列沿用 <see cref="NumericTable.featureNames"/> 作为列名
    ''' 2. 全部标签列使用 ``{labelPrefix}{标签列名}`` 作为列名
    ''' 3. 行名写入 <see cref="DataFrame.rownames"/>
    ''' </summary>
    ''' <param name="table">需要转换的二维表对象</param>
    ''' <param name="labelPrefix">标签列的列名前缀</param>
    ''' <returns></returns>
    <Extension>
    Public Function AsDataFrame(table As NumericTable,
                                Optional labelPrefix As String = NumericTableIO.DefaultLabelPrefix) As DataFrame

        If table Is Nothing Then
            Throw New ArgumentNullException(NameOf(table))
        End If

        Dim prefix As String = If(labelPrefix, NumericTableIO.DefaultLabelPrefix)
        Dim featureNames As String() = NumericTableIO.FeatureNameList(table)
        Dim n As Integer = table.nsamples
        Dim columns As New Dictionary(Of String, FeatureVector)

        For j As Integer = 0 To featureNames.Length - 1
            Call columns.Add(featureNames(j),
                             New FeatureVector(featureNames(j), Column(table.features, n, j)))
        Next

        If table.labels IsNot Nothing AndAlso table.labelNames IsNot Nothing Then
            For j As Integer = 0 To table.labelNames.Length - 1
                Dim name As String = prefix & table.labelNames(j)

                Call columns.Add(name, New FeatureVector(name, Column(table.labels, n, j)))
            Next
        End If

        Return New DataFrame With {
            .features = columns,
            .rownames = table.RowNamesOrDefault(),
            .name = table.name,
            .description = table.description
        }
    End Function

    ''' <summary>
    ''' 将数据帧对象转换为二维表：
    ''' 
    ''' 1. 列名以 <paramref name="labelPrefix"/> 开头的列归入标签矩阵（列名去掉前缀）
    ''' 2. 列名出现在 <paramref name="labels"/> 参数之中的列同样归入标签矩阵
    ''' 3. 其余的列归入特征矩阵；<see cref="DataFrame.rownames"/> 作为行名
    ''' </summary>
    ''' <param name="df">需要转换的数据帧对象</param>
    ''' <param name="labels">需要额外作为标签列的列名集合</param>
    ''' <param name="labelPrefix">标签列的列名前缀</param>
    ''' <param name="strict">
    ''' 无法解析为数值的单元格的处理策略：True 直接抛出 ``InvalidDataException``，
    ''' False 则置为 <see cref="Double.NaN"/>
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Public Function AsNumericTable(df As DataFrame,
                                   Optional labels As String() = Nothing,
                                   Optional labelPrefix As String = NumericTableIO.DefaultLabelPrefix,
                                   Optional strict As Boolean = True) As NumericTable

        If df Is Nothing Then
            Throw New ArgumentNullException(NameOf(df))
        End If

        Dim prefix As String = If(labelPrefix, NumericTableIO.DefaultLabelPrefix)
        Dim splits = NumericTableIO.SplitColumns(df.featureNames, labels, prefix)
        Dim n As Integer = df.nsamples
        Dim features As Double()() = NumericTableIO.ToMatrix(df, splits.features, n, strict)
        Dim hasLabels As Boolean = splits.labels.Length > 0

        Return New NumericTable(features, df.rownames, splits.features) With {
            .labels = If(hasLabels, NumericTableIO.ToMatrix(df, splits.labelColumns, n, strict), Nothing),
            .labelNames = If(hasLabels, splits.labels, Nothing),
            .name = df.name,
            .description = df.description
        }
    End Function

    ''' <summary>
    ''' 取出矩阵之中的指定列
    ''' </summary>
    Private Function Column(matrix As Double()(), n As Integer, index As Integer) As Double()
        Dim v As Double() = New Double(n - 1) {}

        For i As Integer = 0 To n - 1
            v(i) = matrix(i)(index)
        Next

        Return v
    End Function
End Module

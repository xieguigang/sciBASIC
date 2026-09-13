#Region "Microsoft.VisualBasic::NumericTableDataSet, Data_science\Mathematica\Math\ANOVA\MultivariateAnalysis\NumericTableDataSet.vb"

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

Imports System.Collections.ObjectModel
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data

''' <summary>
''' 将 Core 之中的统一二维表 <see cref="NumericTable"/> 适配为多变量分析所需要的
''' <see cref="StatisticsObject"/> 输入对象的帮助模块。
''' 
''' 表之中的特征矩阵会作为自变量 X，而指定的标签列则作为响应/分组变量 y。
''' </summary>
Public Module NumericTableDataSet

    ''' <summary>
    ''' 将二维表转换为多变量分析（PCA / PLS-DA / OPLS-DA）所使用的统计对象。
    ''' 
    ''' 其中：
    ''' 
    ''' 1. ``X`` 取表的**全部**特征列（<see cref="NumericTable.features"/>）
    ''' 2. ``y`` 取由参数 <paramref name="y"/> 指定的标签列；当该参数为空的时候
    '''    自动选取表中的第一个标签列
    ''' 3. 当表中没有任何标签列并且也没有指定 <paramref name="y"/> 的时候，
    '''    响应变量会是 Nothing（适用于不需要响应变量的 PCA 分析）；
    '''    对于 PLS-DA / OPLS-DA 这类需要响应变量的分析，请见
    '''    <see cref="NumericTableAnalysis.plsda"/> 与 <see cref="NumericTableAnalysis.oplsda"/>
    '''    的响应变量校验
    ''' </summary>
    ''' <param name="table">经过预处理之后的纯数值二维表</param>
    ''' <param name="y">
    ''' 响应/分组变量所在的标签列名。为空时自动选取第一个标签列
    ''' </param>
    ''' <param name="scale">
    ''' X 矩阵的缩放方法，缺省为 <see cref="ScaleMethod.AutoScale"/>
    ''' </param>
    ''' <param name="transform">
    ''' X 矩阵的变换方法，缺省为 <see cref="TransformMethod.None"/>
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Public Function AsStatisticsObject(table As NumericTable,
                                       Optional y As String = Nothing,
                                       Optional scale As ScaleMethod = ScaleMethod.AutoScale,
                                       Optional transform As TransformMethod = TransformMethod.None) As StatisticsObject

        If table Is Nothing Then
            Throw New ArgumentNullException(NameOf(table))
        End If

        Dim n As Integer = table.nsamples
        Dim m As Integer = table.nfeatures

        If n = 0 OrElse m = 0 Then
            Throw New InvalidConstraintException(
                $"the given table contains no feature data matrix({n} samples x {m} features) for run the multivariate analysis!"
            )
        End If

        ' ToMatrix会产生一个全新的Double(,)矩阵对象，
        ' 因此这里可以安全的直接把表之中的特征矩阵作为分析的输入，
        ' 而不会因为PCA等方法就地修改矩阵元素而污染到原始的表对象
        Dim xm As Double()() = table.NumericRows
        Dim yv As Double() = table.GetResponseVector(y)

        Dim ds As New StatisticsObject With {
            .XDataMatrix = xm.ToMatrix,
            .YVariables = yv,
            .Scale = scale,
            .Transform = transform
        }

        Call ds.StatInitialization()

        Dim xnames As String() = table.FeatureNamesOrDefault()
        Dim ynames As String() = table.RowNamesOrDefault()

        ds.XLabels = New ObservableCollection(Of String)(xnames)
        ds.YLabels = New ObservableCollection(Of String)(ynames)
        ds.YLabels2 = New ObservableCollection(Of String)(ynames)
        ds.XIndexes = New ObservableCollection(Of Integer)(Enumerable.Range(0, m))
        ds.YIndexes = New ObservableCollection(Of Integer)(Enumerable.Range(0, n))

        Return ds
    End Function

    ''' <summary>
    ''' 读取二维表之中的响应/分组变量向量：
    ''' 
    ''' 1. 指定了 <paramref name="y"/> 的时候直接读取对应的标签列
    ''' 2. 否则自动读取第一个标签列
    ''' 3. 表中没有任何标签列的时候返回 Nothing
    ''' </summary>
    ''' <param name="table"></param>
    ''' <param name="y">响应/分组变量所在的标签列名</param>
    ''' <returns></returns>
    <Extension>
    Public Function GetResponseVector(table As NumericTable, Optional y As String = Nothing) As Double()
        If table Is Nothing Then
            Throw New ArgumentNullException(NameOf(table))
        End If

        Dim name As String = y

        If String.IsNullOrEmpty(name) Then
            If table.labelNames Is Nothing OrElse table.labelNames.Length = 0 Then
                Return Nothing
            End If

            name = table.labelNames(0)
        End If

        Return table.GetLabel(name)
    End Function

    ''' <summary>
    ''' 获取表之中指定标签列的**列名**：指定了 <paramref name="y"/> 的时候直接返回该名称，
    ''' 否则返回表中的第一个标签列名；表中没有标签列的时候返回 Nothing。
    ''' </summary>
    <Extension>
    Public Function GetResponseName(table As NumericTable, Optional y As String = Nothing) As String
        If table Is Nothing Then
            Throw New ArgumentNullException(NameOf(table))
        End If

        If Not String.IsNullOrEmpty(y) Then
            Return y
        ElseIf table.labelNames Is Nothing OrElse table.labelNames.Length = 0 Then
            Return Nothing
        Else
            Return table.labelNames(0)
        End If
    End Function

    ''' <summary>
    ''' 获取特征矩阵的列名数组（缺失的时候自动生成 ``1..n`` 序号）
    ''' </summary>
    <Extension>
    Public Function FeatureNamesOrDefault(table As NumericTable) As String()
        If table Is Nothing Then
            Return New String() {}
        End If

        Dim m As Integer = table.nfeatures

        If table.featureNames IsNot Nothing AndAlso table.featureNames.Length = m Then
            Return table.featureNames
        End If

        Return Enumerable.Range(1, m).Select(Function(i) CStr(i)).ToArray
    End Function
End Module

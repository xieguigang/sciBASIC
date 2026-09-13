#Region "Microsoft.VisualBasic::NumericTableResult, Data_science\Mathematica\Math\ANOVA\MultivariateAnalysis\NumericTableResult.vb"

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
''' 将多变量分析的结果对象 <see cref="MultivariateAnalysisResult"/> 之中的
''' 降维投影结果（score）与载荷结果（loading）抽取为统一的二维表对象
''' <see cref="NumericTable"/>，从而可以方便的直接送入聚类、可视化等后续流程。
''' </summary>
Public Module NumericTableResult

    ''' <summary>
    ''' 提取降维投影结果（score）为二维表：
    ''' 
    ''' 1. 表的行是样本（行名取样本 ID）
    ''' 2. 表的特征是各个成分的投影坐标，列名分别为 ``PC1..PCn``（PCA）、
    '''    ``T1..Tn``（PLS-DA / OPLS-DA 的预测成分），OPLS-DA 还会额外追加
    '''    ``To1..Tom``（正交成分）
    ''' 3. 附带的 ``Y experiment``（观测值）与 ``Y predicted``（预测值）
    '''    作为标签列写入标签矩阵
    ''' </summary>
    ''' <param name="mvar">多变量分析的结果对象</param>
    ''' <returns></returns>
    <Extension>
    Public Function ScoreTable(mvar As MultivariateAnalysisResult) As NumericTable
        If mvar Is Nothing Then
            Throw New ArgumentNullException(NameOf(mvar))
        End If

        Dim stat As StatisticsObject = mvar.StatisticsObject
        Dim isPca As Boolean = mvar.analysis Is GetType(PCA)
        Dim isOpls As Boolean = IsOplsResult(mvar)
        Dim dims As New List(Of Double())
        Dim names As New List(Of String)

        Call AppendComponents(dims, names, mvar.TPreds, If(isPca, "PC", "T"))

        If isOpls Then
            Call AppendComponents(dims, names, mvar.ToPreds, "To")
        End If

        If dims.Count = 0 Then
            Throw New InvalidOperationException("no dimension reduction score data was found in the given analysis result!")
        End If

        Dim mat As Double()() = TransposeComponents(dims)
        Dim table As New NumericTable(mat, SampleNames(stat, mat.Length), names.ToArray) With {
            .name = AnalysisName(mvar, "score"),
            .description = "the dimension reduction score result of the multivariate analysis"
        }

        Call WriteLabels(table, "Y experiment", If(stat Is Nothing, Nothing, stat.YVariables))
        Call WriteLabels(table, "Y predicted", mvar.PredictedYs.ToArray)

        Return table
    End Function

    ''' <summary>
    ''' 提取载荷结果（loading）为二维表：
    ''' 
    ''' 1. 表的行是变量（行名取变量名）
    ''' 2. 表的特征是各个成分的载荷，列名分别为 ``PC1..PCn``（PCA）、
    '''    ``P1..Pn``（PLS-DA / OPLS-DA），OPLS-DA 还会额外追加 ``Po1..Pom``（正交载荷）
    ''' 3. 附带的 ``VIP`` 与 ``Coefficients``（回归系数）作为标签列写入标签矩阵
    ''' </summary>
    ''' <param name="mvar">多变量分析的结果对象</param>
    ''' <returns></returns>
    <Extension>
    Public Function LoadingTable(mvar As MultivariateAnalysisResult) As NumericTable
        If mvar Is Nothing Then
            Throw New ArgumentNullException(NameOf(mvar))
        End If

        Dim stat As StatisticsObject = mvar.StatisticsObject
        Dim isPca As Boolean = mvar.analysis Is GetType(PCA)
        Dim isOpls As Boolean = IsOplsResult(mvar)
        Dim dims As New List(Of Double())
        Dim names As New List(Of String)

        Call AppendComponents(dims, names, mvar.PPreds, If(isPca, "PC", "P"))

        If isOpls Then
            Call AppendComponents(dims, names, mvar.PoPreds, "Po")
        End If

        If dims.Count = 0 Then
            Throw New InvalidOperationException("no dimension reduction loading data was found in the given analysis result!")
        End If

        Dim mat As Double()() = TransposeComponents(dims)
        Dim table As New NumericTable(mat, VariableNames(stat, mat.Length), names.ToArray) With {
            .name = AnalysisName(mvar, "loading"),
            .description = "the variable loading result of the multivariate analysis"
        }

        Call WriteLabels(table, "VIP", mvar.Vips.ToArray)
        Call WriteLabels(table, "Coefficients", mvar.Coefficients.ToArray)

        Return table
    End Function

    ''' <summary>
    ''' 判断分析结果是否来自 OPLS-DA（无法从 <see cref="MultivariateAnalysisResult.analysis"/>
    ''' 判定的时候，通过对角成分是否存在进行推断）
    ''' </summary>
    Private Function IsOplsResult(mvar As MultivariateAnalysisResult) As Boolean
        If mvar.analysis Is GetType(OPLS) Then
            Return True
        ElseIf mvar.analysis IsNot Nothing Then
            Return False
        Else
            Return mvar.ToPreds.Count > 0 OrElse mvar.PoPreds.Count > 0
        End If
    End Function

    Private Function AnalysisName(mvar As MultivariateAnalysisResult, suffix As String) As String
        Dim prefix As String

        If mvar.analysis Is GetType(PCA) Then
            prefix = "PCA"
        ElseIf IsOplsResult(mvar) Then
            prefix = "OPLS-DA"
        Else
            prefix = "PLS-DA"
        End If

        Return $"{prefix} {suffix}"
    End Function

    ''' <summary>
    ''' 将各个成分的投影向量收集起来：<paramref name="dims"/> 为各成分的向量，
    ''' <paramref name="names"/> 为对应的列名（``{prefix}{i}``）
    ''' </summary>
    Private Sub AppendComponents(dims As List(Of Double()), names As List(Of String), components As IEnumerable(Of Double()), prefix As String)
        Dim i As Integer = 0

        For Each v As Double() In components
            If v Is Nothing Then
                Continue For
            End If

            i += 1
            Call names.Add($"{prefix}{i}")
            Call dims.Add(v)
        Next
    End Sub

    ''' <summary>
    ''' 将各个成分的向量转置为 ``样本 x 成分`` 的行主序矩阵
    ''' </summary>
    Private Function TransposeComponents(dims As List(Of Double())) As Double()()
        Dim n As Integer = dims.Select(Function(v) v.Length).Max
        Dim mat As Double()() = New Double(n - 1)() {}

        For i As Integer = 0 To n - 1
            mat(i) = New Double(dims.Count - 1) {}
        Next

        For j As Integer = 0 To dims.Count - 1
            Dim v As Double() = dims(j)

            For i As Integer = 0 To v.Length - 1
                mat(i)(j) = v(i)
            Next
        Next

        Return mat
    End Function

    ''' <summary>
    ''' 取样本的行名：优先使用样本 ID（<see cref="StatisticsObject.YLabels2"/>），
    ''' 其次是 <see cref="StatisticsObject.YLabels"/>，缺失的时候生成 ``1..n`` 序号
    ''' </summary>
    Private Function SampleNames(stat As StatisticsObject, n As Integer) As String()
        Dim names As String() = Nothing

        If stat IsNot Nothing Then
            If stat.YLabels2 IsNot Nothing AndAlso stat.YLabels2.Count = n Then
                names = stat.YLabels2.ToArray
            ElseIf stat.YLabels IsNot Nothing AndAlso stat.YLabels.Count = n Then
                names = stat.YLabels.ToArray
            End If
        End If

        If names Is Nothing Then
            names = Enumerable.Range(1, n).Select(Function(i) CStr(i)).ToArray
        End If

        Return names
    End Function

    ''' <summary>
    ''' 取变量的行名：优先使用 <see cref="StatisticsObject.XLabels"/>，
    ''' 缺失的时候生成 ``1..n`` 序号
    ''' </summary>
    Private Function VariableNames(stat As StatisticsObject, n As Integer) As String()
        Dim names As String() = Nothing

        If stat IsNot Nothing AndAlso stat.XLabels IsNot Nothing AndAlso stat.XLabels.Count = n Then
            names = stat.XLabels.ToArray
        End If

        If names Is Nothing Then
            names = Enumerable.Range(1, n).Select(Function(i) CStr(i)).ToArray
        End If

        Return names
    End Function

    ''' <summary>
    ''' 将一列附加数据写入结果表的标签矩阵（长度和样本数量不一致的时候自动忽略）
    ''' </summary>
    Private Sub WriteLabels(table As NumericTable, name As String, values As Double())
        If values IsNot Nothing AndAlso values.Length = table.nsamples Then
            Call table.SetLabel(name, values)
        End If
    End Sub
End Module

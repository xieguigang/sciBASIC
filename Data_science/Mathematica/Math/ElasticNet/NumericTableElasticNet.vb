Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data

''' <summary>
''' 弹性网络回归的统一二维表入口
''' 
''' 这一组扩展方法必须留在 ElasticNet 程序集里面：本工程引用了 DataFittings，
''' 而 DataFittings 并不（也不可能）反过来引用本工程，因此把入口写在
''' DataFittings 里会形成循环依赖。
''' 
''' 输入约定与 DataFittings 的其它回归入口一致：自变量取表的**全部**特征列，
''' 响应变量由参数 ``y`` 指定标签列名、缺省为第一个标签列。
''' </summary>
Public Module NumericTableElasticNet

    ''' <summary>
    ''' 对二维表执行弹性网络回归
    ''' </summary>
    ''' <param name="table">特征矩阵为 X、指定标签列为 y 的二维表</param>
    ''' <param name="alpha">总的正则化强度</param>
    ''' <param name="l1Ratio">L1 惩罚所占的混合比例，取值 [0, 1]</param>
    ''' <param name="standardize">是否先对特征做标准化</param>
    ''' <param name="tolerance">坐标下降的收敛容差</param>
    ''' <param name="maxIterations">坐标下降的最大迭代轮数</param>
    ''' <param name="y">响应变量所在的标签列名，缺省为第一个标签列</param>
    ''' <returns>拟合结果</returns>
    <Extension>
    Public Function ElasticNetRegression(table As NumericTable,
                                         alpha As Double,
                                         l1Ratio As Double,
                                         Optional standardize As Boolean = True,
                                         Optional tolerance As Double = 0.0001,
                                         Optional maxIterations As Integer = 10000,
                                         Optional y As String = Nothing) As ElasticNetFit

        Dim xm As Double()() = FeatureValues(table, NameOf(ElasticNetRegression))
        Dim yv As Double() = ResponseValues(table, y, NameOf(ElasticNetRegression))

        Return ElasticNetFitting.Fit(
            x:=xm,
            y:=yv,
            alpha:=alpha,
            l1Ratio:=l1Ratio,
            standardize:=standardize,
            tolerance:=tolerance,
            maxIterations:=maxIterations,
            featureNames:=FeatureNames(table)
        )
    End Function

    ''' <summary>
    ''' 对二维表求解弹性网络的正则化路径（alpha 由大到小）
    ''' </summary>
    ''' <param name="table">特征矩阵为 X、指定标签列为 y 的二维表</param>
    ''' <param name="l1Ratio">L1 惩罚所占的混合比例</param>
    ''' <param name="numberOfAlphas">路径的步数</param>
    ''' <param name="standardize">是否先对特征做标准化</param>
    ''' <param name="tolerance">坐标下降的收敛容差</param>
    ''' <param name="maxIterations">每一步的最大迭代轮数</param>
    ''' <param name="y">响应变量所在的标签列名，缺省为第一个标签列</param>
    ''' <returns>正则化路径</returns>
    <Extension>
    Public Function ElasticNetRegularizationPath(table As NumericTable,
                                                 l1Ratio As Double,
                                                 Optional numberOfAlphas As Integer = 100,
                                                 Optional standardize As Boolean = True,
                                                 Optional tolerance As Double = 0.0001,
                                                 Optional maxIterations As Integer = 10000,
                                                 Optional y As String = Nothing) As ElasticNetPath

        Dim xm As Double()() = FeatureValues(table, NameOf(ElasticNetRegularizationPath))
        Dim yv As Double() = ResponseValues(table, y, NameOf(ElasticNetRegularizationPath))

        Return ElasticNetFitting.RegularizationPath(
            x:=xm,
            y:=yv,
            l1Ratio:=l1Ratio,
            numberOfAlphas:=numberOfAlphas,
            standardize:=standardize,
            tolerance:=tolerance,
            maxIterations:=maxIterations,
            featureNames:=FeatureNames(table)
        )
    End Function

    ''' <summary>
    ''' 对二维表执行弹性网络的交叉验证，自动选择最优的 ``alpha`` 与 ``l1Ratio``
    ''' </summary>
    ''' <param name="table">特征矩阵为 X、指定标签列为 y 的二维表</param>
    ''' <param name="l1Ratios">待搜索的 L1 混合比例网格，缺省为 sklearn 的默认值</param>
    ''' <param name="numberOfAlphas">每一比例下的 alpha 路径步数</param>
    ''' <param name="folds">交叉验证的折数</param>
    ''' <param name="standardize">是否先对特征做标准化</param>
    ''' <param name="tolerance">坐标下降的收敛容差</param>
    ''' <param name="maxIterations">每一步的最大迭代轮数</param>
    ''' <param name="y">响应变量所在的标签列名，缺省为第一个标签列</param>
    ''' <returns>交叉验证结果（含重新拟合的最终模型）</returns>
    <Extension>
    Public Function ElasticNetCrossValidate(table As NumericTable,
                                            Optional l1Ratios As Double() = Nothing,
                                            Optional numberOfAlphas As Integer = 50,
                                            Optional folds As Integer = 5,
                                            Optional standardize As Boolean = True,
                                            Optional tolerance As Double = 0.0001,
                                            Optional maxIterations As Integer = 10000,
                                            Optional y As String = Nothing) As ElasticNetCVResult

        Dim xm As Double()() = FeatureValues(table, NameOf(ElasticNetCrossValidate))
        Dim yv As Double() = ResponseValues(table, y, NameOf(ElasticNetCrossValidate))

        Return ElasticNetFitting.CrossValidate(
            x:=xm,
            y:=yv,
            l1Ratios:=l1Ratios,
            numberOfAlphas:=numberOfAlphas,
            folds:=folds,
            standardize:=standardize,
            tolerance:=tolerance,
            maxIterations:=maxIterations,
            featureNames:=FeatureNames(table)
        )
    End Function

    ''' <summary>
    ''' 读取特征矩阵
    ''' </summary>
    Private Function FeatureValues(table As NumericTable, caller As String) As Double()()
        If table Is Nothing Then
            Throw New ArgumentNullException(NameOf(table))
        End If
        If table.nsamples = 0 OrElse table.nfeatures = 0 Then
            Throw New ArgumentException($"the '{caller}' requires a non-empty feature matrix!", NameOf(table))
        End If

        Return table.NumericRows
    End Function

    ''' <summary>
    ''' 读取响应变量：指定了名称的时候读取对应的标签列，否则读取第一个标签列
    ''' </summary>
    Private Function ResponseValues(table As NumericTable, y As String, caller As String) As Double()
        Dim name As String = y

        If String.IsNullOrEmpty(name) Then
            If table.labelNames Is Nothing OrElse table.labelNames.Length = 0 OrElse table.labels Is Nothing Then
                Throw New ArgumentException(
                    $"the '{caller}' requires a response variable, but the given table has no label column for use as the response variable y!",
                    NameOf(table)
                )
            End If

            name = table.labelNames(0)
        End If

        Return table.GetLabel(name)
    End Function

    ''' <summary>
    ''' 读取特征列的名称（缺失的时候自动生成 ``1..n``）
    ''' </summary>
    Private Function FeatureNames(table As NumericTable) As String()
        Dim m As Integer = table.nfeatures

        If table.featureNames IsNot Nothing AndAlso table.featureNames.Length = m Then
            Return table.featureNames
        End If

        Return Enumerable.Range(1, m).Select(Function(i) CStr(i)).ToArray
    End Function
End Module

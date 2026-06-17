Imports Microsoft.VisualBasic.Math.Statistics.Distributions

Public Class AnovaFTest

    ' 分组数据：每个元素是一个分组在该代谢物上的表达量数组
    Private ReadOnly groups As Double()()
    Private _fStat As Double
    Private _pValue As Double

    ''' <summary>
    ''' 组间自由度 (k - 1)
    ''' </summary>
    Public ReadOnly Property DfBetween As Integer
        Get
            Return groups.Length - 1
        End Get
    End Property

    ''' <summary>
    ''' 组内自由度 (N - k)
    ''' </summary>
    Public ReadOnly Property DfWithin As Integer
        Get
            Dim total As Integer = 0
            For Each g In groups
                total += g.Length
            Next
            Return total - groups.Length
        End Get
    End Property

    Public ReadOnly Property FStatistic As Double
        Get
            Return _fStat
        End Get
    End Property

    Public ReadOnly Property PValue As Double
        Get
            Return _pValue
        End Get
    End Property

    ''' <summary>
    ''' 初始化 ANOVA F 检验
    ''' </summary>
    ''' <param name="groupedData">分组表达量数组，例如 5个分组：Dim data = {group1Array, group2Array, ...}</param>
    Sub New(groupedData As Double()())
        Me.groups = groupedData
        Calculate()
    End Sub

    Private Sub Calculate()
        Dim k As Integer = groups.Length
        If k < 2 Then
            _pValue = 1.0
            _fStat = 0.0
            Return
        End If

        Dim allData As New List(Of Double)
        For Each g In groups
            allData.AddRange(g)
        Next

        Dim N As Integer = allData.Count
        Dim grandMean As Double = allData.Average()

        ' 计算 SS_between (组间平方和)
        Dim ssBetween As Double = 0.0
        For Each g In groups
            Dim groupMean As Double = g.Average()
            ssBetween += g.Length * ((groupMean - grandMean) ^ 2)
        Next

        ' 计算 SS_within (组内平方和)
        Dim ssWithin As Double = 0.0
        For Each g In groups
            Dim groupMean As Double = g.Average()
            For Each val As Double In g
                ssWithin += (val - groupMean) ^ 2
            Next
        Next

        ' 均方
        Dim msBetween As Double = ssBetween / DfBetween
        Dim msWithin As Double = ssWithin / DfWithin

        ' 处理分母为0的极端情况（所有样本表达量一致）
        If msWithin = 0 Then
            If msBetween = 0 Then
                _fStat = 0
                _pValue = 1.0
            Else
                _fStat = Double.PositiveInfinity
                _pValue = 0.0
            End If
            Return
        End If

        ' F 统计量
        _fStat = msBetween / msWithin

        ' 计算 P 值 (右尾概率)
        _pValue = Distribution.FDistribution(_fStat, DfBetween, DfWithin)
    End Sub
End Class

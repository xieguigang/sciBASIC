''' <summary>
''' 用于报告基于迭代的机器学习算法的状态进度之类的信息的框架
''' </summary>
''' <remarks>
''' 这个对象模块应该是应用于训练部分的模块
''' </remarks>
Public MustInherit Class IterationReporter

    Protected reporter As Action(Of IterationReporter)

    Public Function AttachReporter(reporter As Action(Of IterationReporter)) As IterationReporter
        Me.reporter = reporter
        Return Me
    End Function


End Class

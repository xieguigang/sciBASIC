''' <summary>
''' 用于报告基于迭代的机器学习算法的状态进度之类的信息的框架
''' </summary>
''' <remarks>
''' 这个对象模块应该是应用于训练部分的模块
''' </remarks>
Public MustInherit Class IterationReporter(Of T As Model)

    Protected reporter As DoReport

    Public Delegate Sub DoReport(iteration%, error#, model As T)

    Public Function AttachReporter(reporter As DoReport) As IterationReporter(Of T)
        Me.reporter = reporter
        Return Me
    End Function

    Public MustOverride Sub Train(Optional parallel As Boolean = False)

End Class

Public MustInherit Class Model

End Class
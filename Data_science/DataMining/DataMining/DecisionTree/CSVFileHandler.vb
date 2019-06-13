Namespace DecisionTree

    ''' <summary>
    ''' 分类用的对象实例
    ''' </summary>
    Public Class Entity

        Public Property variables As String()
        ''' <summary>
        ''' 分类结果
        ''' </summary>
        ''' <returns></returns>
        Public Property decisions As String

        Public Overrides Function ToString() As String
            Return decisions
        End Function

    End Class
End Namespace
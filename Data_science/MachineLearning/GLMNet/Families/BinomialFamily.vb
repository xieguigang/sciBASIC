''' <summary>
''' <c>family = "binomial"</c>：带弹性网惩罚的逻辑回归（lognet）。
''' </summary>
''' <remarks>
''' 支持 0/1 因子、比例响应以及两列 <c>(success, failure)</c> 矩阵；
''' 后者在 <see cref="GlmnetData"/> 中已转换为比例并把试验次数并入权重。
''' </remarks>
Friend NotInheritable Class BinomialFamily : Implements IGlmnetFamily

    Public ReadOnly Property Kind As FamilyKind Implements IGlmnetFamily.Kind
        Get
            Return FamilyKind.Binomial
        End Get
    End Property

    Public Function Fit(data As GlmnetData, options As GlmnetOptions) As FamilyPath Implements IGlmnetFamily.Fit
        Return SingleResponsePath.Fit(data, options, FamilyKind.Binomial)
    End Function

    Public Function Measure(linkPredictions As Double()()(), truth As GlmnetData, metric As CVMeasure) As Double() Implements IGlmnetFamily.Measure
        Return TypeMeasure.EvaluateSingle(linkPredictions, truth, metric, FamilyKind.Binomial)
    End Function

    Public Function LinkToResponse(link As Double()()) As Double()() Implements IGlmnetFamily.LinkToResponse
        Dim result As Double()() = New Double(link.Length - 1)() {}
        For i As Integer = 0 To link.Length - 1
            Dim row As Double() = New Double(link(i).Length - 1) {}
            For c As Integer = 0 To link(i).Length - 1
                row(c) = IrlsKernel.Sigmoid(link(i)(c))
            Next
            result(i) = row
        Next
        Return result
    End Function

    Public Function IsMaximize(measure As CVMeasure) As Boolean Implements IGlmnetFamily.IsMaximize
        Return TypeMeasure.IsMaximize(measure)
    End Function

End Class

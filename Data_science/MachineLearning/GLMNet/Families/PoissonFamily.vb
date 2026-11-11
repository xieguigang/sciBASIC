''' <summary>
''' <c>family = "poisson"</c>：带弹性网惩罚的对数链接计数回归（fishnet）。
''' </summary>
''' <remarks>
''' offset 直接加在 link scale 上（与 R glmnet 一致）；既有
''' <c>PoissonFitting</c> 的 <c>exposure</c> 是原始暴露量、内部取 log，
''' 且强制 n &gt; p，因此本内核独立实现，不复用其 IRLS。
''' </remarks>
Friend NotInheritable Class PoissonFamily : Implements IGlmnetFamily

    Public ReadOnly Property Kind As FamilyKind Implements IGlmnetFamily.Kind
        Get
            Return FamilyKind.Poisson
        End Get
    End Property

    Public Function Fit(data As GlmnetData, options As GlmnetOptions) As FamilyPath Implements IGlmnetFamily.Fit
        Return SingleResponsePath.Fit(data, options, FamilyKind.Poisson)
    End Function

    Public Function Measure(linkPredictions As Double()()(), truth As GlmnetData, measure As CVMeasure) As Double() Implements IGlmnetFamily.Measure
        Return TypeMeasure.EvaluateSingle(linkPredictions, truth, measure, FamilyKind.Poisson)
    End Function

    Public Function LinkToResponse(link As Double()()) As Double()() Implements IGlmnetFamily.LinkToResponse
        Dim result As Double()() = New Double(link.Length - 1)() {}
        For i As Integer = 0 To link.Length - 1
            Dim row As Double() = New Double(link(i).Length - 1) {}
            For c As Integer = 0 To link(i).Length - 1
                row(c) = Convergence.SafeExp(link(i)(c))
            Next
            result(i) = row
        Next
        Return result
    End Function

    Public Function IsMaximize(measure As CVMeasure) As Boolean Implements IGlmnetFamily.IsMaximize
        Return TypeMeasure.IsMaximize(measure)
    End Function

End Class

''' <summary><c>family = "gaussian"</c>：单响应线性弹性网（elnet）。</summary>
Friend NotInheritable Class GaussianFamily : Implements IGlmnetFamily

    Public ReadOnly Property Kind As FamilyKind Implements IGlmnetFamily.Kind
        Get
            Return FamilyKind.Gaussian
        End Get
    End Property

    Public Function Fit(data As GlmnetData, options As GlmnetOptions) As FamilyPath Implements IGlmnetFamily.Fit
        Return SingleResponsePath.Fit(data, options, FamilyKind.Gaussian)
    End Function

    Public Function Measure(linkPredictions As Double()()(), truth As GlmnetData, metric As CVMeasure) As Double() Implements IGlmnetFamily.Measure
        Return TypeMeasure.EvaluateSingle(linkPredictions, truth, metric, FamilyKind.Gaussian)
    End Function

    Public Function LinkToResponse(link As Double()()) As Double()() Implements IGlmnetFamily.LinkToResponse
        Dim result As Double()() = New Double(link.Length - 1)() {}
        For i As Integer = 0 To link.Length - 1
            result(i) = New Double(link(i).Length - 1) {}
            Call Array.Copy(link(i), result(i), link(i).Length)
        Next
        Return result
    End Function

    Public Function IsMaximize(measure As CVMeasure) As Boolean Implements IGlmnetFamily.IsMaximize
        Return TypeMeasure.IsMaximize(measure)
    End Function

End Class

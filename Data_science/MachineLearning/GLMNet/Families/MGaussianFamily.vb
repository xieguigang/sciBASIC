Imports MGaussianNS = Microsoft.VisualBasic.Data.Bootstrapping.MGaussian
Imports std = System.Math

''' <summary>
''' <c>family = "mgaussian"</c>：多响应高斯组-lasso（mrelnet）。
''' </summary>
''' <remarks>
''' 复用 <c>DataFittings</c> 中已有的块坐标下降内核
''' （<c>standardize.response</c>、weights、2-norm 路径、warm start），
''' 并把 <c>MGaussianFit</c> 映射为统一的 <see cref="FamilyPath"/>。
''' </remarks>
Friend NotInheritable Class MGaussianFamily : Implements IGlmnetFamily

    Public ReadOnly Property Kind As FamilyKind Implements IGlmnetFamily.Kind
        Get
            Return FamilyKind.MGaussian
        End Get
    End Property

    Public Function Fit(data As GlmnetData, options As GlmnetOptions) As FamilyPath Implements IGlmnetFamily.Fit
        Dim model As MGaussianNS.MGaussianFit = MGaussianNS.MGaussianFitting.Fit(
            x:=data.X,
            y:=data.Y,
            lambda:=options.Lambda,
            alpha:=options.Alpha,
            numberOfLambdas:=options.NLambda,
            standardize:=options.Standardize,
            standardizeResponse:=options.StandardizeResponse,
            intercept:=options.Intercept,
            weights:=data.Weights,
            maxIterations:=std.Min(options.MaxIt, 100000),
            tolerance:=std.Max(options.Thresh, 0.0000000001R),
            featureNames:=data.FeatureNames,
            responseNames:=data.ResponseNames,
            Silent:=True)

        Dim steps As Integer = model.lambdas.Length
        Dim p As Integer = data.P
        Dim k As Integer = data.K
        Dim nullMse As Double = NullMeanSquare(data)

        Dim allA0 As Double()() = New Double(steps - 1)() {}
        Dim allBeta As Double()()() = New Double(steps - 1)()() {}
        Dim allDf As Integer() = New Integer(steps - 1) {}
        Dim allNZero As Integer() = New Integer(steps - 1) {}
        Dim allDev As Double() = New Double(steps - 1) {}
        Dim allDevRatio As Double() = New Double(steps - 1) {}
        Dim allConverged As Boolean() = New Boolean(steps - 1) {}
        Dim allIterations As Integer() = New Integer(steps - 1) {}

        For s As Integer = 0 To steps - 1
            allA0(s) = CType(model.intercepts(s).Clone(), Double())
            Dim matrix As Double()() = New Double(p - 1)() {}
            For j As Integer = 0 To p - 1
                matrix(j) = CType(model.coefficients(s)(j).Clone(), Double())
            Next
            allBeta(s) = matrix
            allDf(s) = model.df(s)
            allNZero(s) = model.df(s) * k
            allDev(s) = model.trainMse(s) * CDbl(data.N)
            allDevRatio(s) = If(nullMse > 0.0R, std.Max(0.0R, 1.0R - model.trainMse(s) / nullMse), 0.0R)
            allConverged(s) = model.converged(s)
            allIterations(s) = model.iterations(s)
        Next

        Return New FamilyPath With {
            .Lambda = CType(model.lambdas.Clone(), Double()),
            .A0 = allA0,
            .Beta = allBeta,
            .Df = allDf,
            .NZero = allNZero,
            .Deviance = allDev,
            .DevRatio = allDevRatio,
            .NullDev = nullMse * CDbl(data.N),
            .Converged = allConverged,
            .Iterations = allIterations,
            .Jerr = GlmnetError.None,
            .Family = FamilyKind.MGaussian,
            .Options = options,
            .FeatureNames = data.FeatureNames,
            .ResponseNames = data.ResponseNames,
            .TrainingData = data
        }
    End Function

    ''' <summary>零模型（各响应加权均值）的均方误差，用于 dev.ratio。</summary>
    Private Function NullMeanSquare(data As GlmnetData) As Double
        Dim n As Integer = data.N
        Dim k As Integer = data.K
        Dim means As Double() = New Double(k - 1) {}

        For c As Integer = 0 To k - 1
            Dim sum As Double = 0.0R
            For i As Integer = 0 To n - 1
                sum += data.Weights(i) * data.Y(i)(c)
            Next
            means(c) = sum / CDbl(n)
        Next

        Dim total As Double = 0.0R
        For i As Integer = 0 To n - 1
            For c As Integer = 0 To k - 1
                Dim d As Double = data.Y(i)(c) - means(c)
                total += data.Weights(i) * d * d
            Next
        Next

        Return total / CDbl(n * k)
    End Function

    Public Function Measure(linkPredictions As Double()()(), truth As GlmnetData, measure As CVMeasure) As Double() Implements IGlmnetFamily.Measure
        Return TypeMeasure.EvaluateMulti(linkPredictions, truth, measure, FamilyKind.MGaussian)
    End Function

    Public Function LinkToResponse(link As Double()()) As Double()() Implements IGlmnetFamily.LinkToResponse
        Dim result As Double()() = New Double(link.Length - 1)() {}
        For i As Integer = 0 To link.Length - 1
            result(i) = CType(link(i).Clone(), Double())
        Next
        Return result
    End Function

    Public Function IsMaximize(measure As CVMeasure) As Boolean Implements IGlmnetFamily.IsMaximize
        Return TypeMeasure.IsMaximize(measure)
    End Function

End Class

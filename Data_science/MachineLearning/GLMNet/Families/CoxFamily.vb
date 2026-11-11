Imports CoxNS = Microsoft.VisualBasic.Data.Bootstrapping.Cox
Imports std = System.Math

''' <summary>
''' <c>family = "cox"</c>：Cox 比例风险模型（coxnet）。
''' </summary>
''' <remarks>
''' 复用 <c>DataFittings</c> 中已有的偏似然 + 弹性网坐标下降内核
''' （Breslow / Efron 平局、penalty.factor、C 指数、基线生存），
''' 并把 <c>CoxFit</c> 映射为统一的 <see cref="FamilyPath"/>。
''' glmnet 5.1 的默认平局方式为 Efron，与既有内核的默认值一致。
''' </remarks>
Friend NotInheritable Class CoxFamily : Implements IGlmnetFamily

    Public ReadOnly Property Kind As FamilyKind Implements IGlmnetFamily.Kind
        Get
            Return FamilyKind.Cox
        End Get
    End Property

    Public Function Fit(data As GlmnetData, options As GlmnetOptions) As FamilyPath Implements IGlmnetFamily.Fit
        Dim ties As CoxNS.CoxTies = If(options.CoxTies = CoxTiesMode.Efron,
                                        CoxNS.CoxTies.Efron, CoxNS.CoxTies.Breslow)

        Dim model As CoxNS.CoxFit = CoxNS.CoxFitting.Fit(
            x:=data.X,
            y:=data.Y,
            lambda:=options.Lambda,
            alpha:=options.Alpha,
            numberOfLambdas:=options.NLambda,
            standardize:=options.Standardize,
            ties:=ties,
            penaltyFactor:=options.PenaltyFactor,
            maxIterations:=std.Min(options.MaxIt, 100000),
            tolerance:=std.Max(options.Thresh, 0.0000000001R),
            featureNames:=data.FeatureNames,
            Silent:=True)

        Dim steps As Integer = model.lambdas.Length
        Dim p As Integer = data.P
        Dim allA0 As Double()() = New Double(steps - 1)() {}
        Dim allBeta As Double()()() = New Double(steps - 1)()() {}
        Dim allDf As Integer() = New Integer(steps - 1) {}
        Dim allNZero As Integer() = New Integer(steps - 1) {}
        Dim allDev As Double() = New Double(steps - 1) {}
        Dim allDevRatio As Double() = New Double(steps - 1) {}
        Dim allConverged As Boolean() = New Boolean(steps - 1) {}
        Dim allIterations As Integer() = New Integer(steps - 1) {}

        For s As Integer = 0 To steps - 1
            allA0(s) = New Double() {0.0R}
            Dim matrix As Double()() = New Double(p - 1)() {}
            For j As Integer = 0 To p - 1
                matrix(j) = New Double() {model.coefficients(s)(j)}
            Next
            allBeta(s) = matrix
            allDf(s) = model.df(s)
            allNZero(s) = model.df(s)
            allDev(s) = -2.0R * model.logPL(s)
            allDevRatio(s) = model.devRatio(s)
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
            .NullDev = -2.0R * model.nullLogLikelihood,
            .Converged = allConverged,
            .Iterations = allIterations,
            .Jerr = GlmnetError.None,
            .Family = FamilyKind.Cox,
            .Options = options,
            .FeatureNames = data.FeatureNames,
            .ResponseNames = data.ResponseNames,
            .TrainingData = data
        }
    End Function

    Public Function Measure(linkPredictions As Double()()(), truth As GlmnetData, metric As CVMeasure) As Double() Implements IGlmnetFamily.Measure
        Dim steps As Integer = linkPredictions.Length
        Dim n As Integer = truth.N
        Dim time As Double() = New Double(n - 1) {}
        Dim status As Double() = New Double(n - 1) {}
        For i As Integer = 0 To n - 1
            time(i) = truth.Y(i)(0)
            status(i) = truth.Y(i)(1)
        Next

        Dim result As Double() = New Double(steps - 1) {}
        For s As Integer = 0 To steps - 1
            Dim risk As Double() = New Double(n - 1) {}
            For i As Integer = 0 To n - 1
                risk(i) = linkPredictions(s)(i)(0)
            Next

            If metric = CVMeasure.CIndex Then
                result(s) = TypeMeasure.CIndex(risk, time, status)
            Else
                result(s) = -2.0R * PartialLogLikelihood(time, status, risk) / CDbl(n)
            End If
        Next

        Return result
    End Function

    ''' <summary>Breslow 偏对数似然（按时间降序累加风险集）。</summary>
    Private Function PartialLogLikelihood(time As Double(), status As Double(), risk As Double()) As Double
        Dim n As Integer = time.Length
        Dim order As Integer() = New Integer(n - 1) {}
        For i As Integer = 0 To n - 1
            order(i) = i
        Next
        Call Array.Sort(time.Clone(), order)
        Array.Reverse(order)

        Dim maxRisk As Double = Double.NegativeInfinity
        For i As Integer = 0 To n - 1
            If risk(i) > maxRisk Then maxRisk = risk(i)
        Next

        Dim logPl As Double = 0.0R
        Dim riskSum As Double = 0.0R
        For Each i As Integer In order
            riskSum += std.Exp(risk(i) - maxRisk)
            If status(i) > 0.0R Then
                logPl += (risk(i) - maxRisk) - std.Log(std.Max(riskSum, GlmnetControl.MinCurvature))
            End If
        Next

        Return logPl
    End Function

    Public Function LinkToResponse(link As Double()()) As Double()() Implements IGlmnetFamily.LinkToResponse
        Dim result As Double()() = New Double(link.Length - 1)() {}
        For i As Integer = 0 To link.Length - 1
            result(i) = New Double() {Convergence.SafeExp(link(i)(0))}
        Next
        Return result
    End Function

    Public Function IsMaximize(measure As CVMeasure) As Boolean Implements IGlmnetFamily.IsMaximize
        Return TypeMeasure.IsMaximize(measure)
    End Function

End Class

''' <summary>
''' family 字符串 / 枚举解析与求解器分发。
''' </summary>
Friend Module FamilyRegistry

    ''' <summary>把 R 风格的 family 名称解析为枚举。</summary>
    Public Function Parse(family As String) As FamilyKind
        If String.IsNullOrEmpty(family) Then
            Throw New ArgumentException("family must not be empty!", NameOf(family))
        End If

        Select Case family.Trim().ToLowerInvariant()
            Case "gaussian", "elnet"
                Return FamilyKind.Gaussian
            Case "binomial", "lognet", "logistic"
                Return FamilyKind.Binomial
            Case "poisson", "fishnet"
                Return FamilyKind.Poisson
            Case "multinomial", "multnet"
                Return FamilyKind.Multinomial
            Case "cox", "coxnet"
                Return FamilyKind.Cox
            Case "mgaussian", "mrelnet"
                Return FamilyKind.MGaussian
            Case Else
                Throw New ArgumentException($"unsupported glmnet family: '{family}'!", NameOf(family))
        End Select
    End Function

    ''' <summary>返回 R 端使用的 family 名称。</summary>
    Public Function Name(kind As FamilyKind) As String
        Select Case kind
            Case FamilyKind.Gaussian : Return "gaussian"
            Case FamilyKind.Binomial : Return "binomial"
            Case FamilyKind.Poisson : Return "poisson"
            Case FamilyKind.Multinomial : Return "multinomial"
            Case FamilyKind.Cox : Return "cox"
            Case FamilyKind.MGaussian : Return "mgaussian"
            Case Else : Throw New ArgumentOutOfRangeException(NameOf(kind))
        End Select
    End Function

    ''' <summary>创建对应 family 的求解器实例。</summary>
    Public Function Create(kind As FamilyKind) As IGlmnetFamily
        Select Case kind
            Case FamilyKind.Gaussian : Return New GaussianFamily()
            Case FamilyKind.Binomial : Return New BinomialFamily()
            Case FamilyKind.Poisson : Return New PoissonFamily()
            Case FamilyKind.Multinomial : Return New MultinomialFamily()
            Case FamilyKind.Cox : Return New CoxFamily()
            Case FamilyKind.MGaussian : Return New MGaussianFamily()
            Case Else : Throw New ArgumentOutOfRangeException(NameOf(kind))
        End Select
    End Function

    ''' <summary>返回该 family 的默认折外评估指标名称。</summary>
    Public Function MeasureName(measure As CVMeasure) As String
        Select Case measure
            Case CVMeasure.Mse : Return "Mean-Squared Error"
            Case CVMeasure.Mae : Return "Mean Absolute Error"
            Case CVMeasure.Deviance : Return "Partial-Likelihood Deviance"
            Case CVMeasure.Class : Return "Misclassification Error"
            Case CVMeasure.Auc : Return "AUC"
            Case CVMeasure.CIndex : Return "Harrell C-index"
            Case Else : Return measure.ToString()
        End Select
    End Function

End Module

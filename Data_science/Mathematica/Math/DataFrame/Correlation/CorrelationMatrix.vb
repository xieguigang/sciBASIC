Imports Microsoft.VisualBasic.ComponentModel.Collection

Public Class CorrelationMatrix : Inherits DataMatrix

    ReadOnly pvalueMat As Double()()

    Public Property pvalue(a$, b$) As Double
        Get
            Return pvalue(names(a), names(b))
        End Get
        Set
            pvalue(names(a), names(b)) = Value
        End Set
    End Property

    Public Overridable Property pvalue(i%, j%) As Double
        Get
            Return pvalueMat(j)(i)
        End Get
        Set(value As Double)
            pvalueMat(j)(i) = value
        End Set
    End Property

    Sub New(names As IEnumerable(Of String))
        Call MyBase.New(names)
    End Sub

    Sub New(names As Index(Of String), matrix As Double()(), pvalue As Double()())
        Call MyBase.New(names, matrix)

        Me.pvalueMat = pvalue
    End Sub

End Class

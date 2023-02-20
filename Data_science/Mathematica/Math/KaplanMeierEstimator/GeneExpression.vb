Imports System

Namespace KaplanMeierEstimator.Common
    ''' <summary>
    '''  Holds the gene expression before and after a procedure, referring to a specific patient
    ''' </summary>
    Public Class GeneExpression
        Public Property GeneId As String

        Public Property PatientId As Integer

        Public Property Before As Double = Double.NaN

        Public Property After As Double = Double.NaN

        Public ReadOnly Property AbsoluteDifference As Double
            Get
                If Double.IsNaN(Before) OrElse Double.IsNaN(After) Then
                    Return Double.NaN
                End If

                Return Math.Abs(Before - After)
            End Get
        End Property
    End Class
End Namespace

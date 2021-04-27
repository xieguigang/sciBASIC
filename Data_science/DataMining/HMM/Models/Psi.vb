Namespace Models

    Public Class Psi

        Public Property psi As Integer
        Public Property index As Integer

    End Class

    Public Class TrellisPsi

        Public Property trellisSequence As Double()()
        Public Property psiArrays As PsiArray

    End Class

    Public Class termViterbi
        Public Property maximizedProbability As Double
        Public Property psiArrays As PsiArray
    End Class
End Namespace
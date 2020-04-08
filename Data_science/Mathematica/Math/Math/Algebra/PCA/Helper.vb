Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace LinearAlgebra.Prcomp

    <HideModuleName>
    Public Module Helper

        <Extension>
        Public Function Summary(pca As PCA) As String
            Dim table As New List(Of String())

            ' > summary(pca)
            ' Importance of components
            '                           Comp.1     Comp.2     Comp.3     Comp.4
            ' Standard deviation     1.8817805 0.55980636 0.28179594 0.25711844
            ' Proportion of Variance 0.8852745 0.07834579 0.01985224 0.01652747
            ' Cumulative Proportion  0.8852745 0.96362029 0.98347253 1.00000000

            table += {""}.JoinIterates(pca.CumulativeVariance.Sequence(1).Select(Function(i) $"Comp.{i}")).ToArray
            table += {"Standard deviation"}.JoinIterates(pca.StandardDeviations.AsCharacter(True)).ToArray
            table += {"Proportion of Variance"}.JoinIterates(pca.ExplainedVariance.AsCharacter(True)).ToArray
            table += {"Cumulative Proportion"}.JoinIterates(pca.CumulativeVariance.AsCharacter(True)).ToArray

            Dim sb$ = "Importance of components:" & vbCrLf & table.Print(addBorder:=False)

            table *= 0
            table += {""}.JoinIterates(pca _
                              .CumulativeVariance _
                              .Sequence(offSet:=1) _
                              .Select(Function(i) $"Comp.{i}")) _
                         .ToArray

            For Each factor In pca.Loadings.Array.SeqIterator(offset:=1)
                table += {"X" & factor.i} _
                    .JoinIterates(factor.value.AsCharacter(True)) _
                    .ToArray
            Next

            sb = sb & vbCrLf
            sb = sb & "Loading: " & vbCrLf & table.Print(addBorder:=False)

            Return sb
        End Function
    End Module
End Namespace
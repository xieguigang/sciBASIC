Namespace Math.Correlations

    Public Module Scripting

        Public Function GetComputeAPI(name$) As ICorrelation
            Select Case name.ToLower
                Case "pearson"
                    Return AddressOf Correlations.GetPearson
                Case "spearman"
                    Return AddressOf Correlations.Spearman
                Case Else
                    Throw New NotImplementedException($"Method `{name}` is not implemented or support yet!")
            End Select
        End Function
    End Module
End Namespace
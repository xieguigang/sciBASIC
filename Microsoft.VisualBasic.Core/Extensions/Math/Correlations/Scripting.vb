#Region "Microsoft.VisualBasic::529ef937f9a73e22edebef9c23f1bcd8, Microsoft.VisualBasic.Core\Extensions\Math\Correlations\Scripting.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Module Scripting
    ' 
    '         Function: GetComputeAPI
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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

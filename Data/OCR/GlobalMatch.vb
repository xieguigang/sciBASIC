#Region "Microsoft.VisualBasic::fea48b0200e4934c1ed5e96010a98c43, Data\OCR\GlobalMatch.vb"

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

    ' Module GlobalMatch
    ' 
    '     Function: Similarity
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Text.Levenshtein
Imports Microsoft.VisualBasic.GenericLambda(Of Double)

Public Module GlobalMatch

    Public Function Similarity(a As Vector, b As Vector, threshold#) As Double
        Dim equals As IEquals = Function(x, y)
                                    x = threshold <= x
                                    y = threshold <= y
                                    Return x = y
                                End Function
        Dim dist = LevenshteinDistance.ComputeDistance(
            a.ToArray,
            b.ToArray,
            equals,
            Function(x) If(x <= threshold, "0"c, "1"c)
        )

        If dist Is Nothing Then
            Return 0
        Else
            Return dist.MatchSimilarity
        End If
    End Function
End Module

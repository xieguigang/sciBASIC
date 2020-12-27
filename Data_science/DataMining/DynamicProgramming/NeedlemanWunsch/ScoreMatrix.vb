Imports Microsoft.VisualBasic.ComponentModel.Algorithm.DynamicProgramming
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace NeedlemanWunsch

    Public Class ScoreMatrix(Of T)

        ''' <summary>
        ''' get gap open penalty </summary>
        ''' <returns> gap open penalty </returns>
        Public Property GapPenalty As Integer = 1

        ''' <summary>
        ''' get match score </summary>
        ''' <returns> match score </returns>
        Public Property MatchScore As Integer = 1

        ''' <summary>
        ''' get mismatch score </summary>
        ''' <returns> mismatch score </returns>
        Public Property MismatchScore As Integer = -1

        Friend ReadOnly __equals As IEquals(Of T)

        Sub New(match As IEquals(Of T))
            __equals = match
        End Sub

        ''' <summary>
        ''' if char a is equal to char b
        ''' return the match score
        ''' else return mismatch score
        ''' </summary>
        Public Overridable Function getMatchScore(a As T, b As T) As Integer
            If __equals(a, b) Then
                Return MatchScore
            Else
                Return MismatchScore
            End If
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

    End Class
End Namespace
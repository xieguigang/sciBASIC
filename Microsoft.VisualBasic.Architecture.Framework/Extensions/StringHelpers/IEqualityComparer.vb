Imports Microsoft.VisualBasic.Text.Levenshtein

''' <summary>
''' thresholds:
''' 
''' + 0: text equals
''' + 1: binary equals
''' + (0, 1): similarity threshold
''' </summary>
Public Class StringEqualityHelper : Implements IEqualityComparer(Of String)

    ''' <summary>
    ''' + 0: text equals
    ''' + 1: binary equals
    ''' + (0, 1): similarity threshold
    ''' </summary>
    Dim threshold#
    Dim compare As IEquals(Of String)

    ''' <summary>
    ''' thresholds:
    ''' 
    ''' + 0: text equals
    ''' + 1: binary equals
    ''' + (0, 1): similarity threshold
    ''' </summary>
    Sub New(threshold#)
        Me.threshold = threshold

        If threshold = 0R Then
            compare = Function(s1, s2) s1.TextEquals(s2)
        ElseIf threshold = 1 Then
            compare = Function(s1, s2) s1 = s2
        Else
            compare = Function(s1, s2)
                          With LevenshteinDistance.ComputeDistance(s1, s2)
                              Return?.MatchSimilarity >= threshold
                          End With
                      End Function
        End If
    End Sub

    Public Shared ReadOnly Property TextEquals As New StringEqualityHelper(0)
    Public Shared ReadOnly Property BinaryEquals As New StringEqualityHelper(1)

    Public Overrides Function ToString() As String
        If threshold = 0R Then
            Return "Text Equals Of the Two String"
        ElseIf threshold = 1.0R Then
            Return "Binary Equals Of the Two String"
        Else
            Return "String Similarity With threshold " & threshold
        End If
    End Function

    Public Overloads Function Equals(x As String, y As String) As Boolean Implements IEqualityComparer(Of String).Equals
        Return compare(x, y)
    End Function

    Public Overloads Function GetHashCode(obj As String) As Integer Implements IEqualityComparer(Of String).GetHashCode
        If obj Is Nothing Then
            Return 0
        End If
        Return obj.GetHashCode
    End Function
End Class

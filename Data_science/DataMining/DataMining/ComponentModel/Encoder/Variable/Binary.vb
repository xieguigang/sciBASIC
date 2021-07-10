Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace ComponentModel.Encoder.Variable

    Public Class Binary : Inherits EntityBase(Of Boolean)
        Implements INamedValue

        Public Property id As String Implements INamedValue.Key

        ''' <summary>
        ''' Contingency table of two binary variable:
        ''' 
        ''' ```
        '''        _____Y___________
        '''    ___|__1__|__0__|_sum_|
        '''   |1  | a   | b   | a+b |
        ''' X |0  | c   | d   | c+d |
        '''   |sum| a+c | b+d | p   |
        ''' ```
        ''' </summary>
        ''' <param name="X"></param>
        ''' <param name="Y"></param>
        ''' <returns></returns>
        Public Shared Function ContingencyDistance(X As Binary, Y As Binary) As Double
            Dim a, b, c, d As Integer
            Dim nsize As Integer = X.Length

            For i As Integer = 0 To nsize - 1
                If X(i) AndAlso Y(i) Then
                    ' 11
                    a += 1
                ElseIf X(i) AndAlso Not Y(i) Then
                    ' 10
                    b += 1
                ElseIf (Not X(i)) AndAlso Y(i) Then
                    ' 01
                    c += 1
                Else
                    ' 00
                    d += 1
                End If
            Next

            Return (b + c) / (a + b + c)
        End Function
    End Class
End Namespace
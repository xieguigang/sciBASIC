Imports Microsoft.VisualBasic.Language.Java

Namespace LP


    Public Class Objective

        Public Sub New(ByVal ___tableau As Tableau)
            Me.Tableau = ___tableau
        End Sub

        Public Overridable ReadOnly Property Solution As Double()
            Get
                Dim columnCount As Integer = Tableau.Matrix(0).Length
                Dim ___solution As Double() = New Double(columnCount - 1) {}
                Dim rhs As Double() = Tableau.getColumn(columnCount - 1)
                Arrays.Fill(___solution, 0)
                Dim basicVariableIndices As Integer() = Tableau.BasicVariables
                For i As Integer = 0 To basicVariableIndices.Length - 1
                    ___solution(basicVariableIndices(i)) = rhs(i)
                Next i
                Return ___solution
            End Get
        End Property

        Public Overridable ReadOnly Property Tableau As Tableau

    End Class

End Namespace
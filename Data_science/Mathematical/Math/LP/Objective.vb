Imports Microsoft.VisualBasic.Language.Java
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace LP

    Public Class Objective

        Public Overridable ReadOnly Property Tableau As Tableau

        Public Sub New(___tableau As Tableau)
            Me.Tableau = ___tableau
        End Sub

        Public Overridable ReadOnly Property Solution As Double()
            Get
                Dim columnCount As Integer = Tableau.Matrix(0).Length
                Dim ___solution As Double() = New Double(columnCount - 1) {}
                Dim rhs As Double() = Tableau.getColumn(columnCount - 1)
                Dim basicVariableIndices As Integer() = Tableau.BasicVariables

                For i As Integer = 0 To basicVariableIndices.Length - 1
                    ___solution(basicVariableIndices(i)) = rhs(i)
                Next

                Return ___solution
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Solution.GetJson
        End Function
    End Class
End Namespace
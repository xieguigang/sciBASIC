#Region "Microsoft.VisualBasic::458d2e34acd39b90407042d5f97353f4, ..\visualbasic_App\Data_science\Mathematical\Math\LP\Objective.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

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

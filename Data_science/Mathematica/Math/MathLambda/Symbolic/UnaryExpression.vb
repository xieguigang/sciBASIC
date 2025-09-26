#Region "Microsoft.VisualBasic::f381983867dd24bed0334ff80ccce79d, Data_science\Mathematica\Math\MathLambda\Symbolic\UnaryExpression.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 30
    '    Code Lines: 23 (76.67%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 7 (23.33%)
    '     File Size: 1002 B


    '     Class UnaryExpression
    ' 
    '         Properties: [operator], value
    ' 
    '         Function: Evaluate, GetVariableSymbols, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Scripting.MathExpression
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl

Namespace Symbolic

    Public Class UnaryExpression : Inherits Expression

        Public Property [operator] As String
        Public Property value As Expression

        Public Overrides Function Evaluate(env As ExpressionEngine) As Double
            Dim value As Double = Me.value.Evaluate(env)

            Select Case [operator]
                Case "+" : Return value
                Case "-" : Return -value
                Case Else
                    Throw New NotImplementedException([operator])
            End Select
        End Function

        Public Overrides Function ToString() As String
            Return $"{[operator]}{value}"
        End Function

        Public Overrides Function GetVariableSymbols() As IEnumerable(Of String)
            Return value.GetVariableSymbols
        End Function
    End Class
End Namespace

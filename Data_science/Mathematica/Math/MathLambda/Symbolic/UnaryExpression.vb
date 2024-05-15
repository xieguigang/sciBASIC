#Region "Microsoft.VisualBasic::559bd4389a172c80f4756d87dbe2fdc4, Data_science\Mathematica\Math\MathLambda\Symbolic\UnaryExpression.vb"

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

    '   Total Lines: 26
    '    Code Lines: 20
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 851 B


    '     Class UnaryExpression
    ' 
    '         Properties: [operator], value
    ' 
    '         Function: Evaluate, ToString
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
    End Class
End Namespace

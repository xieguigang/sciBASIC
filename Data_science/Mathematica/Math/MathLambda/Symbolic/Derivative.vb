#Region "Microsoft.VisualBasic::21f40e382b0e7bd8d26cb98909213173, Data_science\Mathematica\Math\MathLambda\Symbolic\Derivative.vb"

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

    '   Total Lines: 21
    '    Code Lines: 15
    ' Comment Lines: 2
    '   Blank Lines: 4
    '     File Size: 602 B


    '     Module Derivative
    ' 
    '         Function: GetDerivative
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl

Namespace Symbolic

    Module Derivative

        <Extension>
        Public Function GetDerivative(exp As Expression) As Expression
            If TypeOf exp Is Literal Then
                ' num ' = 0 
                Return Literal.Zero
            ElseIf TypeOf exp Is UnifySymbol Then

            ElseIf TypeOf exp Is SymbolExpression Then
                ' x ' = 1
                Return Literal.One
            End If
        End Function
    End Module
End Namespace

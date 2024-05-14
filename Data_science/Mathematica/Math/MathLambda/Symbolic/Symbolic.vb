#Region "Microsoft.VisualBasic::53aa95c748cee7c9024f9682d7a2d0bd, Data_science\Mathematica\Math\MathLambda\Symbolic\Symbolic.vb"

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

    '   Total Lines: 42
    '    Code Lines: 32
    ' Comment Lines: 3
    '   Blank Lines: 7
    '     File Size: 1.44 KB


    '     Module Symbolic
    ' 
    '         Function: (+2 Overloads) Derivative, (+2 Overloads) Simplify
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl
Imports Script = Microsoft.VisualBasic.Math.Scripting.ScriptEngine

Namespace Symbolic

    ''' <summary>
    ''' symbolic computation engine
    ''' </summary>
    Public Module Symbolic

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Simplify(expr As String) As Expression
            Return Simplify(Script.ParseExpression(expr))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Simplify(raw As Expression) As Expression
            If TypeOf raw Is UnifySymbol Then
                Return DirectCast(raw, UnifySymbol).GetSimplify
            End If

            If Not TypeOf raw Is BinaryExpression Then
                Return raw
            Else
                Return MakeSimplify.makeSimple(raw)
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Derivative(expr As String) As Expression
            Return Derivative(Script.ParseExpression(expr))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Derivative(exp As Expression) As Expression
            Return exp.GetDerivative
        End Function
    End Module
End Namespace

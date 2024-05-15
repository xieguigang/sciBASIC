#Region "Microsoft.VisualBasic::abe3cac3961783196c91ac2613042994, Data_science\Mathematica\Math\Math\Scripting\Expression\Expression\Factorial.vb"

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
    '    Code Lines: 18
    ' Comment Lines: 3
    '   Blank Lines: 5
    '     File Size: 691 B


    '     Class Factorial
    ' 
    '         Properties: factor
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Scripting.MathExpression.Impl

    ''' <summary>
    ''' x!
    ''' </summary>
    Public Class Factorial : Inherits Expression

        Public ReadOnly Property factor As Integer

        Sub New(factor As String)
            Me.factor = Val(factor)
        End Sub

        Public Overrides Function Evaluate(env As ExpressionEngine) As Double
            Return VBMath.Factorial(factor)
        End Function

        Public Overrides Function ToString() As String
            If factor < 0 Then
                Return $"({factor})!"
            Else
                Return $"{factor}!"
            End If
        End Function
    End Class
End Namespace

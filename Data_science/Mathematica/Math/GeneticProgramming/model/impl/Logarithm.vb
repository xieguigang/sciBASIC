#Region "Microsoft.VisualBasic::f9c5f1dacf1b1d290eb98a03a75227ce, Data_science\Mathematica\Math\GeneticProgramming\model\impl\Logarithm.vb"

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

    '   Total Lines: 25
    '    Code Lines: 17
    ' Comment Lines: 0
    '   Blank Lines: 8
    '     File Size: 649 B


    '     Class Logarithm
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: eval, ToString, toStringExpression
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace model.impl

    Public Class Logarithm : Inherits AbstractUnaryExpression

        Public Sub New(child As Expression)
            MyBase.New(child)
        End Sub

        Public Overrides Function eval(x As Double) As Double
            Return std.Log(m_child.eval(x))
        End Function

        Public Overrides Function toStringExpression() As String
            Return String.Format("log({0})", m_child.toStringExpression())
        End Function

        Public Overrides Function ToString() As String
            Return "log()"
        End Function

    End Class

End Namespace

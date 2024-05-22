#Region "Microsoft.VisualBasic::d11422c25f7d10027436ef4da44bc84f, Data_science\Mathematica\Math\GeneticProgramming\model\impl\literal\PI.vb"

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

    '   Total Lines: 23
    '    Code Lines: 15 (65.22%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (34.78%)
    '     File Size: 478 B


    '     Class PI
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: duplicate, toStringExpression
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace model.impl

    Public Class PI : Inherits Number

        Public Shared ReadOnly Pi As PI = New PI()

        Public Sub New()
            MyBase.New(std.PI)
        End Sub

        Public Overrides Function duplicate() As Expression
            Return Pi
        End Function

        Public Overrides Function toStringExpression() As String
            Return "π"
        End Function

    End Class

End Namespace

﻿#Region "Microsoft.VisualBasic::210f89a5be50c245fb3b176191851d71, G:/GCModeller/src/runtime/sciBASIC#/Data_science/Mathematica/Math/GeneticProgramming//model/impl/Plus.vb"

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
    '    Code Lines: 16
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 734 B


    '     Class Plus
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: eval, ToString, toStringExpression
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace model.impl

    Public Class Plus : Inherits AbstractBinaryExpression

        Public Sub New(leftChild As Expression, rightChild As Expression)
            MyBase.New(leftChild, rightChild)
        End Sub

        Public Overrides Function eval(x As Double) As Double
            Return leftChildField.eval(x) + rightChildField.eval(x)
        End Function

        Public Overrides Function toStringExpression() As String
            Return String.Format("({0} + {1})", leftChildField.toStringExpression(), rightChildField.toStringExpression())
        End Function

        Public Overrides Function ToString() As String
            Return "L+R"
        End Function

    End Class

End Namespace


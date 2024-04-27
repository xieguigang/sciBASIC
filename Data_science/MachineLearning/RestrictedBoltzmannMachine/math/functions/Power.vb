﻿#Region "Microsoft.VisualBasic::1eb6891d5e96b38be051e00f46ae6d15, G:/GCModeller/src/runtime/sciBASIC#/Data_science/MachineLearning/RestrictedBoltzmannMachine//math/functions/Power.vb"

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

    '   Total Lines: 20
    '    Code Lines: 12
    ' Comment Lines: 3
    '   Blank Lines: 5
    '     File Size: 462 B


    '     Class Power
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: apply
    ' 
    ' 
    ' /********************************************************************************/

#End Region


Namespace math.functions
    ''' <summary>
    ''' Created by kenny on 5/24/14.
    ''' </summary>
    Public Class Power
        Inherits DoubleFunction

        Private ReadOnly power As Double

        Public Sub New(power As Double)
            Me.power = power
        End Sub

        Public Overrides Function apply(v As Double) As Double
            Return System.Math.Pow(v, power)
        End Function
    End Class

End Namespace


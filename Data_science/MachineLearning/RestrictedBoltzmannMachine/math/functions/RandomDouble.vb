#Region "Microsoft.VisualBasic::67d8aab58485e7e4b83d5941e084cce0, Data_science\MachineLearning\RestrictedBoltzmannMachine\math\functions\RandomDouble.vb"

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

    '   Total Lines: 27
    '    Code Lines: 16 (59.26%)
    ' Comment Lines: 3 (11.11%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (29.63%)
    '     File Size: 608 B


    '     Class RandomDouble
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: apply
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace math.functions

    ''' <summary>
    ''' Created by kenny on 5/24/14.
    ''' </summary>
    Public Class RandomDouble
        Inherits DoubleFunction

        Private ReadOnly scalar As Double

        Public Sub New()
            Me.New(1.0)
        End Sub

        Public Sub New(scalar As Double)
            Me.scalar = scalar
        End Sub

        Public Overrides Function apply(v As Double) As Double
            Return randf.NextDouble() * scalar
        End Function

    End Class

End Namespace

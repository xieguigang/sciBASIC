#Region "Microsoft.VisualBasic::1ff1ffbb7a653c3f750c51318f2567a8, Data_science\MachineLearning\RestrictedBoltzmannMachine\math\functions\Round.vb"

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
    '    Code Lines: 18 (66.67%)
    ' Comment Lines: 3 (11.11%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (22.22%)
    '     File Size: 713 B


    '     Class Round
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: apply, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace math.functions

    ''' <summary>
    ''' Created by kenny on 5/24/14.
    ''' </summary>
    Public Class Round
        Inherits DoubleFunction
        Private ReadOnly threshold As Double

        Public Sub New()
            Me.New(0.80)
        End Sub

        Public Sub New(threshold As Double)
            Me.threshold = threshold
        End Sub

        Public Overrides Function apply(x As Double) As Double
            Return If(x >= threshold, 1.0, 0.0)
        End Function

        Public Overrides Function ToString() As String
            Return "Round{" & "threshold=" & threshold.ToString() & "}"c.ToString()
        End Function
    End Class

End Namespace

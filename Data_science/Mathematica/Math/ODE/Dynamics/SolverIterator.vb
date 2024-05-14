#Region "Microsoft.VisualBasic::16d983e6d84a03a54aa85b39c18123c2, Data_science\Mathematica\Math\ODE\Dynamics\SolverIterator.vb"

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

    '   Total Lines: 48
    '    Code Lines: 34
    ' Comment Lines: 3
    '   Blank Lines: 11
    '     File Size: 1.29 KB


    '     Class SolverIterator
    ' 
    '         Properties: RK4Solver
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Bind, Config, ToString
    ' 
    '         Sub: Tick
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Dynamics

    Public Class SolverIterator

        ReadOnly rk4 As RungeKutta4

        Dim solverEnumerator As IEnumerator(Of Integer)
        Dim triggers As New List(Of Action)

        Public ReadOnly Property RK4Solver As RungeKutta4
            Get
                Return rk4
            End Get
        End Property

        Sub New(rk4 As RungeKutta4)
            Me.rk4 = rk4
        End Sub

        Public Function Config(y0 As Double(), n As Integer, a As Double, b As Double) As SolverIterator
            solverEnumerator = rk4 _
                .solverIteration(y0, n, a, b) _
                .GetEnumerator

            Return Me
        End Function

        Public Function Bind(trigger As Action) As SolverIterator
            triggers.Add(trigger)
            Return Me
        End Function

        ''' <summary>
        ''' 这个方法接口主要是应用于模拟器计算
        ''' </summary>
        Public Sub Tick()
            Call solverEnumerator.MoveNext()

            For Each action In triggers
                Call action()
            Next
        End Sub

        Public Overrides Function ToString() As String
            Return rk4.ToString
        End Function
    End Class
End Namespace

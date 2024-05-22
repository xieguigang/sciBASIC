#Region "Microsoft.VisualBasic::7a0a75c381881c0025c597f28a53d652, Data_science\Mathematica\Math\ODE\Dynamics\RungeKutta4.vb"

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

    '   Total Lines: 98
    '    Code Lines: 60 (61.22%)
    ' Comment Lines: 18 (18.37%)
    '    - Xml Docs: 77.78%
    ' 
    '   Blank Lines: 20 (20.41%)
    '     File Size: 3.32 KB


    '     Class RungeKutta4
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Solve, solverIteration, ToString
    ' 
    '         Sub: GetResult, rungeKutta
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Dynamics

    Public Class RungeKutta4

        Dim K1, K2, K3, K4 As Vector
        Dim ODEs As ODEs

        Dim x As List(Of Double)
        Dim y As List(Of Double)()

        Sub New(system As ODEs)
            ODEs = system
        End Sub

        ''' <summary>
        ''' RK4 ODEs solver
        ''' </summary>
        ''' <param name="dxn">The x initial value.(x初值)</param>
        ''' <param name="dyn">The y initial value.(初值y(n))</param>
        ''' <param name="dh">Steps delta.(步长)</param>
        ''' <param name="dynext">
        ''' Returns the y(n+1) result from this parameter.(下一步的值y(n+1))
        ''' </param>
        Private Sub rungeKutta(dxn As Double,
                               ByRef dyn As Vector,
                               dh As Double,
                               ByRef dynext As Vector)

            Call ODEs.ODEs(dxn, dyn, K1)                        ' 求解K1
            Call ODEs.ODEs(dxn + dh / 2, dyn + dh / 2 * K1, K2) ' 求解K2
            Call ODEs.ODEs(dxn + dh / 2, dyn + dh / 2 * K2, K3) ' 求解K3
            Call ODEs.ODEs(dxn + dh, dyn + dh * K3, K4)         ' 求解K4

            ' 求解下一步的值y(n+1)
            dynext = dyn + (K1 + K2 + K3 + K4) * dh / 6.0
        End Sub

        Public Sub GetResult(ByRef x As Double(), ByRef y As List(Of Double)())
            x = Me.x.ToArray
            y = Me.y
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="n">A larger value of this parameter, will makes your result more precise.</param>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        Public Function Solve(y0 As Double(), n As Integer, a As Double, b As Double) As RungeKutta4
            Call solverIteration(y0, n, a, b).ToArray
            Return Me
        End Function

        Friend Iterator Function solverIteration(y0 As Double(), n As Integer, a As Double, b As Double) As IEnumerable(Of Integer)
            ' 步长
            Dim dh As Double = (b - a) / n
            Dim dx As Double = a
            Dim darrayn As New Vector(y0)
            ' 下一步的值,最好初始化
            Dim darraynext As New Vector(y0.Length)
            Dim xi As New List(Of Double)

            K1 = New Vector(y0.Length)
            K2 = New Vector(y0.Length)
            K3 = New Vector(y0.Length)
            K4 = New Vector(y0.Length)

            y = New List(Of Double)(y0.Length - 1) {}

            For i As Integer = 0 To y.Length - 1
                y(i) = New List(Of Double)
            Next

            For i As Integer = 0 To n
                Call rungeKutta(dx, darrayn, dh, darraynext)

                xi += dx
                dx += dh
                darrayn = darraynext

                For index As Integer = 0 To darrayn.Dim - 1
                    y(index).Add(darrayn(index))
                Next

                Yield i
            Next

            x = xi
        End Function

        Public Overrides Function ToString() As String
            Return ODEs.ToString
        End Function
    End Class
End Namespace

#Region "Microsoft.VisualBasic::e72b31e4ccf8e6d883a8d218af6a24db, Data_science\Mathematica\Math\Math\Numerics\Optimization\LBFGSB\examples\Rosenbrock.vb"

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

'   Total Lines: 65
'    Code Lines: 47 (72.31%)
' Comment Lines: 4 (6.15%)
'    - Xml Docs: 0.00%
' 
'   Blank Lines: 14 (21.54%)
'     File Size: 2.01 KB


' Class Rosenbrock
' 
'     Constructor: (+2 Overloads) Sub New
' 
'     Function: evaluate, in_place_gradient
' 
'     Sub: Main15
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Framework.Optimization.LBFGSB
Imports Microsoft.VisualBasic.Serialization.JSON

' ROSENBROCK
' https://www.sfu.ca/~ssurjano/rosen.html
' Global minimum: f(1,1,...,1) = 0;


Public Class Rosenbrock
    Inherits IGradFunction
    Private n As Integer

    Public Sub New(n As Integer)
        Me.n = n
    End Sub

    Public Sub New()
        Me.New(5)
    End Sub

    Public Overrides Function in_place_gradient() As Boolean
        Return True
    End Function

    Public Overrides Function evaluate(x As Double(), grad As Double()) As Double
        Dim fx = (x(0) - 1.0) * (x(0) - 1.0)
        grad(0) = 2.0 * (x(0) - 1.0) + 16.0 * (x(0) * x(0) - x(1)) * x(0)
        For i = 1 To n - 1
            Dim v = x(i) - x(i - 1) * x(i - 1)
            fx += 4.0 * v * v
            If i = n - 1 Then
                grad(i) = 8.0 * v
            Else
                grad(i) = 8.0 * v + 16.0 * (x(i) * x(i) - x(i + 1)) * x(i)
            End If
        Next
        Return fx
    End Function

    Public Shared Sub Main15(args As String())

        ' Debugger.flag = True

        Dim param As Parameters = New Parameters()
        Dim lbfgsb As LBFGSB = New LBFGSB(param)

        ' converges to global minimum
        Try
            Dim res As Double() = lbfgsb.minimize(New Rosenbrock(), New Double() {2, -4, 2, 4, -2}, New Double() {-5, -5, -5, -5, -5}, New Double() {10, 10, 10, 10, 10})
            Call ("!RESULT").debug
            Call ("k = " & lbfgsb.k.ToString()).debug
            Call ("x = " & res.GetJson).debug
            Call ("fx = " & lbfgsb.fx.ToString()).debug
            Call ("grad = " & lbfgsb.m_grad.GetJson).debug
        Catch e As LBFGSBException
            Console.WriteLine(e.ToString())
            Console.Write(e.StackTrace)
        End Try

        Dim f As Rosenbrock = New Rosenbrock()
        Dim g = New Double(4) {}
        Call ("res=" & f.eval(New Double() {2, -4, 2, 4, -2}, g).ToString()).debug

    End Sub

End Class

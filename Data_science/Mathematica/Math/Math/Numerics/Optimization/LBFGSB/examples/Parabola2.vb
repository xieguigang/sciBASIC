#Region "Microsoft.VisualBasic::c2254b2a668292e3943bfffbd377fd29, Data_science\Mathematica\Math\Math\Numerics\Optimization\LBFGSB\examples\Parabola2.vb"

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

'   Total Lines: 37
'    Code Lines: 29 (78.38%)
' Comment Lines: 1 (2.70%)
'    - Xml Docs: 0.00%
' 
'   Blank Lines: 7 (18.92%)
'     File Size: 1.25 KB


' Class Parabola2
' 
'     Function: evaluate
' 
'     Sub: Main13
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Framework.Optimization.LBFGSB
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class Parabola2
    Inherits IGradFunction

    Public Overrides Function evaluate([in] As Double()) As Double
        Dim x = [in](0)
        Dim y = [in](1)
        Dim t = x * x + y * y
        If x < 1.0 OrElse y < 1.0 Then
            Call ("W"c & "outside " & "[" & String.Join(", ", [in]) & "]").debug
        End If
        Return t + Math.Sin(t) * Math.Sin(t)
    End Function

    Public Shared Sub Main13(args As String())

        ' Debugger.flag = True

        Dim param As Parameters = New Parameters()
        Dim lbfgsb As LBFGSB = New LBFGSB(param)

        ' converges to global minimum
        Try
            Dim res As Double() = lbfgsb.minimize(New Parabola2(), New Double() {3, 3}, New Double() {1, 1}, New Double() {5, 5})
            Call ("!RESULT").debug
            Call ("k = " & lbfgsb.k.ToString()).debug
            Call ("x = " & res.GetJson).debug
            Call ("fx = " & lbfgsb.fx.ToString()).debug
            Call ("grad = " & lbfgsb.m_grad.GetJson).debug
        Catch e As LBFGSBException
            Console.WriteLine(e.ToString())
            Console.Write(e.StackTrace)
        End Try
    End Sub

End Class

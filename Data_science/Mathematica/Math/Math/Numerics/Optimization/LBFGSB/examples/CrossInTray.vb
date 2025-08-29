#Region "Microsoft.VisualBasic::65a6cc1a93ecfd310f2b5a67a80923a9, Data_science\Mathematica\Math\Math\Numerics\Optimization\LBFGSB\examples\CrossInTray.vb"

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

'   Total Lines: 39
'    Code Lines: 25 (64.10%)
' Comment Lines: 3 (7.69%)
'    - Xml Docs: 0.00%
' 
'   Blank Lines: 11 (28.21%)
'     File Size: 1.28 KB


' Class CrossInTray
' 
'     Function: evaluate
' 
'     Sub: Main7
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Framework.Optimization.LBFGSB
Imports Microsoft.VisualBasic.Serialization.JSON

' CROSS-IN-TRAY
' https://www.sfu.ca/~ssurjano/crossit.html
' f(+/-1.3491, +/-1.3491) = -2.06261

Public Class CrossInTray
    Inherits IGradFunction

    Public Overrides Function evaluate([in] As Double()) As Double
        Dim x1 = [in](0)
        Dim x2 = [in](1)

        Return -0.0001 * Math.Pow(Math.Abs(Math.Sin(x1) * Math.Sin(x2) * Math.Exp(Math.Abs(100.0 - Math.Sqrt(x1 * x1 + x2 * x2) / Math.PI))) + 1.0, 0.1)
    End Function

    Public Shared Sub Main7(args As String())

        ' Debugger.flag = True

        Dim param As Parameters = New Parameters()
        Dim lbfgsb As LBFGSB = New LBFGSB(param)

        Try
            Dim res As Double() = lbfgsb.minimize(New CrossInTray(), New Double() {3, -3}, New Double() {-10, -10}, New Double() {10, 10})

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

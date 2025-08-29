#Region "Microsoft.VisualBasic::da9717c615126ed9b50c48e97486606f, Data_science\Mathematica\Math\Math\Numerics\Optimization\LBFGSB\examples\Bohachevsky2.vb"

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

'   Total Lines: 38
'    Code Lines: 26 (68.42%)
' Comment Lines: 1 (2.63%)
'    - Xml Docs: 0.00%
' 
'   Blank Lines: 11 (28.95%)
'     File Size: 1.25 KB


' Class Bohachevsky2
' 
'     Function: evaluate
' 
'     Sub: Main3
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Framework.Optimization.LBFGSB
Imports Microsoft.VisualBasic.Serialization.JSON

' https://www.sfu.ca/~ssurjano/boha.html

Public Class Bohachevsky2
    Inherits IGradFunction

    Public Overrides Function evaluate([in] As Double()) As Double
        Dim x1 = [in](0)
        Dim x2 = [in](1)

        Return x1 * x1 + 2.0 * x2 * x2 - 0.3 * Math.Cos(3.0 * Math.PI * x1) * Math.Cos(4.0 * Math.PI * x2) + 0.3
    End Function

    Public Shared Sub Main3(args As String())

        ' Debugger.flag = True

        Dim param As Parameters = New Parameters()
        param.linesearch = LINESEARCH.MORETHUENTE_LBFGSPP
        Dim lbfgsb As LBFGSB = New LBFGSB(param)

        Try
            Dim res As Double() = lbfgsb.minimize(New Bohachevsky2(), New Double() {-50, 90}, New Double() {-100, -100}, New Double() {100, 100})

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

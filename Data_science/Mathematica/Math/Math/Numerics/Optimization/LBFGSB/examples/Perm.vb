#Region "Microsoft.VisualBasic::be12023f318ad0f7bc5c11f767f50b6b, Data_science\Mathematica\Math\Math\Numerics\Optimization\LBFGSB\examples\Perm.vb"

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

    '   Total Lines: 72
    '    Code Lines: 56 (77.78%)
    ' Comment Lines: 2 (2.78%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 14 (19.44%)
    '     File Size: 2.12 KB


    ' Class Perm
    ' 
    '     Constructor: (+3 Overloads) Sub New
    ' 
    '     Function: evaluate
    ' 
    '     Sub: Main14
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Math.Framework.Optimization.LBFGSB
Imports Microsoft.VisualBasic.Serialization.JSON


Public Class Perm
    Inherits IGradFunction
    Private d As Integer
    Private ij As Double()()
    Private jbeta As Double()

    Public Sub New(beta As Double, d As Integer)
        Me.d = d

        ij = RectangularArray.Matrix(Of Double)(d, d)
        jbeta = New Double(d - 1) {}

        For j = 1 To d
            jbeta(j - 1) = j + beta
            For i = 1 To d
                ij(i - 1)(j - 1) = 1.0 / Math.Pow(j, i)
            Next
        Next
    End Sub

    Public Sub New(d As Integer)
        Me.New(1.0, d)
    End Sub
    Public Sub New()
        Me.New(5)
    End Sub

    Public Overrides Function evaluate([in] As Double()) As Double
        Dim res = 0.0
        For i = 1 To d
            Dim resj = 0.0
            For j = 1 To d
                resj += jbeta(j - 1) * (Math.Pow([in](j - 1), i) - ij(i - 1)(j - 1))
            Next
            res += resj * resj
        Next
        Return res
    End Function

    Public Shared Sub Main14(args As String())

        ' Debugger.flag = True

        Dim param As Parameters = New Parameters()
        Dim lbfgsb As LBFGSB = New LBFGSB(param)

        Dim d = 3

        Dim z = New Double() {0, 0, 0}
        Dim f As Perm = New Perm(d)
        Call ("res = " & f.evaluate(z).ToString()).debug

        ' converges to global minimum
        Try
            Dim res As Double() = lbfgsb.minimize(New Perm(d), New Double() {1, -1, -3}, New Double() {-d, -d, -d}, New Double() {d, d, d})
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

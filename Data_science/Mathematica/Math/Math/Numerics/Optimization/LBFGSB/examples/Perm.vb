#Region "Microsoft.VisualBasic::f5ffaa212a15688e3a71ccc3557e4332, Data_science\Mathematica\Math\Math\Numerics\Optimization\LBFGSB\examples\Perm.vb"

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

    '   Total Lines: 71
    '    Code Lines: 56 (78.87%)
    ' Comment Lines: 1 (1.41%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 14 (19.72%)
    '     File Size: 2.06 KB


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

        Debug.flag = True

        Dim param As Parameters = New Parameters()
        Dim lbfgsb As LBFGSB = New LBFGSB(param)

        Dim d = 3

        Dim z = New Double() {0, 0, 0}
        Dim f As Perm = New Perm(d)
        Call Debug.debug("res = " & f.evaluate(z).ToString())

        ' converges to global minimum
        Try
            Dim res As Double() = lbfgsb.minimize(New Perm(d), New Double() {1, -1, -3}, New Double() {-d, -d, -d}, New Double() {d, d, d})
            Debug.debug("!"c, "RESULT")
            Call Debug.debug("k = " & lbfgsb.k.ToString())
            Debug.debug("x = ", res)
            Call Debug.debug("fx = " & lbfgsb.fx.ToString())
            Debug.debug("grad = ", lbfgsb.m_grad)
        Catch e As LBFGSBException
            Console.WriteLine(e.ToString())
            Console.Write(e.StackTrace)
        End Try
    End Sub

End Class

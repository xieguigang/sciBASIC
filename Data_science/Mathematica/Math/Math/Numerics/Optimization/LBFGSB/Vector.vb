#Region "Microsoft.VisualBasic::aeb9c96aa664b2f9dc2467642b5ebc4b, Data_science\Mathematica\Math\Math\Numerics\Optimization\LBFGSB\Vector.vb"

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

    '   Total Lines: 62
    '    Code Lines: 51 (82.26%)
    ' Comment Lines: 1 (1.61%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (16.13%)
    '     File Size: 1.80 KB


    '     Class Vector
    ' 
    '         Function: dot, norm, resize, squaredNorm
    ' 
    '         Sub: [sub], normalize, setAll
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace Framework.Optimization.LBFGSB

    Public NotInheritable Class Vector

        ' Eigen resize functionality
        Public Shared Function resize(a As Double(), n As Integer) As Double()
            If a Is Nothing Then
                Return New Double(n - 1) {}
            Else
                If a.Length = n Then
                    Return a
                Else
                    Dim res = New Double(n - 1) {}
                    Array.Copy(a, 0, res, 0, n)
                    Return res
                End If
            End If
        End Function

        Public Shared Function dot(a As Double(), b As Double()) As Double
            Dim res = 0.0
            For i = 0 To a.Length - 1
                res += a(i) * b(i)
            Next
            Return res
        End Function

        Public Shared Function squaredNorm(x As Double()) As Double
            Dim res = 0.0
            For i = 0 To x.Length - 1
                res += x(i) * x(i)
            Next
            Return res
        End Function

        Public Shared Function norm(x As Double()) As Double
            Return std.Sqrt(squaredNorm(x))
        End Function

        Public Shared Sub normalize(x As Double())
            Dim n = norm(x)
            For i = 0 To x.Length - 1
                x(i) /= n
            Next
        End Sub

        Public Shared Sub setAll(x As Double(), v As Double)
            For i = 0 To x.Length - 1
                x(i) = v
            Next
        End Sub

        Public Shared Sub [sub](a As Double(), b As Double(), res As Double())
            For i = 0 To res.Length - 1
                res(i) = a(i) - b(i)
            Next
        End Sub
    End Class

End Namespace

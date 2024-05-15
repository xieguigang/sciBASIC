#Region "Microsoft.VisualBasic::85d43bd3c10f5985e02fc8abfdc918aa, Microsoft.VisualBasic.Core\src\Extensions\Math\SIMD\Arithmetic\Exponent.vb"

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

    '   Total Lines: 73
    '    Code Lines: 33
    ' Comment Lines: 26
    '   Blank Lines: 14
    '     File Size: 2.29 KB


    '     Class Exponent
    ' 
    '         Function: f64_exp, f64_op_exponent_f64, f64_op_exponent_f64_scalar, f64_scalar_op_exponent_f64
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace Math.SIMD

    ''' <summary>
    ''' implements the power operator in VB
    ''' </summary>
    Public Class Exponent

        ''' <summary>
        ''' <paramref name="v1"/> ^ <paramref name="v2"/>
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="v2"></param>
        ''' <returns></returns>
        Public Shared Function f64_scalar_op_exponent_f64(v1 As Double, v2 As Double()) As Double()
            Dim result As Double() = New Double(v2.Length - 1) {}

            For i As Integer = 0 To v2.Length - 1
                result(i) = v1 ^ v2(i)
            Next

            Return result
        End Function

        ''' <summary>
        ''' <paramref name="v1"/> ^ <paramref name="v2"/>
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="v2"></param>
        ''' <returns></returns>
        Public Shared Function f64_op_exponent_f64_scalar(v1 As Double(), v2 As Double) As Double()
            Dim result As Double() = New Double(v1.Length - 1) {}

            For i As Integer = 0 To v1.Length - 1
                result(i) = v1(i) ^ v2
            Next

            Return result
        End Function

        ''' <summary>
        ''' <paramref name="v1"/> ^ <paramref name="v2"/>
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="v2"></param>
        ''' <returns></returns>
        Public Shared Function f64_op_exponent_f64(v1 As Double(), v2 As Double()) As Double()
            Dim result As Double() = New Double(v1.Length - 1) {}

            For i As Integer = 0 To v1.Length - 1
                result(i) = v1(i) ^ v2(i)
            Next

            Return result
        End Function

        ''' <summary>
        ''' exp(<paramref name="v"/>)
        ''' </summary>
        ''' <param name="v"></param>
        ''' <returns></returns>
        Public Shared Function f64_exp(v As Double()) As Double()
            Dim result As Double() = New Double(v.Length - 1) {}

            For i As Integer = 0 To v.Length - 1
                result(i) = std.Exp(v(i))
            Next

            Return result
        End Function
    End Class
End Namespace

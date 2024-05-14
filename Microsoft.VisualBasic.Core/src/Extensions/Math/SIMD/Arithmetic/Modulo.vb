#Region "Microsoft.VisualBasic::b010dc5e87822ef1f9906912620d14e8, Microsoft.VisualBasic.Core\src\Extensions\Math\SIMD\Arithmetic\Modulo.vb"

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

    '   Total Lines: 42
    '    Code Lines: 30
    ' Comment Lines: 0
    '   Blank Lines: 12
    '     File Size: 1.20 KB


    '     Class Modulo
    ' 
    '         Function: f64_op_modulo_f64, f64_op_modulo_f64_scalar, f64_scalar_op_modulo_f64
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Numerics

#If Not NET48 Then
Imports System.Runtime.Intrinsics
Imports System.Runtime.Intrinsics.X86
#End If

Namespace Math.SIMD

    Public Class Modulo

        Public Shared Function f64_scalar_op_modulo_f64(v1 As Double, v2 As Double()) As Double()
            Dim result As Double() = New Double(v2.Length - 1) {}

            For i As Integer = 0 To v2.Length - 1
                result(i) = v1 Mod v2(i)
            Next

            Return result
        End Function

        Public Shared Function f64_op_modulo_f64_scalar(v1 As Double(), v2 As Double) As Double()
            Dim result As Double() = New Double(v1.Length - 1) {}

            For i As Integer = 0 To v1.Length - 1
                result(i) = v1(i) Mod v2
            Next

            Return result
        End Function

        Public Shared Function f64_op_modulo_f64(v1 As Double(), v2 As Double()) As Double()
            Dim result As Double() = New Double(v1.Length - 1) {}

            For i As Integer = 0 To v1.Length - 1
                result(i) = v1(i) Mod v2(i)
            Next

            Return result
        End Function
    End Class
End Namespace

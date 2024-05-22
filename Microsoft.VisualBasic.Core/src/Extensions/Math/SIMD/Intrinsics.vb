#Region "Microsoft.VisualBasic::4e4d46c0d8448c7f8c80a4b9bb876fc6, Microsoft.VisualBasic.Core\src\Extensions\Math\SIMD\Intrinsics.vb"

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

    '   Total Lines: 66
    '    Code Lines: 55 (83.33%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (16.67%)
    '     File Size: 2.49 KB


    '     Class SIMDIntrinsics
    ' 
    ' 
    '         Delegate Function
    ' 
    '             Function: VectorAddAvx, VectorAddAvx2
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

#If NETCOREAPP Then
Imports System.Runtime.Intrinsics
Imports System.Runtime.Intrinsics.X86
#End If

Namespace Math.SIMD
#If NETCOREAPP Then

    Public Class SIMDIntrinsics

        Public Delegate Function Math(a As Vector256(Of Double), b As Vector256(Of Double)) As Vector256(Of Double)

        Public Shared Function VectorAddAvx(v1 As Double(), v2 As Double()) As Double()
            Dim a As Vector256(Of Double)
            Dim b As Vector256(Of Double)
            Dim c As Vector256(Of Double)
            Dim size As Integer = v1.Length
            Dim vec As Double() = New Double(size - 1) {}
            Dim remaining As Integer = v1.Length Mod SIMDEnvironment.countDouble
            Dim ends = vec.Length - remaining - 1

            For i As Integer = 0 To ends Step 4
                a = Vector256.Create(v1(i), v1(i + 1), v1(i + 2), v1(i + 3))
                b = Vector256.Create(v2(i), v2(i + 1), v2(i + 2), v2(i + 3))
                c = Avx.Add(a, b)
                vec(i) = c.GetElement(0)
                vec(i + 1) = c.GetElement(1)
                vec(i + 2) = c.GetElement(2)
                vec(i + 3) = c.GetElement(3)
            Next

            For i As Integer = v1.Length - remaining To v1.Length - 1
                vec(i) = v1(i) + v2(i)
            Next

            Return vec
        End Function

        Public Shared Function VectorAddAvx2(v1 As Double(), v2 As Double()) As Double()
            Dim a As Vector256(Of Double)
            Dim b As Vector256(Of Double)
            Dim c As Vector256(Of Double)
            Dim size As Integer = v1.Length
            Dim vec As Double() = New Double(size - 1) {}
            Dim remaining As Integer = v1.Length Mod SIMDEnvironment.countDouble
            Dim ends = vec.Length - remaining - 1

            For i As Integer = 0 To ends Step 4
                a = Vector256.Create(v1(i), v1(i + 1), v1(i + 2), v1(i + 3))
                b = Vector256.Create(v2(i), v2(i + 1), v2(i + 2), v2(i + 3))
                c = Avx2.Add(a, b)
                vec(i) = c.GetElement(0)
                vec(i + 1) = c.GetElement(1)
                vec(i + 2) = c.GetElement(2)
                vec(i + 3) = c.GetElement(3)
            Next

            For i As Integer = v1.Length - remaining To v1.Length - 1
                vec(i) = v1(i) + v2(i)
            Next

            Return vec
        End Function
    End Class
#End If
End Namespace

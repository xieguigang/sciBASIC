#Region "Microsoft.VisualBasic::f741db03ac7914d7e37f4d7cf9100fe8, Microsoft.VisualBasic.Core\src\Extensions\Math\SIMD\Arithmetic\Multiply.vb"

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

    '   Total Lines: 220
    '    Code Lines: 157 (71.36%)
    ' Comment Lines: 28 (12.73%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 35 (15.91%)
    '     File Size: 8.37 KB


    '     Class Multiply
    ' 
    '         Function: f32_op_multiply_f32, f32_scalar_op_multiply_f32, f64_op_multiply_f64, f64_scalar_op_multiply_f64
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

    Public Class Multiply

        ''' <summary>
        ''' <paramref name="v1"/> * <paramref name="v2"/> or 
        ''' <paramref name="v2"/> * <paramref name="v1"/>
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="v2"></param>
        ''' <returns></returns>
        Public Shared Function f32_scalar_op_multiply_f32(v1 As Single, v2 As Single()) As Single()
            Select Case SIMDEnvironment.config
                Case SIMDConfiguration.disable
none:               Dim result As Single() = New Single(v2.Length - 1) {}

                    For i As Integer = 0 To v2.Length - 1
                        result(i) = v1 * v2(i)
                    Next

                    Return result
                Case SIMDConfiguration.enable
                    GoTo legacy
                Case SIMDConfiguration.legacy
legacy:
                    Dim array_v1 As Single() = New Single(SIMDEnvironment.countFloat - 1) {}

                    For i As Integer = 0 To array_v1.Length - 1
                        array_v1(i) = v1
                    Next

                    Dim x1 As Vector(Of Single)
                    Dim x2 As Vector(Of Single) = New Vector(Of Single)(array_v1, Scan0)
                    Dim vec As Single() = New Single(v2.Length - 1) {}
                    Dim remaining As Integer = v2.Length Mod SIMDEnvironment.countFloat
                    Dim ends As Integer = v2.Length - remaining - 1

                    For i As Integer = 0 To ends Step SIMDEnvironment.countFloat
                        x1 = New Vector(Of Single)(v2, i)

                        Call (x1 * x2).CopyTo(vec, i)
                    Next

                    For i As Integer = v2.Length - remaining To v2.Length - 1
                        vec(i) = v2(i) * v1
                    Next

                    Return vec
                Case Else
                    If v2.Length < 10000 Then
                        GoTo none
                    Else
                        GoTo legacy
                    End If
            End Select
        End Function

        ''' <summary>
        ''' <paramref name="v1"/> * <paramref name="v2"/> or 
        ''' <paramref name="v2"/> * <paramref name="v1"/>
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="v2"></param>
        ''' <returns></returns>
        Public Shared Function f64_scalar_op_multiply_f64(v1 As Double, v2 As Double()) As Double()
            Select Case SIMDEnvironment.config
                Case SIMDConfiguration.disable
none:               Dim result As Double() = New Double(v2.Length - 1) {}

                    For i As Integer = 0 To v2.Length - 1
                        result(i) = v1 * v2(i)
                    Next

                    Return result
                Case SIMDConfiguration.enable
                    GoTo legacy
                Case SIMDConfiguration.legacy
legacy:
                    Dim array_v1 As Double() = New Double(SIMDEnvironment.countDouble - 1) {}

                    For i As Integer = 0 To array_v1.Length - 1
                        array_v1(i) = v1
                    Next

                    Dim x1 As Vector(Of Double)
                    Dim x2 As Vector(Of Double) = New Vector(Of Double)(array_v1, Scan0)
                    Dim vec As Double() = New Double(v2.Length - 1) {}
                    Dim remaining As Integer = v2.Length Mod SIMDEnvironment.countDouble
                    Dim ends As Integer = v2.Length - remaining - 1

                    For i As Integer = 0 To ends Step SIMDEnvironment.countDouble
                        x1 = New Vector(Of Double)(v2, i)

                        Call (x1 * x2).CopyTo(vec, i)
                    Next

                    For i As Integer = v2.Length - remaining To v2.Length - 1
                        vec(i) = v2(i) * v1
                    Next

                    Return vec
                Case Else
                    If v2.Length < 10000 Then
                        GoTo none
                    Else
                        GoTo legacy
                    End If
            End Select
        End Function

        ''' <summary>
        ''' <paramref name="v1"/> * <paramref name="v2"/> or 
        ''' <paramref name="v2"/> * <paramref name="v1"/>
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="v2"></param>
        ''' <returns></returns>
        Public Shared Function f32_op_multiply_f32(v1 As Single(), v2 As Single()) As Single()
            Select Case SIMDEnvironment.config
                Case SIMDConfiguration.disable
none:               Dim out As Single() = New Single(v1.Length - 1) {}

                    For i As Integer = 0 To v1.Length - 1
                        out(i) = v1(i) * v2(i)
                    Next

                    Return out
                Case SIMDConfiguration.enable
#If NET48 Then
                    GoTo legacy
#Else
                    GoTo legacy
#End If
                Case SIMDConfiguration.legacy
legacy:             Dim x1 As Vector(Of Single)
                    Dim x2 As Vector(Of Single)
                    Dim vec As Single() = New Single(v1.Length - 1) {}
                    Dim remaining As Integer = v1.Length Mod SIMDEnvironment.countFloat
                    Dim ends As Integer = v1.Length - remaining - 1

                    For i As Integer = 0 To ends Step SIMDEnvironment.countFloat
                        x1 = New Vector(Of Single)(v1, i)
                        x2 = New Vector(Of Single)(v2, i)

                        Call (x1 * x2).CopyTo(vec, i)
                    Next

                    For i As Integer = v1.Length - remaining To v1.Length - 1
                        vec(i) = v1(i) * v2(i)
                    Next

                    Return vec
                Case Else
                    If v1.Length < 10000 Then
                        GoTo none
                    Else
                        GoTo legacy
                    End If
            End Select
        End Function

        ''' <summary>
        ''' <paramref name="v1"/> * <paramref name="v2"/> or 
        ''' <paramref name="v2"/> * <paramref name="v1"/>
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="v2"></param>
        ''' <returns></returns>
        Public Shared Function f64_op_multiply_f64(v1 As Double(), v2 As Double()) As Double()
            Select Case SIMDEnvironment.config
                Case SIMDConfiguration.disable
none:               Dim out As Double() = New Double(v1.Length - 1) {}

                    For i As Integer = 0 To v1.Length - 1
                        out(i) = v1(i) * v2(i)
                    Next

                    Return out
                Case SIMDConfiguration.enable
#If NET48 Then
                    GoTo legacy
#Else
                    GoTo legacy
#End If
                Case SIMDConfiguration.legacy
legacy:             Dim x1 As Vector(Of Double)
                    Dim x2 As Vector(Of Double)
                    Dim vec As Double() = New Double(v1.Length - 1) {}
                    Dim remaining As Integer = v1.Length Mod SIMDEnvironment.countDouble
                    Dim ends As Integer = v1.Length - remaining - 1

                    For i As Integer = 0 To ends Step SIMDEnvironment.countDouble
                        x1 = New Vector(Of Double)(v1, i)
                        x2 = New Vector(Of Double)(v2, i)

                        Call (x1 * x2).CopyTo(vec, i)
                    Next

                    For i As Integer = v1.Length - remaining To v1.Length - 1
                        vec(i) = v1(i) * v2(i)
                    Next

                    Return vec
                Case Else
                    If v1.Length < 10000 Then
                        GoTo none
                    Else
                        GoTo legacy
                    End If
            End Select
        End Function
    End Class
End Namespace

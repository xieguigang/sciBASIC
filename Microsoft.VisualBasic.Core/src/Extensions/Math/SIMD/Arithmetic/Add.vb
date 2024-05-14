#Region "Microsoft.VisualBasic::f18ce48bcaf54c9d47be3ebf7d98ea47, Microsoft.VisualBasic.Core\src\Extensions\Math\SIMD\Arithmetic\Add.vb"

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

    '   Total Lines: 133
    '    Code Lines: 89
    ' Comment Lines: 25
    '   Blank Lines: 19
    '     File Size: 5.19 KB


    '     Class Add
    ' 
    '         Function: f64_op_add_f64, f64_op_add_f64_scalar
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

    Public Class Add

        ''' <summary>
        ''' <paramref name="v1"/> + <paramref name="v2"/> or 
        ''' <paramref name="v2"/> + <paramref name="v1"/>
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="v2"></param>
        ''' <returns></returns>
        Public Shared Function f64_op_add_f64_scalar(v1 As Double(), v2 As Double) As Double()
            Select Case SIMDEnvironment.config
                Case SIMDConfiguration.disable
none:               Dim out As Double() = New Double(v1.Length - 1) {}

                    For i As Integer = 0 To v1.Length - 1
                        out(i) = v1(i) + v2
                    Next

                    Return out
                Case SIMDConfiguration.enable
                    '#If NET48 Then
                    '                    GoTo legacy
                    '#Else
                    '                    If Avx2.IsSupported Then
                    '                        Return SIMDIntrinsics.Vector2(v1, v2, AddressOf Avx2.Add)
                    '                    ElseIf Avx.IsSupported Then
                    '                        Return SIMDIntrinsics.Vector2(v1, v2, AddressOf Avx.Add)
                    '                    Else
                    '                        GoTo legacy
                    '                    End If
                    '#End If
                    GoTo legacy
                Case SIMDConfiguration.legacy
legacy:
                    Dim array_v2 As Double() = New Double(SIMDEnvironment.countDouble - 1) {}

                    For i As Integer = 0 To array_v2.Length - 1
                        array_v2(i) = v2
                    Next

                    Dim x1 As Vector(Of Double)
                    Dim x2 As Vector(Of Double) = New Vector(Of Double)(array_v2, Scan0)
                    Dim vec As Double() = New Double(v1.Length - 1) {}
                    Dim remaining As Integer = v1.Length Mod SIMDEnvironment.countDouble
                    Dim ends As Integer = v1.Length - remaining - 1

                    For i As Integer = 0 To ends Step SIMDEnvironment.countDouble
                        x1 = New Vector(Of Double)(v1, i)
                        ' x2 = New Vector(Of Double)(v2, i)

                        Call (x1 + x2).CopyTo(vec, i)
                    Next

                    For i As Integer = v1.Length - remaining To v1.Length - 1
                        vec(i) = v1(i) + v2
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
        ''' data unchecked add two f64 vector
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="v2"></param>
        ''' <returns></returns>
        Public Shared Function f64_op_add_f64(v1 As Double(), v2 As Double()) As Double()
            Select Case SIMDEnvironment.config
                Case SIMDConfiguration.disable
none:               Dim out As Double() = New Double(v1.Length - 1) {}

                    For i As Integer = 0 To v1.Length - 1
                        out(i) = v1(i) + v2(i)
                    Next

                    Return out
                Case SIMDConfiguration.enable
#If NET48 Then
                    GoTo legacy
#Else
                    If Avx2.IsSupported Then
                        Return SIMDIntrinsics.VectorAddAvx2(v1, v2)
                    ElseIf Avx.IsSupported Then
                        Return SIMDIntrinsics.VectorAddAvx(v1, v2)
                    Else
                        GoTo legacy
                    End If
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

                        Call (x1 + x2).CopyTo(vec, i)
                    Next

                    For i As Integer = v1.Length - remaining To v1.Length - 1
                        vec(i) = v1(i) + v2(i)
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

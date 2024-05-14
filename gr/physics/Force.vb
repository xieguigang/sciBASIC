#Region "Microsoft.VisualBasic::2557f80e8b3002ce71963b0de95f9d72, gr\physics\Force.vb"

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

    '   Total Lines: 109
    '    Code Lines: 68
    ' Comment Lines: 22
    '   Blank Lines: 19
    '     File Size: 3.08 KB


    ' Class Force
    ' 
    '     Properties: angle, source, strength
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: ToString
    ' 
    '     Sub: void
    ' 
    '     Operators: -, (+2 Overloads) *, ^, +, (+2 Overloads) <
    '                (+2 Overloads) <>, (+2 Overloads) =, (+2 Overloads) >
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Math
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math
Imports std = System.Math

''' <summary>
''' 力
''' </summary>
Public Class Force

    ''' <summary>
    ''' 力的大小
    ''' </summary>
    ''' <returns></returns>
    Public Property strength As Double
    ''' <summary>
    ''' 力的方向，与水平的夹角，使用弧度
    ''' </summary>
    ''' <returns></returns>
    Public Property angle As Double
    Public Property source As String

    Sub New()
    End Sub

    Sub New(F#, angle#, <CallerMemberName> Optional trace$ = Nothing)
        Me.strength = F#
        Me.angle = angle
        Me.source = trace
    End Sub

    Public Sub void()
        strength = 0
        angle = 0
    End Sub

    Public Overrides Function ToString() As String
        Dim d$ = angle.ToDegrees.ToString("F2")
        Return $"a={d}, {strength.ToString("F2")}N [{source}]"
    End Function

    Public Shared Operator ^(f As Force, n As Double) As Double
        Return f.strength ^ n
    End Operator

    Public Shared Operator *(x As Double, f As Force) As Double
        Return x * f.strength
    End Operator

    Public Shared Operator *(x As Integer, f As Force) As Double
        Return x * f.strength
    End Operator

    Public Shared Operator =(f As Force, strength#) As Boolean
        Return f.strength = strength
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Operator =(f As Force, strength%) As Boolean
        Return std.Abs(f.strength - strength) <= 0.0001
    End Operator

    Public Shared Operator <>(f As Force, strength#) As Boolean
        Return Not f = strength
    End Operator

    Public Shared Operator <>(f As Force, strength%) As Boolean
        Return Not f = strength
    End Operator

    Public Shared Operator >(strength#, f As Force) As Boolean
        Return strength > f.strength
    End Operator

    Public Shared Operator <(strength#, f As Force) As Boolean
        Return strength < f.strength
    End Operator

    Public Shared Operator >(f1 As Force, f2 As Force) As Boolean
        Return f1.strength > f2.strength
    End Operator

    Public Shared Operator <(f1 As Force, f2 As Force) As Boolean
        Return f1.strength < f2.strength
    End Operator

    ''' <summary>
    ''' 这个力的反向力
    ''' </summary>
    ''' <param name="f"></param>
    ''' <returns></returns>
    Public Shared Operator -(f As Force) As Force
        Return New Force With {
            .strength = f.strength,
            .angle = f.angle + PI,
            .source = $"Reverse({f.source})"
        }
    End Operator

    ''' <summary>
    ''' 使用平行四边形法则进行力的合成
    ''' </summary>
    ''' <param name="f1"></param>
    ''' <param name="f2"></param>
    ''' <returns></returns>
    Public Shared Operator +(f1 As Force, f2 As Force) As Force
        Return Math.ParallelogramLaw(f1, f2)
    End Operator
End Class

#Region "Microsoft.VisualBasic::557390929d4192b66cdc525b6d00a811, ..\sciBASIC#\gr\physics\Force.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Math
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math

''' <summary>
''' 力
''' </summary>
Public Class Force

    ''' <summary>
    ''' 力的大小
    ''' </summary>
    ''' <returns></returns>
    Public Property Strength As Double
    ''' <summary>
    ''' 力的方向，与水平的夹角，使用弧度
    ''' </summary>
    ''' <returns></returns>
    Public Property Angle As Double
    Public Property source As String

    Sub New()
    End Sub

    Sub New(F#, angle#, <CallerMemberName> Optional trace$ = Nothing)
        Strength = F#
        _Angle = angle
        source = trace
    End Sub

    Public Sub void()
        Strength = 0
        Angle = 0
    End Sub

    Public Overrides Function ToString() As String
        Dim d$ = Angle.ToDegrees.ToString("F2")
        Return $"a={d}, {Strength.ToString("F2")}N [{source}]"
    End Function

    Public Shared Operator ^(f As Force, n As Double) As Double
        Return f.Strength ^ n
    End Operator

    Public Shared Operator *(x As Double, f As Force) As Double
        Return x * f.Strength
    End Operator

    Public Shared Operator *(x As Integer, f As Force) As Double
        Return x * f.Strength
    End Operator

    Public Shared Operator =(f As Force, strength#) As Boolean
        Return f.Strength = strength
    End Operator

    Public Shared Operator =(f As Force, strength%) As Boolean
        Return Abs(f.Strength - strength) <= 0.0001
    End Operator

    Public Shared Operator <>(f As Force, strength#) As Boolean
        Return Not f = strength
    End Operator

    Public Shared Operator <>(f As Force, strength%) As Boolean
        Return Not f = strength
    End Operator

    Public Shared Operator >(strength#, f As Force) As Boolean
        Return strength > f.Strength
    End Operator

    Public Shared Operator <(strength#, f As Force) As Boolean
        Return strength < f.Strength
    End Operator

    Public Shared Operator >(f1 As Force, f2 As Force) As Boolean
        Return f1.Strength > f2.Strength
    End Operator

    Public Shared Operator <(f1 As Force, f2 As Force) As Boolean
        Return f1.Strength < f2.Strength
    End Operator

    ''' <summary>
    ''' 这个力的反向力
    ''' </summary>
    ''' <param name="f"></param>
    ''' <returns></returns>
    Public Shared Operator -(f As Force) As Force
        Return New Force With {
            .Strength = f.Strength,
            .Angle = f.Angle + PI,
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


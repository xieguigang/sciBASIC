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
    ' along with this program.  If not, see <http://www.gnu.org/licenses/>.
    
    
    
    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 109
    '    Code Lines: 68 (62.39%)
    ' Comment Lines: 22 (20.18%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 19 (17.43%)
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
    Public Property Strength As Double
    ''' <summary>
    ''' 力的方向，与水平的夹角，使用弧度
    ''' </summary>
    ''' <returns></returns>
    Public Property Angle As Double
    Public Property Source As String

    Sub New()
    End Sub

    Sub New(F#, angle#, <CallerMemberName> Optional trace$ = Nothing)
        Me.Strength = F#
        Me.Angle = angle
        Me.Source = trace
    End Sub

    Public Sub Reset()
        Strength = 0
        Angle = 0
    End Sub

    Public Overrides Function ToString() As String
        Dim d$ = Angle.ToDegrees.ToString("F2")
        Return $"a={d}, {Strength.ToString("F2")}N [{Source}]"
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

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Operator =(f As Force, strength%) As Boolean
        Return std.Abs(f.Strength - strength) <= 0.0001
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
            .Source = $"Reverse({f.Source})"
        }
    End Operator

    ''' <summary>
    ''' 使用平行四边形法则进行力的合成
    ''' </summary>
    ''' <param name="f1"></param>
    ''' <param name="f2"></param>
    ''' <returns></returns>
    Public Shared Operator +(f1 As Force, f2 As Force) As Force
        Return ForceMath.ParallelogramLaw(f1, f2)
    End Operator
End Class

#Region "Microsoft.VisualBasic::759cd975edadb7dbba4b997b6043bdf9, Microsoft.VisualBasic.Core\src\Extensions\Image\Math\Models\Line.vb"

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

    '   Total Lines: 84
    '    Code Lines: 62 (73.81%)
    ' Comment Lines: 8 (9.52%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 14 (16.67%)
    '     File Size: 2.38 KB


    '     Class Line
    ' 
    '         Properties: Length, P1, P2, X1, X2
    '                     Y1, Y2
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices

Namespace Imaging.Math2D

    Public Class Line

        ''' <summary>
        ''' (<see cref="X1"/>, <see cref="Y1"/>)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property P1 As PointF
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New PointF(X1, Y1)
            End Get
        End Property

        ''' <summary>
        ''' (<see cref="X2"/>, <see cref="Y2"/>)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property P2 As PointF
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New PointF(X2, Y2)
            End Get
        End Property

        Public ReadOnly Property Length As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return P1.Distance(P2)
            End Get
        End Property

        Public Sub New(p1 As PointF, p2 As PointF)
            Me.X1 = p1.X
            Me.Y1 = p1.Y
            Me.X2 = p2.X
            Me.Y2 = p2.Y
        End Sub

        Sub New()
        End Sub

        Sub New(x1!, y1!, x2!, y2!)
            Me.X1 = x1
            Me.X2 = x2
            Me.Y1 = y1
            Me.Y2 = y2
        End Sub

#Region "Points"
        Public Property X1 As Single
        Public Property X2 As Single
        Public Property Y1 As Single
        Public Property Y2 As Single
#End Region

        Public Overrides Function ToString() As String
            Return $"[{{{X1}, {Y1}}}, {{{X2}, {Y2}}}]"
        End Function

#If NET_48 = 1 Or netcore5 = 1 Then

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(pointTuple As (X#, Y#)()) As Line
            Return New Line With {
                .X1 = pointTuple(0).X,
                .Y1 = pointTuple(0).Y,
                .X2 = pointTuple(1).X,
                .Y2 = pointTuple(1).Y
            }
        End Operator

#End If

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(twoPoints As PointF()) As Line
            Return New Line(twoPoints(0), twoPoints(1))
        End Operator
    End Class
End Namespace

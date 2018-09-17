#Region "Microsoft.VisualBasic::f532319ba203ff80910141664ccdb0f8, Microsoft.VisualBasic.Core\Extensions\Image\Math\Models\Line.vb"

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

    '     Structure Line
    ' 
    '         Properties: Length, P1, P2, X1, X2
    '                     Y1, Y2
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices

Namespace Imaging.Math2D

    Public Structure Line

        Public Shared ReadOnly Empty As New Line

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

#Region "Points"
        Public Property X1 As Single
        Public Property X2 As Single
        Public Property Y1 As Single
        Public Property Y2 As Single
#End Region

        Public Overrides Function ToString() As String
            Return $"[{{{X1}, {Y1}}}, {{{X2}, {Y2}}}]"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(pointTuple As (X#, Y#)()) As Line
            Return New Line With {
                .X1 = pointTuple(0).X,
                .Y1 = pointTuple(0).Y,
                .X2 = pointTuple(1).X,
                .Y2 = pointTuple(1).Y
            }
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(twoPoints As PointF()) As Line
            Return New Line(twoPoints(0), twoPoints(1))
        End Operator
    End Structure
End Namespace

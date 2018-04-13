#Region "Microsoft.VisualBasic::5962050bbc1c53acb7607ddfed8d879e, Microsoft.VisualBasic.Core\Extensions\Image\Math\Models\Line.vb"

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
    '         Properties: X1, X2, Y1, Y2
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

        Dim P1, P2 As PointF

        Public Sub New(p1 As PointF, p2 As PointF)
            Me.P1 = p1
            Me.P2 = p2
        End Sub

#Region "Points"
        Public Property X1() As Single
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return P1.X
            End Get
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Set
                P1.X = Value
            End Set
        End Property

        Public Property X2() As Single
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return P2.X
            End Get
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Set
                P2.X = Value
            End Set
        End Property

        Public Property Y1() As Single
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return P1.Y
            End Get
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Set
                P1.Y = Value
            End Set
        End Property

        Public Property Y2() As Single
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return P2.Y
            End Get
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Set
                P2.Y = Value
            End Set
        End Property
#End Region

        Public Overrides Function ToString() As String
            Return $"[{{{X1}, {Y1}}}, {{{X2}, {Y2}}}]"
        End Function
    End Structure
End Namespace

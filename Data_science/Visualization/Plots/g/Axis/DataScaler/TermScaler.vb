#Region "Microsoft.VisualBasic::5fcfa1900c543e5ca1382fc40a9e5950, Data_science\Visualization\Plots\g\Axis\DataScaler\TermScaler.vb"

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

    '   Total Lines: 34
    '    Code Lines: 24 (70.59%)
    ' Comment Lines: 4 (11.76%)
    '    - Xml Docs: 75.00%
    ' 
    '   Blank Lines: 6 (17.65%)
    '     File Size: 1.08 KB


    '     Class TermScaler
    ' 
    '         Properties: AxisTicks, X
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Translate, TranslateX
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.d3js.scale
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Graphic.Axis

    Public Class TermScaler : Inherits YScaler

        Public Property X As OrdinalScale
        Public Property AxisTicks As (X As String(), Y As Vector)

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="rev">是否需要将Y坐标轴上下翻转颠倒</param>
        Sub New(Optional rev As Boolean = False)
            Call MyBase.New(reversed:=rev)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Translate(x$, y#) As PointF
            Return New PointF With {
                .x = TranslateX(x),
                .y = TranslateY(y)
            }
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function TranslateX(x As String) As Double
            Return Me.X.Value(x)
        End Function
    End Class
End Namespace

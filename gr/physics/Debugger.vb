#Region "Microsoft.VisualBasic::a4dd9917c269cada70cc5800e2eac201, gr\physics\Debugger.vb"

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

    '   Total Lines: 49
    '    Code Lines: 34 (69.39%)
    ' Comment Lines: 5 (10.20%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (20.41%)
    '     File Size: 1.75 KB


    ' Module Debugger
    ' 
    '     Function: Vector2D
    ' 
    '     Sub: ShowForce
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Module Debugger

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Vector2D(v As Vector) As PointF
        Return New PointF(v(X), v(Y))
    End Function

    <Extension>
    Public Sub ShowForce(m As MassPoint, ByRef canvas As Graphics2D, F As IEnumerable(Of Force), Optional offset As PointF = Nothing)
        Dim font As New Font(FontFace.MicrosoftYaHei, 12, FontStyle.Bold)
        Dim a = m.Point.Vector2D.OffSet2D(offset)

        '#If DEBUG Then
        '        Call $"Sum({F.JoinBy(", ")}) = {F.Sum}".__DEBUG_ECHO
        '#End If

        With canvas

            Call .DrawCircle(a, 10, Brushes.Black)
            ' Call .DrawString(m.ToString, font, Brushes.Black, a)

            Dim draw = Sub(force As Force, color As SolidBrush)
                           Dim v = force.Decomposition2D
                           Dim b = (m.Point + v).Vector2D.OffSet2D(offset)
                           Dim pen As New Pen(color.Color) With {
                               .EndCap = LineCap.ArrowAnchor,
                               .Width = 8
                           }

                           Call .DrawLine(pen, a, b)
                           Call .DrawString(force.ToString, font, color, b)
                       End Sub

            For Each force As Force In F
                Call draw(force, Brushes.SkyBlue)
            Next

            ' 绘制出合力
            Call draw(F.Sum, Brushes.Violet)
        End With
    End Sub
End Module

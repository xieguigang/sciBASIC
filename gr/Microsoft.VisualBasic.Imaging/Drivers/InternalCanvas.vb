#Region "Microsoft.VisualBasic::1fa24466fcf97290572a948f5a68948d, gr\Microsoft.VisualBasic.Imaging\Drivers\InternalCanvas.vb"

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

    '   Total Lines: 66
    '    Code Lines: 45
    ' Comment Lines: 9
    '   Blank Lines: 12
    '     File Size: 2.20 KB


    '     Class InternalCanvas
    ' 
    '         Properties: bg, padding, size
    ' 
    '         Function: InvokePlot
    '         Operators: (+2 Overloads) +, <=, >=
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.Html.CSS

Namespace Driver

    ''' <summary>
    ''' 可以借助这个画布对象创建多图层的绘图操作
    ''' </summary>
    Public Class InternalCanvas

        Dim plots As New List(Of IPlot)

        Public Property size As Size
        Public Property padding As Padding
        Public Property bg As String

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function InvokePlot() As GraphicsData
            Return GraphicsPlots(
                    size, padding, bg,
                    Sub(ByRef g, rect)

                        For Each plot As IPlot In plots
                            Call plot(g, rect)
                        Next
                    End Sub)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator +(g As InternalCanvas, plot As IPlot) As InternalCanvas
            g.plots += plot
            Return g
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator +(g As InternalCanvas, plot As IPlot()) As InternalCanvas
            g.plots += plot
            Return g
        End Operator

        Public Shared Narrowing Operator CType(g As InternalCanvas) As GraphicsData
            Return g.InvokePlot
        End Operator

        ''' <summary>
        ''' canvas invoke this plot.
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="plot"></param>
        ''' <returns></returns>
        Public Shared Operator <=(g As InternalCanvas, plot As IPlot) As GraphicsData
            Dim size As Size = g.size
            Dim margin = g.padding
            Dim bg As String = g.bg

            Return GraphicsPlots(size, margin, bg, plot)
        End Operator

        Public Shared Operator >=(g As InternalCanvas, plot As IPlot) As GraphicsData
            Throw New NotSupportedException
        End Operator
    End Class
End Namespace

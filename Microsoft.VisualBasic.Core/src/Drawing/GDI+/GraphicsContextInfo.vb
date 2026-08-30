#Region "Microsoft.VisualBasic::a9eb51beb082477e755b01367e7a81f6, Microsoft.VisualBasic.Core\src\Drawing\GDI+\GraphicsContextInfo.vb"

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

    '   Total Lines: 39
    '    Code Lines: 10 (25.64%)
    ' Comment Lines: 23 (58.97%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (15.38%)
    '     File Size: 1.47 KB


    '     Class GraphicsContextInfo
    ' 
    '         Properties: Context, Offset
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing

Namespace Imaging

    ''' <summary>
    ''' Describes the cumulative graphics context of an <see cref="IGraphics"/> driver.
    ''' </summary>
    ''' <remarks>
    ''' The gdi+ GetContextInfo overload that takes no arguments is obsolete, and the
    ''' recommended replacements return their results through out arguments instead of
    ''' as an object. This type carries those results together with the driver specific
    ''' context object, so that every driver can describe its context without depending
    ''' on an obsolete api.
    ''' </remarks>
    Public Class GraphicsContextInfo

        ''' <summary>
        ''' Gets or sets the cumulative transform offset of the graphics surface.
        ''' </summary>
        ''' <returns></returns>
        Public Property Offset As PointF

        ''' <summary>
        ''' Gets or sets the driver specific graphics context, e.g. the postscript
        ''' builder or the svg element writer.
        ''' </summary>
        ''' <returns></returns>
        Public Property Context As Object

        ''' <summary>
        ''' Returns a string that represents this graphics context.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return $"[{Offset.X}, {Offset.Y}] {If(Context Is Nothing, "nothing", Context.GetType.FullName)}"
        End Function

    End Class
End Namespace

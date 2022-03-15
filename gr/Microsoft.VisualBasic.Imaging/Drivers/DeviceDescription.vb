#Region "Microsoft.VisualBasic::089537403e121c8c7912f31bc9d8fd3f, sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drivers\DeviceDescription.vb"

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
    '    Code Lines: 26
    ' Comment Lines: 0
    '   Blank Lines: 8
    '     File Size: 963.00 B


    '     Class DeviceDescription
    ' 
    '         Properties: background, bgHtmlColor, dpi, driverUsed, padding
    '                     size
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetRegion, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.MIME.Html.CSS

Namespace Driver

    Public Class DeviceDescription

        Public Property size As Size
        Public Property padding As Padding
        Public Property driverUsed As Drivers
        Public Property dpi As Size

        Public ReadOnly Property background As Color
        Public ReadOnly Property bgHtmlColor As String

        Sub New(bg As String)
            bgHtmlColor = bg
            background = bg.TranslateColor
        End Sub

        Public Overrides Function ToString() As String
            Return $"{bgHtmlColor} [{size.Width},{size.Height}]"
        End Function

        Public Function GetRegion() As GraphicsRegion
            Return New GraphicsRegion With {
                .Size = size,
                .Padding = padding
            }
        End Function

    End Class
End Namespace

#Region "Microsoft.VisualBasic::2c56bbd1072ce11db8b2dc93fc4f9f22, mime\text%html\CSS\Fill.vb"

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

    '   Total Lines: 13
    '    Code Lines: 6 (46.15%)
    ' Comment Lines: 3 (23.08%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (30.77%)
    '     File Size: 263 B


    '     Class Fill
    ' 
    '         Properties: fill
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging

Namespace CSS

    ''' <summary>
    ''' <see cref="Brush"/>: <see cref="SolidBrush"/> and <see cref="TextureBrush"/>
    ''' </summary>
    Public Class Fill

        Public Property fill As String

        Public Overrides Function ToString() As String
            Return fill
        End Function

        Public Function CreateBrush() As Brush
            Return fill.GetBrush
        End Function

    End Class
End Namespace

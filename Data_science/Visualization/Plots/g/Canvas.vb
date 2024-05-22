#Region "Microsoft.VisualBasic::2571aa791eccacb8cb8d2adc6b7f65d3, Data_science\Visualization\Plots\g\Canvas.vb"

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

    '   Total Lines: 26
    '    Code Lines: 17 (65.38%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (34.62%)
    '     File Size: 1.17 KB


    '     Module Resolution2K
    ' 
    ' 
    ' 
    '     Module Resolution1K
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging

Namespace Graphic.Canvas

    Public Module Resolution2K

        Public Const Size$ = "3600,2400"

        Public Const PaddingWithRightLegend$ = "padding:300px 600px 300px 200px;"
        Public Const PaddingWithTopTitleAndRightLegend$ = "padding:400px 600px 300px 200px;"
        Public Const PaddingWithTopTitleAndBottomLegend$ = "padding:400px 150px 600px 200px;"
        Public Const PaddingWithTopTitle$ = "padding:400px 150px 300px 300px;"

        Public Const PaddingWithRightLegendAndBottomTitle$ = "padding:100px 400px 300px 200px;"

        Public Const PlotTitle$ = "font-style: strong; font-size: 64; font-family: " & FontFace.BookmanOldStyle & ";"
        Public Const PlotSubTitle$ = "font-style: normal; font-size: 48; font-family: " & FontFace.BookmanOldStyle & ";"
        Public Const PlotSmallTitle$ = "font-style: normal; font-size: 36; font-family: " & FontFace.BookmanOldStyle & ";"
        Public Const PlotLabelNormal$ = "font-style: normal; font-size: 28; font-family: " & FontFace.BookmanOldStyle & ";"

    End Module

    Public Module Resolution1K

    End Module
End Namespace

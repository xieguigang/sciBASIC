#Region "Microsoft.VisualBasic::712f3c822fb45fead01619e6e20cfc73, sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing2D\Colors\CustomDesigns.vb"

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

    '   Total Lines: 97
    '    Code Lines: 80
    ' Comment Lines: 8
    '   Blank Lines: 9
    '     File Size: 3.36 KB


    '     Class CustomDesigns
    ' 
    '         Function: ClusterColour, Halloween, Paper, Rainbow, TSF
    '                   Unicorn, Vibrant
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq

Namespace Drawing2D.Colors

    Public Class CustomDesigns

        Public Shared Function Halloween() As Color()
            Return Viridis.fromHtml("#1C1C1C", "#F4831B", "#902EBB", "#63C328", "#EEEB27", "#D02823").ToArray
        End Function

        Public Shared Function Unicorn() As Color()
            Return Viridis.fromHtml("#5763CF", "#99FF94", "#FEF77C", "#F7A654", "#EF7779", "#B3498B").ToArray
        End Function

        Public Shared Function Vibrant() As Color()
            Return Viridis.fromHtml("#7734EA", "#00A7EA", "#8AE800", "#FAF100", "#FFAA00", "#FF0061").ToArray
        End Function

        Public Shared Function Paper() As Color()
            Return Viridis.fromHtml(
                "#D02823", "#0491d0", "#88bb64", "#15DBFF",
                "#583B73", "#f2ce3f", "#8858BF", "#CCFF33",
                "#00FF00", "#0000A0", "#41b6ab", "#f0bf59",
                "#79c753", "#c02034", "#097988", "#FF1BFF",
                "#fb5b44", "#361f32", "#DF2789", "#396b1c",
                "#009EFB", "#55CE63", "#F62D51", "#FFBC37",
                "#7460EE", "#52E5DD", "#984ea3", "#ffff00",
                "#000000", "#169c78", "#5F4B8B", "#9c1b31"
            ).ToArray
        End Function

        ''' <summary>
        ''' From TSF launcher on Android
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function TSF() As Color()
            Return {
               {247, 69, 58},
               {230, 28, 99},
               {156, 36, 173},
               {107, 57, 181},
               {66, 81, 181},
               {33, 150, 238},
               {8, 170, 247},
               {0, 190, 214},
               {0, 150, 132},
               {74, 174, 82},
               {132, 194, 74},
               {206, 223, 58},
               {255, 235, 58},
               {255, 190, 0},
               {255, 150, 0},
               {255, 85, 33},
               {115, 85, 66},
               {156, 158, 156},
               {99, 125, 140}
            }.RowIterator _
             .Select(Function(c)
                         Return Color.FromArgb(c(0), c(1), c(2))
                     End Function) _
             .ToArray
        End Function

        Public Shared Function Rainbow() As Color()
            Return New String() {
                "#FF0000FF", "#FF9900FF", "#CCFF00FF", "#33FF00FF", "#00FF66FF",
                "#00FFFFFF", "#0066FFFF", "#3300FFFF", "#CC00FFFF", "#FF0099FF"
            } _
                .Select(Function(c) c.TranslateColor) _
                .ToArray
        End Function

        Public Shared Function FlexImaging() As Color()
            Return New String() {
                "#FF0000FF", "#FF9900FF", "#CCFF00FF", "#33FF00FF", "#00FF66FF",
                "#00FFFFFF", "#0066FFFF", "#3300FFFF", "#CC00FFFF", "#FF0099FF",
                "white"
            } _
                .Select(Function(c) c.TranslateColor) _
                .ToArray
        End Function

        ''' <summary>
        ''' 10 category colors for the data object cluster result
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function ClusterColour() As Color()
            Return {
                Color.FromArgb(128, 200, 180),
                Color.FromArgb(135, 70, 194),
                Color.FromArgb(140, 210, 90),
                Color.FromArgb(200, 80, 147),
                Color.FromArgb(201, 169, 79),
                Color.FromArgb(112, 127, 189),
                Color.FromArgb(192, 82, 58),
                Color.FromArgb(83, 99, 60),
                Color.FromArgb(78, 45, 69),
                Color.FromArgb(202, 161, 169)
            }
        End Function
    End Class
End Namespace

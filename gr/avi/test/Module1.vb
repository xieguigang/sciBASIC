#Region "Microsoft.VisualBasic::ee65924ddfde92acb04bac7168f1d9ac, gr\avi\test\Module1.vb"

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

    '   Total Lines: 22
    '    Code Lines: 16 (72.73%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (27.27%)
    '     File Size: 665 B


    ' Module Module1
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.AVIMedia

Module Module1

    Sub Main()
        Dim frame As Bitmap = "E:\ba3a4a086e061d95dfc331c47bf40ad163d9ca04.jpg".LoadImage
        Dim avi As New Encoder(New Settings With {.width = frame.Width, .height = frame.Height})
        Dim stream As New AVIStream(24, frame.Width, frame.Height)

        For i As Integer = 0 To 120
            Call stream.addFrame(frame)
        Next

        Call avi.streams.Add(stream)
        Call avi.WriteBuffer("X:\avi.js-master\avi.js-master\src\test222222.avi")

        Pause()
    End Sub

End Module

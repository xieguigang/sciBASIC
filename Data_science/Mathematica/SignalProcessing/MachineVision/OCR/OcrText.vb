#Region "Microsoft.VisualBasic::09d33b3cd6c30905838cc1214bf00124, Data_science\Mathematica\SignalProcessing\MachineVision\OCR\OcrText.vb"

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

    '   Total Lines: 17
    '    Code Lines: 5 (29.41%)
    ' Comment Lines: 8 (47.06%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (23.53%)
    '     File Size: 382 B


    ' Class OcrText
    ' 
    '     Properties: polygon, score
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing

Public Class OcrText : Inherits Detection

    ''' <summary>
    ''' the cor text detection confidence score
    ''' </summary>
    ''' <returns></returns>
    Public Property score As Double

    ''' <summary>
    ''' polygon of the text box
    ''' </summary>
    ''' <returns></returns>
    Public Property polygon As PointF()

End Class


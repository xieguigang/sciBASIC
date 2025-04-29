#Region "Microsoft.VisualBasic::82129f150eba3cd237ed3e32f875145b, Data_science\Mathematica\SignalProcessing\MachineVision\OCR\OcrFrameScan.vb"

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

    '   Total Lines: 29
    '    Code Lines: 24 (82.76%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (17.24%)
    '     File Size: 859 B


    ' Class OcrFrameScan
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: Filter, FilterLength
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq

Public Class OcrFrameScan : Inherits FrameData(Of OcrText)

    Sub New()
    End Sub

    Sub New(text As IEnumerable(Of OcrText))
        Detections = text.SafeQuery.ToArray
    End Sub

    Public Function Filter(Optional score_cutoff As Double = 0.3) As OcrFrameScan
        Return New OcrFrameScan With {
            .FrameID = FrameID,
            .Detections = Detections _
                .Where(Function(a) a.score > score_cutoff) _
                .ToArray
        }
    End Function

    Public Function FilterLength(len As Integer) As OcrFrameScan
        Return New OcrFrameScan With {
            .FrameID = FrameID,
            .Detections = Detections _
                .Where(Function(a) a.ObjectID.Length > len) _
                .ToArray
        }
    End Function
End Class


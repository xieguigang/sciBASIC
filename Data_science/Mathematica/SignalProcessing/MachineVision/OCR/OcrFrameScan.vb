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
End Class

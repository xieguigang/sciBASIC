Imports Microsoft.VisualBasic.Linq

Public Class OcrFrameScan : Inherits FrameData(Of OcrText)

    Sub New()
    End Sub

    Sub New(text As IEnumerable(Of OcrText))
        Detections = text.SafeQuery.ToArray
    End Sub
End Class

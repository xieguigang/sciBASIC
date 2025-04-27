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

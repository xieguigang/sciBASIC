Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Module DefaultValueTest

    Sub Main()

        'Dim [new] = [Default](New List(Of String))
        'Dim null As List(Of String) = Nothing

        'Dim x As List(Of String) = null Or [new]

        'Dim notnull As New List(Of String) From {"123"}

        'Dim y = notnull Or [new]

        'println("x:= %s", x.GetJson)
        'println("y:= %s", y.GetJson)

        'Pause()

        Call Draw()  ' using default font
        Call Draw(New Font(FontFace.Cambria, 36, FontStyle.Regular))  ' using user defined font

        Pause()
    End Sub

    Public Function Draw(Optional font As Font = Nothing)
        font = font Or MicrosoftYaHei.Large

        println(font.ToString)
    End Function
End Module

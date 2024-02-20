Imports Microsoft.VisualBasic.ApplicationServices.Terminal.LineEditor

Namespace ApplicationServices.Terminal

    Friend Structure Handler
        Public CKI As ConsoleKeyInfo
        Public KeyHandler As KeyHandler
        Public ResetCompletion As Boolean

        Public Sub New(key As ConsoleKey, h As KeyHandler, Optional resetCompletion As Boolean = True)
            CKI = New ConsoleKeyInfo(Microsoft.VisualBasic.ChrW(0), key, False, False, False)
            KeyHandler = h
            Me.ResetCompletion = resetCompletion
        End Sub

        Public Sub New(c As Char, h As KeyHandler, Optional resetCompletion As Boolean = True)
            KeyHandler = h
            ' Use the "Zoom" as a flag that we only have a character.
            CKI = New ConsoleKeyInfo(c, ConsoleKey.Zoom, False, False, False)
            Me.ResetCompletion = resetCompletion
        End Sub

        Public Sub New(cki As ConsoleKeyInfo, h As KeyHandler, Optional resetCompletion As Boolean = True)
            Me.CKI = cki
            KeyHandler = h
            Me.ResetCompletion = resetCompletion
        End Sub

        Public Shared Function Control(c As Char, h As KeyHandler, Optional resetCompletion As Boolean = True) As Handler
            Return New Handler(Microsoft.VisualBasic.ChrW(AscW(c) - AscW("A"c) + 1), h, resetCompletion)
        End Function

        Public Shared Function Alt(c As Char, k As ConsoleKey, h As KeyHandler) As Handler
            Dim cki As ConsoleKeyInfo = New ConsoleKeyInfo(c, k, False, True, False)
            Return New Handler(cki, h)
        End Function
    End Structure

End Namespace
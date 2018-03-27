Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree

''' <summary>
''' The character library tree
''' </summary>
Public Class Library

    ReadOnly font As Font

    Const Numeric$ = "0123456789.+-"
    Const Symbols$ = "/*-+()=^\<>,:;""'{}[]_&%$#@!~`?"
    Const enUS$ = Numeric & Symbols & "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"

    ReadOnly tree As BinaryTree(Of OpticalCharacter, Char)
    ReadOnly cutoff#

    Sub New(font As Font, Optional characters$ = enUS, Optional cutoff# = 0.98)
        Me.font = font
        Me.cutoff = cutoff
        Me.tree = BuildLibraryTree(font, characters, cutoff)
    End Sub

    Public Overrides Function ToString() As String
        Return $"[{font.ToString}] {tree.PopulateNodes.Values.CharString}"
    End Function

    Public Shared Function BuildLibraryTree(font As Font, characters$, cutoff#) As BinaryTree(Of OpticalCharacter, Char)

    End Function
End Class

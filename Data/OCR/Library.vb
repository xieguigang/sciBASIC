Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Math2D

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

    Public Function Match(OpticalCharacter As OpticalCharacter) As (score#, recognized As Char)

    End Function

    Public Overrides Function ToString() As String
        Return $"[{font.ToString}] {tree.PopulateNodes.Values.CharString}"
    End Function

    Public Shared Function BuildLibraryTree(font As Font, characters$, Optional color$ = "black", Optional cutoff# = 0.98) As BinaryTree(Of OpticalCharacter, Char)
        Dim compares As Comparison(Of OpticalCharacter) =
            Function(a As OpticalCharacter, b As OpticalCharacter) As Integer
                Dim score# = a.Compare(b)

                If score >= cutoff Then
                    Return 0
                ElseIf score >= cutoff * 0.8 Then
                    Return 1
                Else
                    Return -1
                End If
            End Function
        Dim tree As New AVLTree(Of OpticalCharacter, Char)(compares, Function(c) c.char)
        Dim size As Size = font.Height.SquareSize
        Dim fontColor As New SolidBrush(color.TranslateColor)
        Dim ZERO As New Point(0, 0)

        For Each c As Char In characters
            Dim optical As Image

            Using g As Graphics2D = size.CreateGDIDevice
                g.DrawString(c, font, fontColor, ZERO)
                optical = g.ImageResource
            End Using

            Dim [char] As New OpticalCharacter(optical, [char]:=c)

            Call tree.Add([char], value:=c, valueReplace:=False)
        Next

        Return tree.root
    End Function
End Class

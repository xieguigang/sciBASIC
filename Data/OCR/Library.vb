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
    ReadOnly compares As Comparison(Of OpticalCharacter)

    Public ReadOnly Property Window As Size

    Sub New(font As Font, Optional characters$ = enUS, Optional cutoff# = 0.98)
        Me.font = font
        Me.cutoff = cutoff
        Me.tree = BuildLibraryTree(font, characters, cutoff:=cutoff)
        Me.compares = CompareTo(cutoff)
        Me.Window = font.Height.SquareSize
    End Sub

    Public Shared Function CompareTo(cutoff As Double) As Comparison(Of OpticalCharacter)
        Return Function(a As OpticalCharacter, b As OpticalCharacter) As Integer
                   Dim score# = a.Compare(b)

                   If score >= cutoff Then
                       Return 0
                   ElseIf score >= cutoff * 0.8 Then
                       Return 1
                   Else
                       Return -1
                   End If
               End Function
    End Function

    ''' <summary>
    ''' 查找失败的时候，返回来的分数值为-1，小于零
    ''' </summary>
    ''' <param name="OpticalCharacter"></param>
    ''' <returns></returns>
    Public Function Match(OpticalCharacter As OpticalCharacter) As (score#, recognized As Char)
        Dim find = tree.Find(OpticalCharacter, compares)

        If find Is Nothing Then
            Return (-1, Nothing)
        Else
            Return (find.Key.Compare(OpticalCharacter), find.Value)
        End If
    End Function

    Public Overrides Function ToString() As String
        Return $"[{font.ToString}] {tree.PopulateNodes.Values.CharString}"
    End Function

    Public Shared Function BuildLibraryTree(font As Font, characters$, Optional color$ = "black", Optional cutoff# = 0.98) As BinaryTree(Of OpticalCharacter, Char)
        Dim tree As New AVLTree(Of OpticalCharacter, Char)(Library.CompareTo(cutoff), Function(c) c.char)
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

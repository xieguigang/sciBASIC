Imports Microsoft.VisualBasic.Imaging.Drawing2D.Shapes
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Text

Namespace PostScript

    ''' <summary>
    ''' abstract model of the painting elements
    ''' </summary>
    Public MustInherit Class PSElement

        Friend MustOverride Sub WriteAscii(ps As Writer)
        Friend MustOverride Sub Paint(g As IGraphics)

    End Class

    ''' <summary>
    ''' An postscript graphics element
    ''' </summary>
    Public MustInherit Class PSElement(Of S As Shape) : Inherits PSElement

        Public Property shape As S

    End Class

    Public Class PsComment : Inherits PSElement

        Public Property binary As Boolean
        ''' <summary>
        ''' this text data will be base64 data string if is <see cref="binary"/> metadata
        ''' </summary>
        ''' <returns></returns>
        Public Property text As String

        Sub New()
        End Sub

        Sub New(binary As Byte())
            Me.binary = True
            Me.text = binary.ToBase64String
        End Sub

        Sub New(text As String)
            Me.text = text
            Me.binary = False
        End Sub

        Friend Overrides Sub WriteAscii(ps As Writer)
            If binary Then
                Call ps.comment("binary meta data")

                ' split into parts by 160 chars
                For Each part As String In text.Chunks(160)
                    Call ps.comment(part)
                Next

                Call ps.comment("EOF_binarydata")
            Else
                Call ps.note(text)
            End If
        End Sub

        ''' <summary>
        ''' do nothing on comment node
        ''' </summary>
        ''' <param name="g"></param>
        Friend Overrides Sub Paint(g As IGraphics)
        End Sub
    End Class

End Namespace



Public Class FormatedRegion

    Friend RTFDocument As Rtf

    Public Property Font As Font

    Public ReadOnly Property Text As String
        Get
#Const DEBUG = 0

#If DEBUG Then
            Try
#End If
            Return Mid(RTFDocument._InternalTextMetaSrcCache, Start, Right - Start + 1)
#If DEBUG Then
            Catch ex As Exception
                Call Console.WriteLine("[DEBUG] ({0}, {1})", Start, Right)
                Throw
            End Try
#End If
        End Get
    End Property

    Dim _Start As Integer
    Dim _Right As Integer

    Public ReadOnly Property Start As Integer
        Get
            Return _Start
        End Get
    End Property

    Public ReadOnly Property Right As Integer
        Get
            Return _Right
        End Get
    End Property

    Sub New(Start As Integer, Right As Integer, Font As Font, Document As Rtf)
        _Start = Start
        _Right = Right
        Me.Font = Font
        Me.RTFDocument = Document

        If Start <= 0 Then
            _Start = 1
        End If
        If Right > RTFDocument.Length Then
            _Right = RTFDocument.Length
        End If
    End Sub

    Public Function Contains(p As Integer) As Boolean
        Return p >= Start AndAlso p <= Right
    End Function

    Public Overrides Function ToString() As String
        Return Text
    End Function

    Public Function GenerateDocumentText() As String
        Return Font.GenerateRTFTAG(Me)
    End Function

    ''' <summary>
    ''' 是否具有分段的标识符
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property HaveParFlag As Boolean
        Get
            Dim TextCache As String = Text

            If InStr(TextCache, vbCrLf) > 0 OrElse InStr(TextCache, vbCr) > 0 OrElse InStr(TextCache, vbLf) > 0 Then
                Return True
            Else
                Return False
            End If
        End Get
    End Property
End Class

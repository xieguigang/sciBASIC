Imports System.Collections.Generic
Imports System.Text

Namespace HTML.CSS.Render

    ''' <summary>
    ''' Splits text on words for a box
    ''' </summary>
    Friend Class CssBoxWordSplitter
#Region "Static"

        ''' <summary>
        ''' Returns a bool indicating if the specified box white-space processing model specifies
        ''' that sequences of white spaces should be collapsed on a single whitespace
        ''' </summary>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Public Shared Function CollapsesWhiteSpaces(b As CssBox) As Boolean
            Return b.WhiteSpace = CssConstants.Normal OrElse b.WhiteSpace = CssConstants.Nowrap OrElse b.WhiteSpace = CssConstants.PreLine
        End Function

        ''' <summary>
        ''' Returns a bool indicating if line breaks at the source should be eliminated
        ''' </summary>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Public Shared Function EliminatesLineBreaks(b As CssBox) As Boolean
            Return b.WhiteSpace = CssConstants.Normal OrElse b.WhiteSpace = CssConstants.Nowrap
        End Function

#End Region

#Region "Fields"
        Private _box As CssBox
        Private _text As String
        Private _words As List(Of CssBoxWord)
        Private _curword As CssBoxWord

#End Region

#Region "Ctor"

        Private Sub New()
            _words = New List(Of CssBoxWord)()
            _curword = Nothing
        End Sub

        Public Sub New(box As CssBox, text As String)
            Me.New()
            _box = box
            _text = text.Replace(vbCr, String.Empty)


        End Sub

#End Region

#Region "Props"


        Public ReadOnly Property Words() As List(Of CssBoxWord)
            Get
                Return _words
            End Get
        End Property


        Public ReadOnly Property Text() As String
            Get
                Return _text
            End Get
        End Property


        Public ReadOnly Property Box() As CssBox
            Get
                Return _box
            End Get
        End Property


#End Region

#Region "Public Metods"

        ''' <summary>
        ''' Splits the text on words using rules of the specified box
        ''' </summary>
        Public Sub SplitWords()

            If String.IsNullOrEmpty(Text) Then
                Return
            End If

            _curword = New CssBoxWord(Box)

            Dim onspace As Boolean = IsSpace(Text(0))

            For i As Integer = 0 To Text.Length - 1
                If IsSpace(Text(i)) Then
                    If Not onspace Then
                        CutWord()
                    End If

                    If IsLineBreak(Text(i)) Then
                        _curword.AppendChar(ControlChars.Lf)
                        CutWord()
                    ElseIf IsTab(Text(i)) Then
                        _curword.AppendChar(ControlChars.Tab)
                        CutWord()
                    Else
                        _curword.AppendChar(" "c)
                    End If

                    onspace = True
                Else
                    If onspace Then
                        CutWord()
                    End If
                    _curword.AppendChar(Text(i))

                    onspace = False
                End If
            Next

            CutWord()
        End Sub

        Private Sub CutWord()
            If _curword.Text.Length > 0 Then
                Words.Add(_curword)
            End If
            _curword = New CssBoxWord(Box)
        End Sub

        Private Function IsSpace(c As Char) As Boolean
            Return c = " "c OrElse c = ControlChars.Tab OrElse c = ControlChars.Lf
        End Function

        Private Function IsLineBreak(c As Char) As Boolean
            Return c = ControlChars.Lf OrElse c = ChrW(7)
        End Function

        Private Function IsTab(c As Char) As Boolean
            Return c = ControlChars.Tab
        End Function

#End Region
    End Class

End Namespace
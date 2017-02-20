Imports System.Collections.Generic
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS.Render

Namespace HTML.Render

    Public Class HtmlTag
#Region "Fields"

        Private _tagName As String
        Private _isClosing As Boolean
        Private _attributes As Dictionary(Of String, String)

#End Region

#Region "Ctor"

        Private Sub New()
            _attributes = New Dictionary(Of String, String)()
        End Sub

        Public Sub New(tag As String)
            Me.New()
            tag = tag.Substring(1, tag.Length - 2)

            Dim spaceIndex As Integer = tag.IndexOf(" ")

            'Extract tag name
            If spaceIndex < 0 Then
                _tagName = tag
            Else
                _tagName = tag.Substring(0, spaceIndex)
            End If

            'Check if is end tag
            If _tagName.StartsWith("/") Then
                _isClosing = True
                _tagName = _tagName.Substring(1)
            End If

            _tagName = _tagName.ToLower()

            'Extract attributes
            Dim atts As MatchCollection = Parser.Match(Parser.HmlTagAttributes, tag)

            For Each att As Match In atts
                'Extract attribute and value
                Dim chunks As String() = att.Value.Split("="c)

                If chunks.Length = 1 Then
                    If Not Attributes.ContainsKey(chunks(0)) Then
                        Attributes.Add(chunks(0).ToLower(), String.Empty)
                    End If
                ElseIf chunks.Length = 2 Then
                    Dim attname As String = chunks(0).Trim()
                    Dim attvalue As String = chunks(1).Trim()

                    If attvalue.StartsWith("""") AndAlso attvalue.EndsWith("""") AndAlso attvalue.Length > 2 Then
                        attvalue = attvalue.Substring(1, attvalue.Length - 2)
                    End If

                    If Not Attributes.ContainsKey(attname) Then
                        Attributes.Add(attname, attvalue)
                    End If
                End If
            Next
        End Sub

#End Region

#Region "Props"

        ''' <summary>
        ''' Gets the dictionary of attributes in the tag
        ''' </summary>
        Public ReadOnly Property Attributes() As Dictionary(Of String, String)
            Get
                Return _attributes
            End Get
        End Property


        ''' <summary>
        ''' Gets the name of this tag
        ''' </summary>
        Public ReadOnly Property TagName() As String
            Get
                Return _tagName
            End Get
        End Property

        ''' <summary>
        ''' Gets if the tag is actually a closing tag
        ''' </summary>
        Public ReadOnly Property IsClosing() As Boolean
            Get
                Return _isClosing
            End Get
        End Property

        ''' <summary>
        ''' Gets if the tag is single placed; in other words it doesn't need a closing tag; 
        ''' e.g. &lt;br&gt;
        ''' </summary>
        Public ReadOnly Property IsSingle() As Boolean
            Get
                Return TagName.StartsWith("!") OrElse (New List(Of String)(New String() {"area", "base", "basefont", "br", "col", "frame",
                "hr", "img", "input", "isindex", "link", "meta",
                "param"})).Contains(TagName)
            End Get
        End Property

        Friend Sub TranslateAttributes(box As CssBox)
            Dim t As String = TagName.ToUpper()

            For Each att As String In Attributes.Keys
                Dim value As String = Attributes(att)

                Select Case att
                    Case HtmlConstants.align
                        If value = HtmlConstants.left OrElse value = HtmlConstants.center OrElse value = HtmlConstants.right OrElse value = HtmlConstants.justify Then
                            box.TextAlign = value
                        Else
                            box.VerticalAlign = value
                        End If

                    Case HtmlConstants.background
                        box.BackgroundImage = value

                    Case HtmlConstants.bgcolor
                        box.BackgroundColor = value

                    Case HtmlConstants.border
                        box.BorderWidth = TranslateLength(value)

                        If t = HtmlConstants.TABLE Then
                            ApplyTableBorder(box, value)
                        Else
                            box.BorderStyle = CssConstants.Solid
                        End If

                    Case HtmlConstants.bordercolor
                        box.BorderColor = value

                    Case HtmlConstants.cellspacing
                        box.BorderSpacing = TranslateLength(value)

                    Case HtmlConstants.cellpadding
                        ApplyTablePadding(box, value)

                    Case HtmlConstants.color
                        box.Color = value

                    Case HtmlConstants.dir
                        box.Direction = value

                    Case HtmlConstants.face
                        box.FontFamily = value

                    Case HtmlConstants.height
                        box.Height = TranslateLength(value)

                    Case HtmlConstants.hspace
                        box.MarginLeft = TranslateLength(value)
                        box.MarginRight = box.MarginLeft

                    Case HtmlConstants.nowrap
                        box.WhiteSpace = CssConstants.Nowrap

                    Case HtmlConstants.size
                        If t = HtmlConstants.HR Then
                            box.Height = TranslateLength(value)
                        End If

                    Case HtmlConstants.valign
                        box.VerticalAlign = value

                    Case HtmlConstants.vspace
                        box.MarginBottom = TranslateLength(value)
                        box.MarginTop = box.MarginBottom

                    Case HtmlConstants.width
                        box.Width = TranslateLength(value)


                End Select
            Next
        End Sub

#End Region

#Region "Methods"

        ''' <summary>
        ''' Converts an HTML length into a Css length
        ''' </summary>
        ''' <param name="htmlLength"></param>
        ''' <returns></returns>
        Private Function TranslateLength(htmlLength As String) As String
            Dim len As New CssLength(htmlLength)

            If len.HasError Then
                Return htmlLength & "px"
            End If

            Return htmlLength
        End Function

        ''' <summary>
        ''' Cascades to the TD's the border spacified in the TABLE tag.
        ''' </summary>
        ''' <param name="table"></param>
        ''' <param name="border"></param>
        Private Sub ApplyTableBorder(table As CssBox, border As String)
            For Each box As CssBox In table.Boxes
                For Each cell As CssBox In box.Boxes
                    cell.BorderWidth = TranslateLength(border)
                Next
            Next
        End Sub

        ''' <summary>
        ''' Cascades to the TD's the border spacified in the TABLE tag.
        ''' </summary>
        ''' <param name="table"></param>
        ''' <param name="padding"></param>
        Private Sub ApplyTablePadding(table As CssBox, padding As String)
            For Each box As CssBox In table.Boxes
                For Each cell As CssBox In box.Boxes

                    cell.Padding = TranslateLength(padding)
                Next
            Next
        End Sub

        ''' <summary>
        ''' Gets a boolean indicating if the attribute list has the specified attribute
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <returns></returns>
        Public Function HasAttribute(attribute As String) As Boolean
            Return Attributes.ContainsKey(attribute)
        End Function

        Public Overrides Function ToString() As String
            Return String.Format("<{1}{0}>", TagName, If(IsClosing, "/", String.Empty))
        End Function
#End Region
    End Class
End Namespace
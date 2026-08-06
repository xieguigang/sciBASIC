Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.Writer.Styling.Style

Namespace XLSX.Writer.Styling


    ''' <summary>
    ''' Factory class with the most important predefined styles
    ''' </summary>
    Public NotInheritable Class BasicStyles

        ''' <summary>
        ''' Defines the bold, italic, boldItalic, underline, doubleUnderline, strike, dateFormat, timeFormat, roundFormat, borderFrame, borderFrameHeader, dottedFill_0_125, mergeCellStyle
        ''' </summary>
        Private Shared boldField, italicField, boldItalicField, underlineField, doubleUnderlineField, strikeField, dateFormatField, timeFormatField, roundFormatField, borderFrameField, borderFrameHeaderField, dottedFill_0_125Field, mergeCellStyleField As Style

        ''' <summary>
        ''' Gets the Bold
        ''' </summary>
        Public Shared ReadOnly Property Bold As Style
            Get
                Return GetStyle(StyleEnum.bold)
            End Get
        End Property

        ''' <summary>
        ''' Gets the BoldItalic
        ''' </summary>
        Public Shared ReadOnly Property BoldItalic As Style
            Get
                Return GetStyle(StyleEnum.boldItalic)
            End Get
        End Property

        ''' <summary>
        ''' Gets the BorderFrame
        ''' </summary>
        Public Shared ReadOnly Property BorderFrame As Style
            Get
                Return GetStyle(StyleEnum.borderFrame)
            End Get
        End Property

        ''' <summary>
        ''' Gets the BorderFrameHeader
        ''' </summary>
        Public Shared ReadOnly Property BorderFrameHeader As Style
            Get
                Return GetStyle(StyleEnum.borderFrameHeader)
            End Get
        End Property

        ''' <summary>
        ''' Gets the DateFormat
        ''' </summary>
        Public Shared ReadOnly Property DateFormat As Style
            Get
                Return GetStyle(StyleEnum.dateFormat)
            End Get
        End Property

        ''' <summary>
        ''' Gets the TimeFormat
        ''' </summary>
        Public Shared ReadOnly Property TimeFormat As Style
            Get
                Return GetStyle(StyleEnum.timeFormat)
            End Get
        End Property

        ''' <summary>
        ''' Gets the DoubleUnderline
        ''' </summary>
        Public Shared ReadOnly Property DoubleUnderline As Style
            Get
                Return GetStyle(StyleEnum.doubleUnderline)
            End Get
        End Property

        ''' <summary>
        ''' Gets the DottedFill_0_125
        ''' </summary>
        Public Shared ReadOnly Property DottedFill_0_125 As Style
            Get
                Return GetStyle(StyleEnum.dottedFill_0_125)
            End Get
        End Property

        ''' <summary>
        ''' Gets the Italic
        ''' </summary>
        Public Shared ReadOnly Property Italic As Style
            Get
                Return GetStyle(StyleEnum.italic)
            End Get
        End Property

        ''' <summary>
        ''' Gets the MergeCellStyle
        ''' </summary>
        Public Shared ReadOnly Property MergeCellStyle As Style
            Get
                Return GetStyle(StyleEnum.mergeCellStyle)
            End Get
        End Property

        ''' <summary>
        ''' Gets the RoundFormat
        ''' </summary>
        Public Shared ReadOnly Property RoundFormat As Style
            Get
                Return GetStyle(StyleEnum.roundFormat)
            End Get
        End Property

        ''' <summary>
        ''' Gets the Strike
        ''' </summary>
        Public Shared ReadOnly Property Strike As Style
            Get
                Return GetStyle(StyleEnum.strike)
            End Get
        End Property

        ''' <summary>
        ''' Gets the Underline
        ''' </summary>
        Public Shared ReadOnly Property Underline As Style
            Get
                Return GetStyle(StyleEnum.underline)
            End Get
        End Property

        ''' <summary>
        ''' Method to maintain the styles and to create singleton instances
        ''' </summary>
        ''' <param name="value">Enum value to maintain.</param>
        ''' <returns>The style according to the passed enum value.</returns>
        Private Shared Function GetStyle(value As StyleEnum) As Style
            Dim s As Style = Nothing
            Select Case value
                Case StyleEnum.bold
                    If boldField Is Nothing Then
                        boldField = New Style()
                        boldField.CurrentFont.Bold = True
                    End If
                    s = boldField
                Case StyleEnum.italic
                    If italicField Is Nothing Then
                        italicField = New Style()
                        italicField.CurrentFont.Italic = True
                    End If
                    s = italicField
                Case StyleEnum.boldItalic
                    If boldItalicField Is Nothing Then
                        boldItalicField = New Style()
                        boldItalicField.CurrentFont.Italic = True
                        boldItalicField.CurrentFont.Bold = True
                    End If
                    s = boldItalicField
                Case StyleEnum.underline
                    If underlineField Is Nothing Then
                        underlineField = New Style()
                        underlineField.CurrentFont.Underline = Style.Font.UnderlineValue.u_single
                    End If
                    s = underlineField
                Case StyleEnum.doubleUnderline
                    If doubleUnderlineField Is Nothing Then
                        doubleUnderlineField = New Style()
                        doubleUnderlineField.CurrentFont.Underline = Style.Font.UnderlineValue.u_double
                    End If
                    s = doubleUnderlineField
                Case StyleEnum.strike
                    If strikeField Is Nothing Then
                        strikeField = New Style()
                        strikeField.CurrentFont.Strike = True
                    End If
                    s = strikeField
                Case StyleEnum.dateFormat
                    If dateFormatField Is Nothing Then
                        dateFormatField = New Style()
                        dateFormatField.CurrentNumberFormat.Number = NumberFormat.FormatNumber.format_14
                    End If
                    s = dateFormatField
                Case StyleEnum.timeFormat
                    If timeFormatField Is Nothing Then
                        timeFormatField = New Style()
                        timeFormatField.CurrentNumberFormat.Number = NumberFormat.FormatNumber.format_21
                    End If
                    s = timeFormatField
                Case StyleEnum.roundFormat
                    If roundFormatField Is Nothing Then
                        roundFormatField = New Style()
                        roundFormatField.CurrentNumberFormat.Number = NumberFormat.FormatNumber.format_1
                    End If
                    s = roundFormatField
                Case StyleEnum.borderFrame
                    If borderFrameField Is Nothing Then
                        borderFrameField = New Style()
                        borderFrameField.CurrentBorder.TopStyle = Border.StyleValue.thin
                        borderFrameField.CurrentBorder.BottomStyle = Border.StyleValue.thin
                        borderFrameField.CurrentBorder.LeftStyle = Border.StyleValue.thin
                        borderFrameField.CurrentBorder.RightStyle = Border.StyleValue.thin
                    End If
                    s = borderFrameField
                Case StyleEnum.borderFrameHeader
                    If borderFrameHeaderField Is Nothing Then
                        borderFrameHeaderField = New Style()
                        borderFrameHeaderField.CurrentBorder.TopStyle = Border.StyleValue.thin
                        borderFrameHeaderField.CurrentBorder.BottomStyle = Border.StyleValue.medium
                        borderFrameHeaderField.CurrentBorder.LeftStyle = Border.StyleValue.thin
                        borderFrameHeaderField.CurrentBorder.RightStyle = Border.StyleValue.thin
                        borderFrameHeaderField.CurrentFont.Bold = True
                    End If
                    s = borderFrameHeaderField
                Case StyleEnum.dottedFill_0_125
                    If dottedFill_0_125Field Is Nothing Then
                        dottedFill_0_125Field = New Style()
                        dottedFill_0_125Field.CurrentFill.PatternFill = Fill.PatternValue.gray125
                    End If
                    s = dottedFill_0_125Field
                Case StyleEnum.mergeCellStyle
                    If mergeCellStyleField Is Nothing Then
                        mergeCellStyleField = New Style()
                        mergeCellStyleField.CurrentCellXf.ForceApplyAlignment = True
                    End If
                    s = mergeCellStyleField
            End Select
            Return s.CopyStyle() ' Copy makes basic styles immutable
        End Function

        ''' <summary>
        ''' Gets a style to colorize the text of a cell
        ''' </summary>
        ''' <param name="rgb">RGB code in hex format (6 characters, e.g. FF00AC). Alpha will be set to full opacity (FF).</param>
        ''' <returns>Style with font color definition.</returns>
        Public Shared Function ColorizedText(rgb As String) As Style
            Fill.ValidateColor(rgb, False)
            Dim s As Style = New Style()
            s.CurrentFont.ColorValue = "FF" & rgb.ToUpper()
            Return s
        End Function

        ''' <summary>
        ''' Gets a style to colorize the background of a cell
        ''' </summary>
        ''' <param name="rgb">RGB code in hex format (6 characters, e.g. FF00AC). Alpha will be set to full opacity (FF).</param>
        ''' <returns>Style with background color definition.</returns>
        Public Shared Function ColorizedBackground(rgb As String) As Style
            Fill.ValidateColor(rgb, False)
            Dim s As Style = New Style()
            s.CurrentFill.SetColor("FF" & rgb.ToUpper(), Fill.FillType.fillColor)
            Return s
        End Function

        ''' <summary>
        ''' Gets a style with a user defined font
        ''' </summary>
        ''' <param name="fontName">Name of the font.</param>
        ''' <param name="fontSize">Size of the font in points (optional; default 11).</param>
        ''' <param name="isBold">If true, the font will be bold (optional; default false).</param>
        ''' <param name="isItalic">If true, the font will be italic (optional; default false).</param>
        ''' <returns>Style with font definition.</returns>
        Public Shared Function Font(fontName As String, Optional fontSize As Integer = 11, Optional isBold As Boolean = False, Optional isItalic As Boolean = False) As Style
            Dim s As Style = New Style()
            s.CurrentFont.Name = fontName
            s.CurrentFont.Size = fontSize
            s.CurrentFont.Bold = isBold
            s.CurrentFont.Italic = isItalic
            Return s
        End Function
    End Class
End Namespace
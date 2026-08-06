Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.Writer.Styling.Style

Namespace XLSX.Writer.Styling


    ''' <summary>
    ''' Factory class with the most important predefined styles
    ''' </summary>
    Public NotInheritable Class BasicStyles

        ''' <summary>
        ''' Defines the bold, italic, boldItalic, underline, doubleUnderline, strike, dateFormat, timeFormat, roundFormat, borderFrame, borderFrameHeader, dottedFill_0_125, mergeCellStyle
        ''' </summary>
        Private Shared m_bold,
            m_italic,
            m_boldItalic,
            m_underline,
            m_doubleUnderline,
            m_strike,
            m_dateFormat,
            m_timeFormat,
            m_roundFormat,
            m_borderFrame,
            m_borderFrameHeader,
            m_dottedFill_0_125,
            m_mergeCellStyle As Style

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
                    If m_bold Is Nothing Then
                        m_bold = New Style()
                        m_bold.CurrentFont.Bold = True
                    End If
                    s = m_bold
                Case StyleEnum.italic
                    If m_italic Is Nothing Then
                        m_italic = New Style()
                        m_italic.CurrentFont.Italic = True
                    End If
                    s = m_italic
                Case StyleEnum.boldItalic
                    If m_boldItalic Is Nothing Then
                        m_boldItalic = New Style()
                        m_boldItalic.CurrentFont.Italic = True
                        m_boldItalic.CurrentFont.Bold = True
                    End If
                    s = m_boldItalic
                Case StyleEnum.underline
                    If m_underline Is Nothing Then
                        m_underline = New Style()
                        m_underline.CurrentFont.Underline = UnderlineValue.u_single
                    End If
                    s = m_underline
                Case StyleEnum.doubleUnderline
                    If m_doubleUnderline Is Nothing Then
                        m_doubleUnderline = New Style()
                        m_doubleUnderline.CurrentFont.Underline = UnderlineValue.u_double
                    End If
                    s = m_doubleUnderline
                Case StyleEnum.strike
                    If m_strike Is Nothing Then
                        m_strike = New Style()
                        m_strike.CurrentFont.Strike = True
                    End If
                    s = m_strike
                Case StyleEnum.dateFormat
                    If m_dateFormat Is Nothing Then
                        m_dateFormat = New Style()
                        m_dateFormat.CurrentNumberFormat.Number = FormatNumber.format_14
                    End If
                    s = m_dateFormat
                Case StyleEnum.timeFormat
                    If m_timeFormat Is Nothing Then
                        m_timeFormat = New Style()
                        m_timeFormat.CurrentNumberFormat.Number = FormatNumber.format_21
                    End If
                    s = m_timeFormat
                Case StyleEnum.roundFormat
                    If m_roundFormat Is Nothing Then
                        m_roundFormat = New Style()
                        m_roundFormat.CurrentNumberFormat.Number = FormatNumber.format_1
                    End If
                    s = m_roundFormat
                Case StyleEnum.borderFrame
                    If m_borderFrame Is Nothing Then
                        m_borderFrame = New Style()
                        m_borderFrame.CurrentBorder.TopStyle = StyleValue.thin
                        m_borderFrame.CurrentBorder.BottomStyle = StyleValue.thin
                        m_borderFrame.CurrentBorder.LeftStyle = StyleValue.thin
                        m_borderFrame.CurrentBorder.RightStyle = StyleValue.thin
                    End If
                    s = m_borderFrame
                Case StyleEnum.borderFrameHeader
                    If m_borderFrameHeader Is Nothing Then
                        m_borderFrameHeader = New Style()
                        m_borderFrameHeader.CurrentBorder.TopStyle = StyleValue.thin
                        m_borderFrameHeader.CurrentBorder.BottomStyle = StyleValue.medium
                        m_borderFrameHeader.CurrentBorder.LeftStyle = StyleValue.thin
                        m_borderFrameHeader.CurrentBorder.RightStyle = StyleValue.thin
                        m_borderFrameHeader.CurrentFont.Bold = True
                    End If
                    s = m_borderFrameHeader
                Case StyleEnum.dottedFill_0_125
                    If m_dottedFill_0_125 Is Nothing Then
                        m_dottedFill_0_125 = New Style()
                        m_dottedFill_0_125.CurrentFill.PatternFill = PatternValue.gray125
                    End If
                    s = m_dottedFill_0_125
                Case StyleEnum.mergeCellStyle
                    If m_mergeCellStyle Is Nothing Then
                        m_mergeCellStyle = New Style()
                        m_mergeCellStyle.CurrentCellXf.ForceApplyAlignment = True
                    End If
                    s = m_mergeCellStyle
            End Select
            Return s.CopyStyle() ' Copy makes basic styles immutable
        End Function

        ''' <summary>
        ''' Gets a style to colorize the text of a cell
        ''' </summary>
        ''' <param name="rgb">RGB code in hex format (6 characters, e.g. FF00AC). Alpha will be set to full opacity (FF).</param>
        ''' <returns>Style with font color definition.</returns>
        Public Shared Function ColorizedText(rgb As String) As Style
            Dim s As Style = New Style()
            ' NormalizeColor completes the alpha channel to FF and strips a leading '#',
            ' so both "FF00AC" and "#FF00AC" work. Assigning through the property stores
            ' the normalized value (the setter no longer just validates).
            s.CurrentFont.ColorValue = Fill.NormalizeColor(rgb, True)
            Return s
        End Function

        ''' <summary>
        ''' Gets a style to colorize the background of a cell
        ''' </summary>
        ''' <param name="rgb">RGB code in hex format (6 characters, e.g. FF00AC). Alpha will be set to full opacity (FF).</param>
        ''' <returns>Style with background color definition.</returns>
        Public Shared Function ColorizedBackground(rgb As String) As Style
            Dim s As Style = New Style()
            ' NormalizeColor completes the alpha channel to FF and strips a leading '#',
            ' so both "FF00AC" and "#FF00AC" work uniformly with ColorizedText.
            s.CurrentFill.SetColor(Fill.NormalizeColor(rgb, True), FillType.fillColor)
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
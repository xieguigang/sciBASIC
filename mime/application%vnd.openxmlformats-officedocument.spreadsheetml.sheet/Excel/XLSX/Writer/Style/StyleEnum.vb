Namespace XLSX.Writer.Styling

    ''' <summary>
    ''' Enum with style selection
    ''' </summary>
    Friend Enum StyleEnum
        ''' <summary>Format text bold</summary>
        bold
        ''' <summary>Format text italic</summary>
        italic
        ''' <summary>Format text bold and italic</summary>
        boldItalic
        ''' <summary>Format text with an underline</summary>
        underline
        ''' <summary>Format text with a double underline</summary>
        doubleUnderline
        ''' <summary>Format text with a strike-through</summary>
        strike
        ''' <summary>Format number as date</summary>
        dateFormat
        ''' <summary>Format number as time</summary>
        timeFormat
        ''' <summary>Rounds number as an integer</summary>
        roundFormat
        ''' <summary>Format cell with a thin border</summary>
        borderFrame
        ''' <summary>Format cell with a thin border and a thick bottom line as header cell</summary>
        borderFrameHeader
        ''' <summary>Special pattern fill style for compatibility purpose </summary>
        dottedFill_0_125
        ''' <summary>Style to apply on merged cells </summary>
        mergeCellStyle
    End Enum
End Namespace
#Region "Microsoft.VisualBasic::e7b2d63d99ae0596fac4d841c07c708a, mime\text%yaml\1.1\Base\ScalarStyle.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Enum ScalarStyle
    ' 
    '         DoubleQuoted, Hex, Plain, SingleQuoted
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Grammar11

    ''' <summary>
    ''' Specifies the style of a YAML scalar.
    ''' </summary>
    Public Enum ScalarStyle
        ''' <summary>
        ''' The plain scalar style.
        ''' </summary>
        Plain

        ''' <summary>
        ''' 
        ''' </summary>
        Hex

        ''' <summary>
        ''' The single-quoted scalar style.
        ''' </summary>
        SingleQuoted

        ''' <summary>
        ''' The double-quoted scalar style.
        ''' </summary>
        DoubleQuoted
    End Enum
End Namespace

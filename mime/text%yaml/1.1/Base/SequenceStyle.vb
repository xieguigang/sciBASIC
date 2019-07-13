#Region "Microsoft.VisualBasic::af3049e344b85a6da6fea3244406ec4b, mime\text%yaml\1.1\Base\SequenceStyle.vb"

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

    '     Enum SequenceStyle
    ' 
    '         Block, Flow, Raw
    ' 
    '  
    ' 
    ' 
    ' 
    '     Module SequenceStyleExtensions
    ' 
    '         Function: ToScalarStyle
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Grammar11

    ''' <summary>
    ''' Specifies the style of a sequence.
    ''' </summary>
    Public Enum SequenceStyle
        ''' <summary>
        ''' The block sequence style.
        ''' </summary>
        Block

        ''' <summary>
        ''' The flow sequence style.
        ''' </summary>
        Flow

        ''' <summary>
        ''' SIngle line with hex data
        ''' </summary>
        Raw
    End Enum

    Public Module SequenceStyleExtensions

        ''' <summary>
        ''' Get scalar style corresponding to current sequence style
        ''' </summary>
        ''' <param name="this">Sequence style</param>
        ''' <returns>Corresponding scalar style</returns>
        <Extension>
        Public Function ToScalarStyle(this As SequenceStyle) As ScalarStyle
            If this = SequenceStyle.Raw Then
                Return ScalarStyle.Hex
            End If
            Return ScalarStyle.Plain
        End Function
    End Module
End Namespace

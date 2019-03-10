Imports System.Runtime.CompilerServices

Namespace Text.Parser

    Public Enum TryParseOptions
        ''' <summary>
        ''' Function should returns empty string when try parse failed.
        ''' </summary>
        Empty
        ''' <summary>
        ''' Function should returns nothing when try parse failed.
        ''' </summary>
        [Nothing]
        ''' <summary>
        ''' Function should returns the source input when try parse failed.
        ''' </summary>
        Source
    End Enum

    Public Delegate Function ITryParse(input As String, ByRef output As String) As Boolean

    ''' <summary>
    ''' Helpers for text parser
    ''' </summary>
    Public Module ParserHelpers

        <Extension>
        Public Function TryParse(parser As ITryParse, input$, Optional opt As TryParseOptions = TryParseOptions.Empty) As String
            Dim out$ = Nothing

            If parser(input, out) Then
                Return out
            Else
                Select Case opt
                    Case TryParseOptions.Empty
                        Return ""
                    Case TryParseOptions.Nothing
                        Return Nothing
                    Case TryParseOptions.Source
                        Return input
                    Case Else
                        Return String.Empty
                End Select
            End If
        End Function
    End Module
End Namespace
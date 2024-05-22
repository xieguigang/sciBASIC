#Region "Microsoft.VisualBasic::f55b44ccaca05d1edae7551d0b6e9456, Microsoft.VisualBasic.Core\src\Text\Parser\TryParse.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 47
    '    Code Lines: 29 (61.70%)
    ' Comment Lines: 12 (25.53%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (12.77%)
    '     File Size: 1.47 KB


    '     Enum TryParseOptions
    ' 
    '         [Nothing], Empty, Source
    ' 
    '  
    ' 
    ' 
    ' 
    '     Delegate Function
    ' 
    ' 
    '     Module ParserHelpers
    ' 
    '         Function: TryParse
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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

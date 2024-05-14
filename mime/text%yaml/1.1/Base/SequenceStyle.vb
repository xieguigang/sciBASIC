#Region "Microsoft.VisualBasic::af3049e344b85a6da6fea3244406ec4b, mime\text%yaml\1.1\Base\SequenceStyle.vb"

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

    '   Total Lines: 40
    '    Code Lines: 17
    ' Comment Lines: 17
    '   Blank Lines: 6
    '     File Size: 1.07 KB


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

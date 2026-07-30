#Region "Microsoft.VisualBasic::8d13dc0ea98f2b4ff443340865302f2e, vs_solutions\dev\vs_PDB\Models\Symbol.vb"

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

    '   Total Lines: 43
    '    Code Lines: 13 (30.23%)
    ' Comment Lines: 22 (51.16%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (18.60%)
    '     File Size: 1.40 KB


    '     Class Symbol
    ' 
    '         Properties: Flags, Kind, Length, Name, Offset
    '                     Section
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Models

    ''' <summary>
    ''' A single symbol (function / public / data) extracted from the symbol stream.
    ''' </summary>
    Public Class Symbol

        ''' <summary>
        ''' Symbol / function name.
        ''' </summary>
        Public Property Name As String

        ''' <summary>
        ''' Section index (segment) where the symbol lives, 1-based. 0 for flat addressing.
        ''' </summary>
        Public Property Section As UShort

        ''' <summary>
        ''' Offset of the symbol from the start of <see cref="Section"/> (or from image base
        ''' when <see cref="Section"/> is 0).
        ''' </summary>
        Public Property Offset As UInteger

        ''' <summary>
        ''' Length of the symbol in bytes (function body size), 0 when unknown.
        ''' </summary>
        Public Property Length As UInteger

        ''' <summary>
        ''' Kind of symbol (Public / Function / Data / ...). See <see cref="SymbolKind"/>.
        ''' </summary>
        Public Property Kind As SymbolKind

        ''' <summary>
        ''' Flags of the symbol (e.g. code / function).
        ''' </summary>
        Public Property Flags As UShort

        Public Overrides Function ToString() As String
            Return $"{Kind} {Name} @[{Section}:{Offset:X}+#{Length}]"
        End Function
    End Class
End Namespace

#Region "Microsoft.VisualBasic::dcf0b83cbfef90c22f1a6f5ddeae66a6, vs_solutions\dev\VisualStudio\VBProject\CodeDOM\EnumSymbol.vb"

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

    '   Total Lines: 12
    '    Code Lines: 7 (58.33%)
    ' Comment Lines: 3 (25.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 2 (16.67%)
    '     File Size: 418 B


    '     Class EnumSymbol
    ' 
    '         Properties: EnumBaseType, Type
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace VBProj.CodeDOM

    Public Class EnumSymbol : Inherits TypeContainerSymbol
        Public Overrides ReadOnly Property Type As SymbolType = SymbolType.[Enum]
        ''' <summary>
        ''' the underlying base type of an enum, e.g. Enum X As Long
        ''' </summary>
        Public Property EnumBaseType As TypeInfo
    End Class
End Namespace

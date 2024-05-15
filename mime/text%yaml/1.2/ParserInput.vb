#Region "Microsoft.VisualBasic::adec478ab0683caaeb480cf13b7bac4a, mime\text%yaml\1.2\ParserInput.vb"

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

    '   Total Lines: 16
    '    Code Lines: 11
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 477 B


    '     Interface ParserInput
    ' 
    '         Properties: Length
    ' 
    '         Function: FormErrorMessage, GetInputSymbol, GetSubSection, HasInput
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic
Imports System.Text

Namespace Grammar

    Public Interface ParserInput(Of T)

        ReadOnly Property Length() As Integer

        Function HasInput(pos As Integer) As Boolean
        Function GetInputSymbol(pos As Integer) As T
        Function GetSubSection(position As Integer, length As Integer) As T()
        Function FormErrorMessage(position As Integer, message As String) As String

    End Interface
End Namespace

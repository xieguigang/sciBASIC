#Region "Microsoft.VisualBasic::894c6bc530ff29f399cd5a2db08edc9a, mime\application%pdf\PdfReader\Tokenizer\TokenObject.vb"

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

    '     Class TokenObject
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class TokenArrayOpen
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class TokenArrayClose
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class TokenDictionaryOpen
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class TokenDictionaryClose
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class TokenEmpty
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace PdfReader
    Public MustInherit Class TokenObject
        Public Shared ReadOnly ArrayOpen As TokenArrayOpen = New TokenArrayOpen()
        Public Shared ReadOnly ArrayClose As TokenArrayClose = New TokenArrayClose()
        Public Shared ReadOnly DictionaryOpen As TokenDictionaryOpen = New TokenDictionaryOpen()
        Public Shared ReadOnly DictionaryClose As TokenDictionaryClose = New TokenDictionaryClose()
        Public Shared ReadOnly Empty As TokenEmpty = New TokenEmpty()

        Public Sub New()
        End Sub
    End Class

    Public Class TokenArrayOpen
        Inherits TokenObject

        Public Sub New()
        End Sub
    End Class

    Public Class TokenArrayClose
        Inherits TokenObject

        Public Sub New()
        End Sub
    End Class

    Public Class TokenDictionaryOpen
        Inherits TokenObject

        Public Sub New()
        End Sub
    End Class

    Public Class TokenDictionaryClose
        Inherits TokenObject

        Public Sub New()
        End Sub
    End Class

    Public Class TokenEmpty
        Inherits TokenObject

        Public Sub New()
        End Sub
    End Class
End Namespace

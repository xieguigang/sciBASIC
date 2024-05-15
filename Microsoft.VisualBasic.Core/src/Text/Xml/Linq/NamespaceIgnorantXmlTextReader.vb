#Region "Microsoft.VisualBasic::1dd2d81772c9463a25d2615ef944eb6f, Microsoft.VisualBasic.Core\src\Text\Xml\Linq\NamespaceIgnorantXmlTextReader.vb"

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

    '   Total Lines: 26
    '    Code Lines: 18
    ' Comment Lines: 3
    '   Blank Lines: 5
    '     File Size: 664 B


    '     Class NamespaceIgnorantXmlTextReader
    ' 
    '         Properties: NamespaceURI
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Xml

Namespace Text.Xml.Linq

    ''' <summary>
    ''' https://stackoverflow.com/questions/12590487/net-xml-deserialization-ignore-namespaces
    ''' </summary>
    Friend Class NamespaceIgnorantXmlTextReader
        Inherits XmlTextReader

        Public Overrides ReadOnly Property NamespaceURI As String
            Get
                Return ""
            End Get
        End Property

        Public Sub New(stream As Stream)
            Call MyBase.New(stream)
        End Sub

        Public Sub New(text As TextReader)
            Call MyBase.New(text)
        End Sub
    End Class
End Namespace

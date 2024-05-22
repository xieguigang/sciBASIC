#Region "Microsoft.VisualBasic::4a7bb088d7895eab98e032e24feb94a5, mime\application%pdf\PdfReader\Document\PdfInfo.vb"

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

    '   Total Lines: 67
    '    Code Lines: 56 (83.58%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (16.42%)
    '     File Size: 1.96 KB


    '     Class PdfInfo
    ' 
    '         Properties: Author, CreationDate, Creator, Keywords, ModDate
    '                     Producer, Subject, Title, Trapped
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: Visit
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace PdfReader
    Public Class PdfInfo
        Inherits PdfDictionary

        Public Sub New(parent As PdfObject, parse As ParseDictionary)
            MyBase.New(parent, parse)
        End Sub

        Public Overrides Sub Visit(visitor As IPdfObjectVisitor)
            visitor.Visit(Me)
        End Sub

        Public ReadOnly Property Title As PdfString
            Get
                Return OptionalValue(Of PdfString)("Title")
            End Get
        End Property

        Public ReadOnly Property Author As PdfString
            Get
                Return OptionalValue(Of PdfString)("Author")
            End Get
        End Property

        Public ReadOnly Property Subject As PdfString
            Get
                Return OptionalValue(Of PdfString)("Subject")
            End Get
        End Property

        Public ReadOnly Property Keywords As PdfString
            Get
                Return OptionalValue(Of PdfString)("Keywords")
            End Get
        End Property

        Public ReadOnly Property Creator As PdfString
            Get
                Return OptionalValue(Of PdfString)("Creator")
            End Get
        End Property

        Public ReadOnly Property Producer As PdfString
            Get
                Return OptionalValue(Of PdfString)("Producer")
            End Get
        End Property

        Public ReadOnly Property CreationDate As PdfDateTime
            Get
                Return OptionalDateTime("CreationDate")
            End Get
        End Property

        Public ReadOnly Property ModDate As PdfDateTime
            Get
                Return OptionalDateTime("ModDate")
            End Get
        End Property

        Public ReadOnly Property Trapped As PdfName
            Get
                Return OptionalValue(Of PdfName)("Trapped")
            End Get
        End Property
    End Class
End Namespace

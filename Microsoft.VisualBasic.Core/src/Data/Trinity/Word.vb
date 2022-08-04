#Region "Microsoft.VisualBasic::d50d0ba2fcaaee407553abd3d8e34a41, sciBASIC#\Microsoft.VisualBasic.Core\src\Data\Trinity\Word.vb"

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

    '   Total Lines: 21
    '    Code Lines: 17
    ' Comment Lines: 0
    '   Blank Lines: 4
    '     File Size: 622 B


    '     Class Word
    ' 
    '         Properties: [Class], Text
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Language

Namespace Data.Trinity.NLP

    Public Class Word : Implements Value(Of String).IValueOf

        <XmlAttribute>
        Public Property [Class] As WordClass
        <XmlAttribute>
        Public Property Text As String Implements Value(Of String).IValueOf.Value

        Public Overrides Function ToString() As String
            If [Class] = WordClass.NA Then
                Return Text
            Else
                Return $"{[Class].Description} {Text}"
            End If
        End Function
    End Class
End Namespace

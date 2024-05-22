#Region "Microsoft.VisualBasic::5ec74406f97ddd0daaaff7a118de2c07, mime\application%json\Serializer\JSONSerializerOptions.vb"

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

    '   Total Lines: 10
    '    Code Lines: 8 (80.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 2 (20.00%)
    '     File Size: 383 B


    ' Class JSONSerializerOptions
    ' 
    '     Properties: digest, enumToString, indent, maskReadonly, unicodeEscape
    '                 unixTimestamp
    ' 
    ' /********************************************************************************/

#End Region

Public Class JSONSerializerOptions

    Public Property maskReadonly As Boolean = False
    Public Property indent As Boolean = False
    Public Property enumToString As Boolean = True
    Public Property unixTimestamp As Boolean = True
    Public Property digest As Dictionary(Of Type, Func(Of Object, Object))
    Public Property unicodeEscape As Boolean = True

End Class

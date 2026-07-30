#Region "Microsoft.VisualBasic::26370efa5ebb6e40c488ed52022b1813, mime\application%json\Serializer\JSONSerializerOptions.vb"

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

    '   Total Lines: 44
    '    Code Lines: 22 (50.00%)
    ' Comment Lines: 15 (34.09%)
    '    - Xml Docs: 73.33%
    ' 
    '   Blank Lines: 7 (15.91%)
    '     File Size: 1.57 KB


    ' Class JSONSerializerOptions
    ' 
    '     Properties: comment, custom_name, digest, enumToString, indent
    '                 indent_width, maskNull, maskReadonly, unicodeEscape, unixTimestamp
    ' 
    '     Function: createUniqueKey, offsets
    ' 
    ' /********************************************************************************/

#End Region

Public Class JSONSerializerOptions

#Region "json string format"
    Public Property indent As Boolean = False
    Public Property indent_width As Integer = 4
    Public Property enumToString As Boolean = True
    Public Property unixTimestamp As Boolean = True
    Public Property unicodeEscape As Boolean = True
#End Region

#Region "clr type schema parser"
    Public Property maskReadonly As Boolean = False
    Public Property maskNull As Boolean = True

    ''' <summary>
    ''' show xml comment text in json? this option usually be applied for generates the json config file
    ''' </summary>
    ''' <returns></returns>
    Public Property comment As Boolean = False
    Public Property custom_name As Boolean = False
#End Region

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <returns></returns>
    Public Property digest As Dictionary(Of Type, Func(Of Object, Object))

    Friend Function offsets(indent As Integer) As String
        Return New String(" "c, indent * indent_width)
    End Function

    ''' <summary>
    ''' create unique reference key for make internal schema cache reference 
    ''' </summary>
    ''' <returns></returns>
    Friend Function createUniqueKey() As String
        ' include custom_name so that the internal schema cache distinguishes
        ' between serializing the same type with/without custom name mapping,
        ' avoiding a stale cache hit that skips the custom name rewrite.
        Return $"{maskReadonly},{comment},{custom_name}"
    End Function

End Class

#Region "Microsoft.VisualBasic::b005e7366df828f8c3aacf4bdac6fc47, mime\application%json\Serializer\JSONSerializerOptions.vb"

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

    '   Total Lines: 15
    '    Code Lines: 12 (80.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 3 (20.00%)
    '     File Size: 566 B


    ' Class JSONSerializerOptions
    ' 
    '     Properties: digest, enumToString, indent, indent_width, maskReadonly
    '                 unicodeEscape, unixTimestamp
    ' 
    '     Function: offsets
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
    ''' <summary>
    ''' show xml comment text in json? this option usually be applied for generates the json config file
    ''' </summary>
    ''' <returns></returns>
    Public Property comment As Boolean = False
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
        Return $"{maskReadonly},{comment}"
    End Function

End Class

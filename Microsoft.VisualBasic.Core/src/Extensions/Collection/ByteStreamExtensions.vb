#Region "Microsoft.VisualBasic::6b1acf46c0da816f42b79cd97de6df05, Microsoft.VisualBasic.Core\src\Extensions\Collection\ByteStreamExtensions.vb"

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

    ' Module ByteStreamExtensions
    ' 
    '     Function: AsciiString, UnicodeString, UTF8String
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Linq.IteratorExtensions

<HideModuleName>
Public Module ByteStreamExtensions

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function UTF8String(stream As IEnumerable(Of Byte)) As String
        Return Encoding.UTF8.GetString(stream.ToArray)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function UnicodeString(stream As IEnumerable(Of Byte)) As String
        Return Encoding.Unicode.GetString(stream.ToArray)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function AsciiString(stream As IEnumerable(Of Byte)) As String
        Return Encoding.ASCII.GetString(stream.ToArray)
    End Function
End Module

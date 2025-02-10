#Region "Microsoft.VisualBasic::331c6220daf9a325307903a141e32a8a, Data\BinaryData\HDSPack\Metadata\AttributeMetadata.vb"

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

    '   Total Lines: 32
    '    Code Lines: 20 (62.50%)
    ' Comment Lines: 7 (21.88%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (15.62%)
    '     File Size: 875 B


    ' Class AttributeMetadata
    ' 
    '     Properties: data, GetUnderlyingType, name, type
    ' 
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices

''' <summary>
''' a name key tagged attribute metadata.
''' </summary>
Public Class AttributeMetadata

    ''' <summary>
    ''' the attribute name
    ''' </summary>
    ''' <returns></returns>
    Public Property name As String
    Public Property type As String
    Public Property data As Byte()

    Public ReadOnly Property GetUnderlyingType As Type
        Get
            If type.StringEmpty Then
                Return GetType(Void)
            Else
                Return TypeInfo.GetType(type)
            End If
        End Get
    End Property

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overrides Function ToString() As String
        Return $"Dim {name} As {type} = binary({StringFormats.Lanudry(data.Length)})"
    End Function

End Class

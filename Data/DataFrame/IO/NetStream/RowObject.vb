#Region "Microsoft.VisualBasic::eda1b6a42e778bc44dde43f84f74aaf2, sciBASIC#\Data\DataFrame\IO\NetStream\RowObject.vb"

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

    '   Total Lines: 84
    '    Code Lines: 0
    ' Comment Lines: 68
    '   Blank Lines: 16
    '     File Size: 3.08 KB


    ' 
    ' /********************************************************************************/

#End Region

'#Region "Microsoft.VisualBasic::aaf71878651b3428101866e53df62e57, Data\DataFrame\IO\NetStream\RowObject.vb"

'    ' Author:
'    ' 
'    '       asuka (amethyst.asuka@gcmodeller.org)
'    '       xie (genetics@smrucc.org)
'    '       xieguigang (xie.guigang@live.com)
'    ' 
'    ' Copyright (c) 2018 GPL3 Licensed
'    ' 
'    ' 
'    ' GNU GENERAL PUBLIC LICENSE (GPL3)
'    ' 
'    ' 
'    ' This program is free software: you can redistribute it and/or modify
'    ' it under the terms of the GNU General Public License as published by
'    ' the Free Software Foundation, either version 3 of the License, or
'    ' (at your option) any later version.
'    ' 
'    ' This program is distributed in the hope that it will be useful,
'    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
'    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'    ' GNU General Public License for more details.
'    ' 
'    ' You should have received a copy of the GNU General Public License
'    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



'    ' /********************************************************************************/

'    ' Summaries:

'    '     Class RowObject
'    ' 
'    '         Constructor: (+4 Overloads) Sub New
'    '         Function: CreateObject, Load
'    ' 
'    ' 
'    ' /********************************************************************************/

'#End Region

'Imports Microsoft.VisualBasic.Net.Protocols.Streams.Array
'Imports Microsoft.VisualBasic.Serialization.BinaryDumping
'Imports Microsoft.VisualBasic.Text

'Namespace IO.NetStream

'    Public Class RowObject : Inherits VarArray(Of String)

'        Sub New(encoding As EncodingHelper)
'            Call MyBase.New(
'                AddressOf encoding.GetBytes,
'                AddressOf encoding.ToString)
'        End Sub

'        Sub New(source As IEnumerable(Of String), encoding As Encodings)
'            Call Me.New(New EncodingHelper(encoding))
'            MyBase.Values = source.ToArray
'        End Sub

'        Sub New(source As IEnumerable(Of String), getbyts As IGetBuffer(Of String), toString As IGetObject(Of String))
'            Call MyBase.New(getbyts, toString)
'            MyBase.Values = source.ToArray
'        End Sub

'        Sub New(raw As Byte(), encoding As EncodingHelper)
'            Call MyBase.New(raw,
'                            AddressOf encoding.GetBytes,
'                            AddressOf encoding.ToString)
'        End Sub

'        Public Shared Function CreateObject(raw As Byte(), encoding As Encodings) As RowObject
'            Dim helper As New EncodingHelper(encoding)
'            Return New RowObject(raw, helper)
'        End Function

'        Public Shared Function Load(raw As Byte(), encoding As Encodings) As IO.RowObject
'            Dim source = CreateObject(raw, encoding)
'            Return New IO.RowObject(source.Values)
'        End Function
'    End Class
'End Namespace

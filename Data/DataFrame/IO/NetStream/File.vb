#Region "Microsoft.VisualBasic::168470844f978e8aac2d97891271af91, sciBASIC#\Data\DataFrame\IO\NetStream\File.vb"

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

    '   Total Lines: 85
    '    Code Lines: 0
    ' Comment Lines: 68
    '   Blank Lines: 17
    '     File Size: 2.99 KB


    ' 
    ' /********************************************************************************/

#End Region

'#Region "Microsoft.VisualBasic::5e4083c264b60b59395448bb44146ff4, Data\DataFrame\IO\NetStream\File.vb"

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

'    '     Class File
'    ' 
'    '         Properties: Encoding
'    ' 
'    '         Constructor: (+3 Overloads) Sub New
'    '         Function: CreateObject
'    ' 
'    ' 
'    ' /********************************************************************************/

'#End Region

'Imports Microsoft.VisualBasic.Net.Protocols.Streams.Array
'Imports Microsoft.VisualBasic.Text
'Imports Microsoft.VisualBasic.Linq

'Namespace IO.NetStream

'    Public Class File : Inherits VarArray(Of RowObject)

'        Public ReadOnly Property Encoding As Encodings

'        Sub New(encoding As Encodings)
'            Call MyBase.New(
'                AddressOf StreamHelper.GetBytes, StreamHelper.LoadHelper(encoding))
'            Me.Encoding = encoding
'        End Sub

'        Sub New(raw As Byte(), encoding As Encodings)
'            Call MyBase.New(raw, AddressOf StreamHelper.GetBytes, StreamHelper.LoadHelper(encoding))
'            Me.Encoding = encoding
'        End Sub

'        Sub New(source As IEnumerable(Of IO.RowObject), encoding As Encodings)
'            Call Me.New(encoding)

'            With New EncodingHelper(encoding)
'                Dim [ctype] As Func(Of IO.RowObject, RowObject) =
'                    Function(row) As RowObject
'                        Return New RowObject(row, AddressOf .GetBytes, AddressOf .ToString)
'                    End Function

'                Me.Encoding = encoding
'                Me.Values = source.Select([ctype]).ToArray
'            End With
'        End Sub

'        Public Function CreateObject() As IO.File
'            Return New IO.File(Values.Select(Function(x) New IO.RowObject(x.Values)))
'        End Function
'    End Class
'End Namespace

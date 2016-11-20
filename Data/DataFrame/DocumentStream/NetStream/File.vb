#Region "Microsoft.VisualBasic::33140e190ed383d8318b96e0f8d40b22, ..\sciBASIC#\Data\DataFrame\DocumentStream\NetStream\File.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.Net.Protocols.Streams.Array
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Linq

Namespace DocumentStream.NetStream

    Public Class File : Inherits VarArray(Of RowObject)

        Public ReadOnly Property Encoding As Encodings

        Sub New(encoding As Encodings)
            Call MyBase.New(
                AddressOf StreamHelper.GetBytes, StreamHelper.LoadHelper(encoding))
            Me.Encoding = encoding
        End Sub

        Sub New(raw As Byte(), encoding As Encodings)
            Call MyBase.New(raw,
                            AddressOf StreamHelper.GetBytes, StreamHelper.LoadHelper(encoding))
            Me.Encoding = encoding
        End Sub

        Sub New(source As IEnumerable(Of DocumentStream.RowObject), encoding As Encodings)
            Call Me.New(encoding)

            Dim helper As New EncodingHelper(encoding)
            Me.Values = source.ToArray(
                Function(x) New RowObject(x, AddressOf helper.GetBytes,
                                          AddressOf helper.ToString))
            Me.Encoding = encoding
        End Sub

        Public Function CreateObject() As DocumentStream.File
            Return New DocumentStream.File(Values.ToArray(Function(x) New DocumentStream.RowObject(x.Values)))
        End Function
    End Class
End Namespace

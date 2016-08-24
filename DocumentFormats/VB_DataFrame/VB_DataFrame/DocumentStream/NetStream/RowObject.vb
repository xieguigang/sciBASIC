#Region "Microsoft.VisualBasic::32407aba286fa232710e8956fbeadfe8, ..\visualbasic_App\DocumentFormats\VB_DataFrame\VB_DataFrame\DocumentStream\NetStream\RowObject.vb"

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

Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Net.Protocols.Streams.Array
Imports Microsoft.VisualBasic.Text

Namespace DocumentStream.NetStream

    Public Class RowObject : Inherits VarArray(Of String)

        Sub New(encoding As EncodingHelper)
            Call MyBase.New(
                AddressOf encoding.GetBytes,
                AddressOf encoding.ToString)
        End Sub

        Sub New(source As IEnumerable(Of String), encoding As Encodings)
            Call Me.New(New EncodingHelper(encoding))
            MyBase.Values = source.ToArray
        End Sub

        Sub New(source As IEnumerable(Of String), getbyts As Func(Of String, Byte()), toString As Func(Of Byte(), String))
            Call MyBase.New(getbyts, toString)
            MyBase.Values = source.ToArray
        End Sub

        Sub New(raw As Byte(), encoding As EncodingHelper)
            Call MyBase.New(raw,
                            AddressOf encoding.GetBytes,
                            AddressOf encoding.ToString)
        End Sub

        Public Shared Function CreateObject(raw As Byte(), encoding As Encodings) As RowObject
            Dim helper As New EncodingHelper(encoding)
            Return New RowObject(raw, helper)
        End Function

        Public Shared Function Load(raw As Byte(), encoding As Encodings) As DocumentStream.RowObject
            Dim source = CreateObject(raw, encoding)
            Return New DocumentStream.RowObject(source.Values)
        End Function
    End Class
End Namespace

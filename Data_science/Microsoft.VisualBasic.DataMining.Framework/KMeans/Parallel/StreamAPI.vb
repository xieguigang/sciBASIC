#Region "Microsoft.VisualBasic::08a44be37ecb694d4aea07cc3952ded2, ..\visualbasic_App\Microsoft.VisualBasic.DataMining.Framework\KMeans\Parallel\StreamAPI.vb"

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

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.BinaryDumping

Namespace KMeans.Parallel

    Public Module StreamAPI

        <Extension>
        Public Function GetRaw(source As IEnumerable(Of Entity)) As Byte()
            Return BufferAPI.CreateBuffer(source, AddressOf GetRaw)
        End Function

        <Extension>
        Public Iterator Function GetObjects(raw As Byte()) As IEnumerable(Of Entity)
            Yield BufferAPI.GetBuffer(raw, AddressOf GetObject)
        End Function

        <Extension> Public Function GetRaw(x As Entity) As Byte()
            Dim name As Byte() = Encoding.Unicode.GetBytes(x.uid)
            Dim buffer As Byte() =
                New Byte(name.Length + RawStream.INT32 + (x.Properties.Length * RawStream.DblFloat) - 1) {}
            Dim i As Integer
            Dim nameLen As Byte() = BitConverter.GetBytes(name.Length)

            Call Array.ConstrainedCopy(nameLen, Scan0, buffer, i.Move(nameLen.Length), nameLen.Length)
            Call Array.ConstrainedCopy(name, Scan0, buffer, i.Move(name.Length), name.Length)

            For Each d As Double In x.Properties
                Call Array.ConstrainedCopy(BitConverter.GetBytes(d), Scan0, buffer, i.Move(RawStream.DblFloat), RawStream.DblFloat)
            Next

            Return buffer
        End Function

        <Extension> Public Function GetObject(buffer As Byte()) As Entity
            Dim nameLen As Byte() = New Byte(RawStream.INT32 - 1) {}
            Dim p As Integer
            Call Array.ConstrainedCopy(buffer, p.Move(nameLen.Length), nameLen, Scan0, nameLen.Length)

            Dim name As Byte() = New Byte(BitConverter.ToInt32(nameLen, Scan0) - 1) {}
            Call Array.ConstrainedCopy(buffer, p.Move(name.Length), name, Scan0, name.Length)

            Dim props As Double() =
                buffer.Skip(nameLen.Length + name.Length).Split(RawStream.DblFloat) _
                      .ToArray(Function(buf) BitConverter.ToDouble(buf, Scan0))

            Return New Entity With {
                .Properties = props,
                .uid = Encoding.Unicode.GetString(name)
            }
        End Function
    End Module
End Namespace

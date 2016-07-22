#Region "Microsoft.VisualBasic::54334e09b0f5c345182d8be92a302d33, ..\Microsoft.VisualBasic.Architecture.Framework\Tools\Network\Protocol\Streams\Array.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Imports Microsoft.VisualBasic.Linq.Extensions

Namespace Net.Protocols.Streams.Array

    Public Class [Long] : Inherits ValueArray(Of Long)

        Sub New()
            Call MyBase.New(AddressOf BitConverter.GetBytes, AddressOf __toInt64, Int64, Nothing)
        End Sub

        Sub New(rawStream As Byte())
            Call MyBase.New(AddressOf BitConverter.GetBytes, AddressOf __toInt64, Int64, rawStream)
        End Sub

        Sub New(array As IEnumerable(Of Long))
            Call Me.New
            Values = array.ToArray
        End Sub

        Private Shared Function __toInt64(byts As Byte()) As Long
            Return BitConverter.ToInt64(byts, Scan0)
        End Function
    End Class

    Public Class [Integer] : Inherits ValueArray(Of Integer)

        Sub New()
            Call MyBase.New(AddressOf BitConverter.GetBytes, AddressOf __toInt32, Int32, Nothing)
        End Sub

        Sub New(rawStream As Byte())
            Call MyBase.New(AddressOf BitConverter.GetBytes, AddressOf __toInt32, Int32, rawStream)
        End Sub

        Sub New(array As IEnumerable(Of Integer))
            Call Me.New
            Values = array.ToArray
        End Sub

        Private Shared Function __toInt32(byts As Byte()) As Integer
            Return BitConverter.ToInt32(byts, Scan0)
        End Function
    End Class

    Public Class [Double] : Inherits ValueArray(Of Double)

        Sub New()
            Call MyBase.New(AddressOf BitConverter.GetBytes, AddressOf __toFloat, DblFloat, Nothing)
        End Sub

        Sub New(array As IEnumerable(Of Double))
            Call Me.New
            Values = array.ToArray
        End Sub

        Sub New(rawStream As Byte())
            Call MyBase.New(AddressOf BitConverter.GetBytes, AddressOf __toFloat, DblFloat, rawStream)
        End Sub

        Private Shared Function __toFloat(byts As Byte()) As Double
            Return BitConverter.ToDouble(byts, Scan0)
        End Function
    End Class

    Public Class [Boolean] : Inherits ValueArray(Of Boolean)

        Sub New()
            Call MyBase.New(AddressOf BitConverter.GetBytes, Nothing, 1, Nothing)
        End Sub

        Sub New(rawStream As Byte())
            Call Me.New

            If Not rawStream.IsNullOrEmpty Then
                Me.Values = rawStream.ToArray(Function(byt) __toBoolean(byt))
            Else
                Me.Values = New Boolean() {}
            End If
        End Sub

        Sub New(array As IEnumerable(Of Boolean))
            Call Me.New
            Values = array.ToArray
        End Sub

        Private Shared Function __toBoolean(byt As Byte) As Boolean
            If byt = 0 Then
                Return False
            Else
                Return True
            End If
        End Function
    End Class
End Namespace

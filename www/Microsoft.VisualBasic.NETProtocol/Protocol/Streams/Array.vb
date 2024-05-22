#Region "Microsoft.VisualBasic::c71404ce6ab97101986f8df9397ea506, www\Microsoft.VisualBasic.NETProtocol\Protocol\Streams\Array.vb"

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

    '   Total Lines: 96
    '    Code Lines: 74 (77.08%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 22 (22.92%)
    '     File Size: 2.94 KB


    '     Class [Long]
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: __toInt64
    ' 
    '     Class [Integer]
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: __toInt32
    ' 
    '     Class [Double]
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: __toFloat
    ' 
    '     Class [Boolean]
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: __toBoolean
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq.Extensions

Namespace Protocols.Streams.Array

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
                Me.Values = rawStream _
                    .Select(AddressOf __toBoolean) _
                    .ToArray
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

#Region "Microsoft.VisualBasic::962ccfdd7e020538d869b8d55f9201a4, Microsoft.VisualBasic.Core\src\Data\TypeCast\Type.vb"

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

    '   Total Lines: 83
    '    Code Lines: 61 (73.49%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 22 (26.51%)
    '     File Size: 2.90 KB


    '     Class StringCaster
    ' 
    '         Function: GetBytes, GetString, ParseObject, ToObject
    ' 
    '     Class IntegerCaster
    ' 
    '         Function: GetBytes, GetString, ParseObject, ToObject
    ' 
    '     Class DoubleCaster
    ' 
    '         Function: GetBytes, GetString, ParseObject, ToObject
    ' 
    '     Class DateCaster
    ' 
    '         Function: GetBytes, GetString, ParseObject, ToObject
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.Text

Namespace ComponentModel.DataSourceModel.TypeCast

    Public Class StringCaster : Inherits TypeCaster(Of String)

        ReadOnly utf8 As Encoding = Encodings.UTF8WithoutBOM.CodePage

        Public Overrides Function GetBytes(value As Object) As Byte()
            Return utf8.GetBytes(DirectCast(value, String))
        End Function

        Public Overrides Function GetString(value As Object) As String
            Return value
        End Function

        Public Overrides Function ToObject(bytes() As Byte) As Object
            Return utf8.GetString(bytes, Scan0, bytes.Length)
        End Function

        Public Overrides Function ParseObject(str As String) As Object
            Return str
        End Function

        Public Overrides Function ParseObject(strs As IEnumerable(Of String)) As Array
            Return strs.ToArray
        End Function
    End Class

    Public Class IntegerCaster : Inherits TypeCaster(Of Integer)

        Public Overrides Function GetBytes(value As Object) As Byte()
            Return BitConverter.GetBytes(DirectCast(value, Integer))
        End Function

        Public Overrides Function GetString(value As Object) As String
            Return DirectCast(value, Integer).ToString
        End Function

        Public Overrides Function ToObject(bytes() As Byte) As Object
            Return BitConverter.ToInt32(bytes, Scan0)
        End Function

        Public Overrides Function ParseObject(str As String) As Object
            Return Integer.Parse(str)
        End Function

        Public Overrides Function ParseObject(strs As IEnumerable(Of String)) As Array
            Dim i32 As New List(Of Integer)

            For Each str As String In strs
                Call i32.Add(Integer.Parse(str))
            Next

            Return i32.ToArray
        End Function
    End Class

    Public Class DoubleCaster : Inherits TypeCaster(Of Double)

        Public Overrides Function GetBytes(value As Object) As Byte()
            Return BitConverter.GetBytes(DirectCast(value, Double))
        End Function

        Public Overrides Function GetString(value As Object) As String
            Return DirectCast(value, Double).ToString
        End Function

        Public Overrides Function ToObject(bytes() As Byte) As Object
            Return BitConverter.ToDouble(bytes, Scan0)
        End Function

        Public Overrides Function ParseObject(str As String) As Object
            Return Double.Parse(str)
        End Function

        Public Overrides Function ParseObject(strs As IEnumerable(Of String)) As Array
            Dim f64 As New List(Of Double)

            For Each str As String In strs
                Call f64.Add(Double.Parse(str))
            Next

            Return f64.ToArray
        End Function
    End Class

    Public Class BooleanCaster : Inherits TypeCaster(Of Boolean)

        Public Overrides Function GetBytes(value As Object) As Byte()
            Return New Byte() {If(CBool(value), 1, 0)}
        End Function

        Public Overrides Function GetString(value As Object) As String
            Return value.ToString
        End Function

        Public Overrides Function ToObject(bytes() As Byte) As Object
            If bytes(0) = 0 Then
                Return False
            Else
                Return True
            End If
        End Function

        Public Overrides Function ParseObject(str As String) As Object
            Return str.ParseBoolean
        End Function

        Public Overrides Function ParseObject(strs As IEnumerable(Of String)) As Array
            Dim bools As New List(Of Boolean)

            For Each str As String In strs
                Call bools.Add(str.ParseBoolean)
            Next

            Return bools.ToArray
        End Function
    End Class

    Public Class DateCaster : Inherits TypeCaster(Of Date)

        Public Overrides Function GetBytes(value As Object) As Byte()
            Return BitConverter.GetBytes(DirectCast(value, Date).ToBinary)
        End Function

        Public Overrides Function GetString(value As Object) As String
            Return DirectCast(value, Date).ToString
        End Function

        Public Overrides Function ToObject(bytes() As Byte) As Object
            Return Date.FromBinary(BitConverter.ToInt64(bytes, Scan0))
        End Function

        Public Overrides Function ParseObject(str As String) As Object
            Return Date.Parse(str)
        End Function

        Public Overrides Function ParseObject(strs As IEnumerable(Of String)) As Array
            Dim dates As New List(Of Date)

            For Each str As String In strs
                Call dates.Add(Date.Parse(str))
            Next

            Return dates.ToArray
        End Function
    End Class
End Namespace

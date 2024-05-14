#Region "Microsoft.VisualBasic::e66491ed014a263c6bf2265618254fef, mime\application%pdf\PdfReader\Document\PdfString.vb"

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

    '   Total Lines: 77
    '    Code Lines: 61
    ' Comment Lines: 2
    '   Blank Lines: 14
    '     File Size: 3.00 KB


    '     Class PdfString
    ' 
    '         Properties: ParseString, StrVal, Value, ValueAsBytes, ValueAsDateTime
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: Visit
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System

Namespace PdfReader
    Public Class PdfString
        Inherits PdfObject

        Public ReadOnly Property StrVal As String
            Get
                Return Value
            End Get
        End Property

        Public Sub New(parent As PdfObject, str As ParseString)
            MyBase.New(parent, str)
        End Sub

        Public Overrides Function ToString() As String
            Return Value
        End Function

        Public Overrides Sub Visit(visitor As IPdfObjectVisitor)
            visitor.Visit(Me)
        End Sub

        Public ReadOnly Property ParseString As ParseString
            Get
                Return TryCast(ParseObject, ParseString)
            End Get
        End Property

        Public ReadOnly Property Value As String
            Get
                Return Decrypt.DecodeString(Me)
            End Get
        End Property

        Public ReadOnly Property ValueAsBytes As Byte()
            Get
                Return Decrypt.DecodeStringAsBytes(Me)
            End Get
        End Property

        Public ReadOnly Property ValueAsDateTime As Date
            Get

                Try
                    Dim str = Value

                    If Not Equals(str, Nothing) AndAlso str.Length >= 4 Then
                        Dim index = 0
                        Dim length = str.Length

                        ' The 'D:' prefix is optional
                        If str(index) = "D"c AndAlso str(index + 1) = ":"c Then index += 2

                        ' Year is mandatory, all the others are optional
                        Dim YYYY = Integer.Parse(str.Substring(index, 4))
                        Dim MM = If(index + 4 < length, Integer.Parse(str.Substring(index + 4, 2)), 1)
                        Dim DD = If(index + 6 < length, Integer.Parse(str.Substring(index + 6, 2)), 1)
                        Dim HH = If(index + 7 < length, Integer.Parse(str.Substring(index + 8, 2)), 0)
                        Dim lMm = If(index + 10 < length, Integer.Parse(str.Substring(index + 10, 2)), 0)
                        Dim SS = If(index + 12 < length, Integer.Parse(str.Substring(index + 12, 2)), 0)
                        Dim O = If(index + 14 < length, str(index + 14), "Z"c)
                        Dim OHH = If(index + 15 < length, Integer.Parse(str.Substring(index + 15, 2)), 0)
                        Dim OSS = If(index + 18 < length, Integer.Parse(str.Substring(index + 18, 2)), 0)
                        Return New DateTime(YYYY, MM, DD, HH, lMm, SS, DateTimeKind.Utc)
                    Else
                        Throw New ApplicationException($"String '{Value}' cannot be converted to a date.")
                    End If

                Catch
                    Throw New ApplicationException($"String '{Value}' cannot be converted to a date.")
                End Try
            End Get
        End Property
    End Class
End Namespace

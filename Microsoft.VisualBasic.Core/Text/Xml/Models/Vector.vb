#Region "Microsoft.VisualBasic::cd6aedcd31ecc1c6e54f642a387e4f3c, Microsoft.VisualBasic.Core\Text\Xml\Models\Vector.vb"

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

'     Class NumericVector
' 
'         Properties: Length, Vector
' 
'         Function: ToString
' 
'     Class TermsVector
' 
'         Properties: Terms
' 
'         Function: ToString
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Text.Xml.Models

    ''' <summary>
    ''' A <see cref="Double"/> type numeric sequence container
    ''' </summary>
    Public Class NumericVector

        <XmlAttribute> Public Property name As String
        <XmlAttribute> Public Property vector As Double()

        ''' <summary>
        ''' Get/Set Element ``Xi``
        ''' </summary>
        ''' <param name="i"></param>
        ''' <returns></returns>
        Default Public Property Xi(i As Integer) As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return vector(i)
            End Get
            Set(value As Double)
                vector(i) = value
            End Set
        End Property

        ''' <summary>
        ''' The vector length for counting the elements in <see cref="Vector"/> property.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Length As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return CInt(vector?.Length)
            End Get
        End Property

        Public Overrides Function ToString() As String
            If name.StringEmpty Then
                Return vector.GetJson
            Else
                Return $"Dim {name} As Vector = {vector.GetJson}"
            End If
        End Function
    End Class

    Public Class TermsVector

        <XmlAttribute>
        Public Property Terms As String()

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace

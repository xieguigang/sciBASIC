#Region "Microsoft.VisualBasic::6e8074e94386ad06ad5689987c546916, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Text\Xml\Models\Vector.vb"

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
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Text.Xml.Models

    ''' <summary>
    ''' A <see cref="Double"/> type numeric sequence container
    ''' </summary>
    Public Class NumericVector

        <XmlAttribute> Public Property Vector As Double()

        ''' <summary>
        ''' Get/Set Element ``Xi``
        ''' </summary>
        ''' <param name="i"></param>
        ''' <returns></returns>
        Default Public Property Xi(i As Integer) As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Vector(i)
            End Get
            Set(value As Double)
                Vector(i) = value
            End Set
        End Property

        ''' <summary>
        ''' The vector length for counting the elements in <see cref="Vector"/> property.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Length As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Vector?.Length
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace

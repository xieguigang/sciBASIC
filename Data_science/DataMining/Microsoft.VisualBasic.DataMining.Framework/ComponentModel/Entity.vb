#Region "Microsoft.VisualBasic::ed1c499738ee3d0c8cfcd69dffc8fcac, Data_science\DataMining\Microsoft.VisualBasic.DataMining.Framework\ComponentModel\Entity.vb"

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

    '     Class EntityBase
    ' 
    '         Properties: Length, Properties
    ' 
    '     Class Entity
    ' 
    '         Properties: [Class]
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Language

Namespace ComponentModel

    ''' <summary>
    '''
    ''' </summary>
    ''' <typeparam name="T">只允许数值类型</typeparam>
    Public MustInherit Class EntityBase(Of T) : Inherits BaseClass

        <XmlAttribute("T")>
        Public Overridable Property Properties As T()

        Default Public Property Item(i%) As T
            Get
                Return Properties(i)
            End Get
            Set(value As T)
                Properties(i) = value
            End Set
        End Property

        Public Overridable ReadOnly Property Length As Integer
            Get
                Return Properties.Length
            End Get
        End Property
    End Class

    ''' <summary>
    ''' {Properties} -> Class
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Entity : Inherits EntityBase(Of Integer)

        <XmlAttribute> Public Property [Class] As Integer

        Public Overrides Function ToString() As String
            Return $"<{String.Join("; ", Properties)}> --> {[Class]}"
        End Function

        Default Public Overloads ReadOnly Property Item(Index As Integer) As Integer
            Get
                Return Properties(Index)
            End Get
        End Property

        Public Shared Widening Operator CType(properties As Double()) As Entity
            Return New Entity With {
                .Properties = (From x In properties Select CType(x, Integer)).ToArray
            }
        End Operator

        Public Shared Widening Operator CType(properties As Integer()) As Entity
            Return New Entity With {
                .Properties = properties
            }
        End Operator
    End Class
End Namespace

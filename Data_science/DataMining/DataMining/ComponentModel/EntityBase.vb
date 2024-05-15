#Region "Microsoft.VisualBasic::dd642958c4ea83d9c52b8c0076669875, Data_science\DataMining\DataMining\ComponentModel\EntityBase.vb"

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

    '   Total Lines: 50
    '    Code Lines: 27
    ' Comment Lines: 17
    '   Blank Lines: 6
    '     File Size: 1.60 KB


    '     Class EntityBase
    ' 
    '         Properties: entityVector, Length
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel

    ''' <summary>
    ''' An abstract property vector 
    ''' </summary>
    ''' <typeparam name="T">只允许数值类型</typeparam>
    Public MustInherit Class EntityBase(Of T As {IComparable, IConvertible})

        ''' <summary>
        ''' Properties vector of current entity.
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute("T")>
        Public Overridable Property entityVector As T()

        ''' <summary>
        ''' Get/Set property value by index
        ''' </summary>
        ''' <param name="i"></param>
        ''' <returns></returns>
        Default Public Overloads Property ItemValue(i As Integer) As T
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return entityVector(i)
            End Get
            Set(value As T)
                entityVector(i) = value
            End Set
        End Property

        ''' <summary>
        ''' Length of <see cref="entityVector"/>
        ''' </summary>
        ''' <returns></returns>
        Public Overridable ReadOnly Property Length As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return entityVector.Length
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return entityVector.GetJson
        End Function
    End Class
End Namespace

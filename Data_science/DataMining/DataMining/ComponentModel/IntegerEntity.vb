#Region "Microsoft.VisualBasic::262fc9a8828c6837a47f77f0f6cb0a39, Data_science\DataMining\DataMining\ComponentModel\IntegerEntity.vb"

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

    '   Total Lines: 41
    '    Code Lines: 30 (73.17%)
    ' Comment Lines: 4 (9.76%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (17.07%)
    '     File Size: 1.45 KB


    '     Class IntegerEntity
    ' 
    '         Properties: [Class]
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization

Namespace ComponentModel

    ''' <summary>
    ''' {Properties} -> Class
    ''' </summary>
    ''' <remarks></remarks>
    Public Class IntegerEntity : Inherits EntityBase(Of Integer)
        Implements IClusterPoint

        <XmlAttribute>
        Public Property [Class] As Integer Implements IClusterPoint.Cluster

        Default Public Overloads ReadOnly Property ItemValue(Index As Integer) As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return entityVector(Index)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"<{String.Join("; ", entityVector)}> --> {[Class]}"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(properties As Double()) As IntegerEntity
            Return New IntegerEntity With {
                .entityVector = (From x In properties Select CType(x, Integer)).ToArray
            }
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(properties As Integer()) As IntegerEntity
            Return New IntegerEntity With {
                .entityVector = properties
            }
        End Operator
    End Class
End Namespace

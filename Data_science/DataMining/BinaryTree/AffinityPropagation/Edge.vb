#Region "Microsoft.VisualBasic::ca7aa3750a9b851419ae9922ee1d37e3, Data_science\DataMining\BinaryTree\AffinityPropagation\Edge.vb"

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

    '   Total Lines: 35
    '    Code Lines: 29
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 1.32 KB


    '     Class Edge
    ' 
    '         Properties: Availability, Destination, Responsability, Similarity, Source
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CompareTo, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace AffinityPropagation

    Public Class Edge : Implements IComparable(Of Edge)

        Public Property Source As Integer
        Public Property Destination As Integer
        Public Property Similarity As Double
        Public Property Responsability As Double
        Public Property Availability As Double

        Public Sub New(Source As Integer, Destination As Integer, Similarity As Single)
            Me.Source = Source
            Me.Destination = Destination
            Me.Similarity = Similarity
            Responsability = 0
            Availability = 0
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"[{Source} -> {Destination}] {New Dictionary(Of String, Double) From {
                {"similarity", Similarity},
                {"responsability", Responsability},
                {"availability", Availability}
            }.GetJson }"
        End Function

        Public Function CompareTo(obj As Edge) As Integer Implements IComparable(Of Edge).CompareTo
            Return Similarity.CompareTo(obj.Similarity)
        End Function
    End Class
End Namespace

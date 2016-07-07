#Region "Microsoft.VisualBasic::f6cde7ac14dcffb5efcac952cd140a09, ..\VisualBasic_AppFramework\Microsoft.VisualBasic.DataMining.Framework\Kernel\GeneticAlgorithm\DifferenceInfo.vb"

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

Namespace Kernel.GeneticAlgorithm
    Public Structure DifferenceInfo
        Implements IEquatable(Of DifferenceInfo)
        Implements IComparable(Of DifferenceInfo)
        Private ReadOnly _differenceCount As Integer
        Private ReadOnly _totalDifference As Long

        Public Sub New(differenceCount As Integer, totalDifference As Long)
            _differenceCount = differenceCount
            _totalDifference = totalDifference
        End Sub

        ''' <summary>
        ''' Gets the number of values that didn't had a valid result.
        ''' Note that this property doesn't care about how different the
        ''' values were, it only cares that they were different.
        ''' Also, the normal function considers a single right value
        ''' as all misses and non-returning calls as double misses.
        ''' </summary>
        Public ReadOnly Property DifferenceCount() As Integer
            Get
                Return _differenceCount
            End Get
        End Property

        Public ReadOnly Property TotalDifference() As Long
            Get
                Return _totalDifference
            End Get
        End Property

        Public Overrides Function Equals(obj As Object) As Boolean
            If TypeOf obj Is DifferenceInfo Then
                Return Equals(CType(obj, DifferenceInfo))
            End If

            Return False
        End Function
        Public Overloads Function Equals(other As DifferenceInfo) As Boolean Implements IEquatable(Of DifferenceInfo).Equals
            Return _differenceCount = other._differenceCount AndAlso _totalDifference = other._totalDifference
        End Function
        Public Overrides Function GetHashCode() As Integer
            Return (_differenceCount << 16) Or _totalDifference.GetHashCode()
        End Function

        Public Function CompareTo(other As DifferenceInfo) As Integer Implements IComparable(Of DifferenceInfo).CompareTo
            If _differenceCount <> other._differenceCount Then
                Return _differenceCount.CompareTo(other._differenceCount)
            End If

            Return _totalDifference.CompareTo(other._totalDifference)
        End Function

        Public Shared Operator =(a As DifferenceInfo, b As DifferenceInfo) As Boolean
            Return a.Equals(b)
        End Operator
        Public Shared Operator <>(a As DifferenceInfo, b As DifferenceInfo) As Boolean
            Return Not a.Equals(b)
        End Operator
    End Structure
End Namespace

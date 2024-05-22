#Region "Microsoft.VisualBasic::d0314ed266cab9adfc01c082b90f5475, Data_science\Mathematica\Math\Math.Statistics\Distributions\DiscreteDistribution.vb"

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

    '   Total Lines: 330
    '    Code Lines: 116 (35.15%)
    ' Comment Lines: 170 (51.52%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 44 (13.33%)
    '     File Size: 10.37 KB


    '     Class DiscreteDistribution
    ' 
    '         Function: ContainsItem, Entropy, GetCount, GetIndex, GetItem
    '                   (+2 Overloads) GetMaxItem, GetProbability, GetProbabilityDistribution, GetProbabilityLaplaceSmoothing, GetSum
    '                   GetValue
    ' 
    '         Sub: AddDistribution, AddItem, RemoveDistribution, RemoveItem
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic

Namespace Distributions
    Public Class DiscreteDistribution
        Inherits Dictionary(Of String, Integer)
        Private _sum As Double

        ' *
        '  
        ' <summary>The addItem method takes a String item as an input and if this map contains a mapping for the item it puts the item
        '  with given value + 1, else it puts item with value of 1.</summary>
        ' 
        ' 
        '  
        ' <param name="item">String input.</param>
        ' 
        ' 

        Public Sub AddItem(item As String)
            If ContainsKey(item) Then
                Me(item) = Me(item) + 1
            Else
                Add(item, 1)
            End If

            _sum += 1
        End Sub

        ' *
        '  
        ' <summary>The removeItem method takes a String item as an input and if this map contains a mapping for the item it puts the item
        '  with given value - 1, and if its value is 0, it removes the item.</summary>
        ' 
        ' 
        '  
        ' <param name="item">String input.</param>
        ' 
        ' 

        Public Sub RemoveItem(item As String)
            If ContainsKey(item) Then
                Me(item) = Me(item) - 1
                If Me(item) = 0 Then
                    Remove(item)
                End If
            End If
        End Sub

        ' *
        '  
        ' <summary>The addDistribution method takes a {@link DiscreteDistribution} as an input and loops through the entries in this distribution
        '  and if this map contains a mapping for the entry it puts the entry with its value + entry, else it puts entry with its value.
        '  It also accumulates the values of entries and assigns to the sum variable.</summary>
        ' 
        ' 
        '  
        ' <param name="distribution">{@link DiscreteDistribution} type input.</param>
        ' 
        ' 

        Public Sub AddDistribution(distribution As DiscreteDistribution)
            For Each entry In distribution.Keys
                If ContainsKey(entry) Then
                    Me(entry) = Me(entry) + distribution(entry)
                Else
                    Add(entry, distribution(entry))
                End If

                _sum += distribution(entry)
            Next
        End Sub

        ' *
        '  
        ' <summary>The removeDistribution method takes a {@link DiscreteDistribution} as an input and loops through the entries in this distribution
        '  and if this map contains a mapping for the entry it puts the entry with its key - value, else it removes the entry.
        '  It also decrements the value of entry from sum and assigns to the sum variable.</summary>
        ' 
        ' 
        '  
        ' <param name="distribution">{@link DiscreteDistribution} type input.</param>
        ' 
        ' 

        Public Sub RemoveDistribution(distribution As DiscreteDistribution)
            For Each entry In distribution.Keys
                If Me(entry) - distribution(entry) <> 0 Then
                    Me(entry) = Me(entry) - distribution(entry)
                Else
                    Remove(entry)
                End If

                _sum -= distribution(entry)
            Next
        End Sub

        ' *
        '  
        ' <summary>The getter for sum variable.</summary>
        ' 
        ' 
        '  
        ' <returns>sum.</returns>
        ' 
        ' 

        Public Function GetSum() As Double
            Return _sum
        End Function

        ' *
        '  
        ' <summary>The getIndex method takes an item as an input and returns the index of given item.</summary>
        ' 
        ' 
        '  
        ' <param name="item">to search for index.</param>
        ' 
        '  
        ' <returns>index of given item.</returns>
        ' 
        ' 

        Public Function GetIndex(item As String) As Integer
            Return New List(Of String)(Keys).IndexOf(item)
        End Function

        ' *
        '  
        ' <summary>The containsItem method takes an item as an input and returns true if this map contains a mapping for the
        '  given item.</summary>
        ' 
        ' 
        '  
        ' <param name="item">to check.</param>
        ' 
        '  
        ' <returns>true if this map contains a mapping for the given item.</returns>
        ' 
        ' 

        Public Function ContainsItem(item As String) As Boolean
            Return ContainsKey(item)
        End Function

        ' *
        '  
        ' <summary>The getItem method takes an index as an input and returns the item at given index.</summary>
        ' 
        ' 
        '  
        ' <param name="index">is used for searching the item.</param>
        ' 
        '  
        ' <returns>the item at given index.</returns>
        ' 
        ' 

        Public Function GetItem(index As Integer) As String
            Dim list As New List(Of String)(Keys)
            Return list(index)
        End Function

        ' *
        '  
        ' <summary>The getValue method takes an index as an input and returns the value at given index.</summary>
        ' 
        ' 
        '  
        ' <param name="index">is used for searching the value.</param>
        ' 
        '  
        ' <returns>the value at given index.</returns>
        ' 
        ' 

        Public Function GetValue(index As Integer) As Integer
            Dim list As New List(Of String)(Keys)

            Return Me(list(index))
        End Function

        ' *
        '  
        ' <summary>The getCount method takes an item as an input returns the value to which the specified item is mapped, or {@code null}
        '  if this map contains no mapping for the key.</summary>
        ' 
        ' 
        '  
        ' <param name="item">is used to search for value.</param>
        ' 
        '  
        ' <returns>the value to which the specified item is mapped</returns>
        ' 
        ' 

        Public Function GetCount(item As String) As Integer
            Return Me(item)
        End Function

        ' *
        '  
        ' <summary>The getMaxItem method loops through the entries and gets the entry with maximum value.</summary>
        ' 
        ' 
        '  
        ' <returns>the entry with maximum value.</returns>
        ' 
        ' 

        Public Function GetMaxItem() As String
            Dim max = -1
            Dim maxItem As String = Nothing
            For Each entry In Keys
                If Me(entry) > max Then
                    max = Me(entry)
                    maxItem = entry
                End If
            Next

            Return maxItem
        End Function

        ' *
        '  
        ' <summary>Another getMaxItem method which takes an {@link ArrayList} of Strings. It loops through the items in this {@link ArrayList}
        '  and gets the item with maximum value.</summary>
        ' 
        ' 
        '  
        ' <param name="includeTheseOnly">{@link ArrayList} of Strings.</param>
        ' 
        '  
        ' <returns>the item with maximum value.</returns>
        ' 
        ' 

        Public Function GetMaxItem(includeTheseOnly As List(Of String)) As String
            Dim max = -1
            Dim maxItem As String = Nothing
            For Each item As String In includeTheseOnly
                Dim frequency = 0
                If Me.ContainsItem(item) Then
                    frequency = Me(item)
                End If

                If frequency > max Then
                    max = frequency
                    maxItem = item
                End If
            Next

            Return maxItem
        End Function

        ' *
        '  
        ' <summary>The getProbability method takes an item as an input returns the value to which the specified item is mapped over sum,
        '  or 0.0 if this map contains no mapping for the key.</summary>
        ' 
        ' 
        '  
        ' <param name="item">is used to search for probability.</param>
        ' 
        '  
        ' <returns>the probability to which the specified item is mapped.</returns>
        ' 
        ' 

        Public Function GetProbability(item As String) As Double
            If ContainsKey(item) Then
                Return Me(item) / _sum
            End If

            Return 0.0
        End Function

        Public Function GetProbabilityDistribution() As Dictionary(Of String, Double)
            Dim result = New Dictionary(Of String, Double)()
            For Each item As String In Keys
                result(item) = Me.GetProbability(item)
            Next

            Return result
        End Function


        ' *
        '  
        ' <summary>The getProbabilityLaplaceSmoothing method takes an item as an input returns the smoothed value to which the specified
        '  item is mapped over sum, or 1.0 over sum if this map contains no mapping for the key.</summary>
        ' 
        ' 
        '  
        ' <param name="item">is used to search for probability.</param>
        ' 
        '  
        ' <returns>the smoothed probability to which the specified item is mapped.</returns>
        ' 
        ' 

        Public Function GetProbabilityLaplaceSmoothing(item As String) As Double
            If ContainsKey(item) Then
                Return (Me(item) + 1) / (_sum + Count + 1)
            End If

            Return 1.0 / (_sum + Count + 1)
        End Function

        ' *
        '  
        ' <summary>The entropy method loops through the values and calculates the entropy of these values.</summary>
        ' 
        ' 
        '  
        ' <returns>entropy value.</returns>
        ' 
        ' 

        Public Function Entropy() As Double
            Dim total = 0.0
            For Each count As Integer In Values
                Dim probability = count / _sum
                total += -probability * (System.Math.Log(probability) / System.Math.Log(2))
            Next

            Return total
        End Function
    End Class
End Namespace

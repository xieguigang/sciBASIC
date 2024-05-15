#Region "Microsoft.VisualBasic::22da49f39ae25ec04eaa6a22f7b4bc2b, Data_science\DataMining\DataMining\DecisionTree\Algorithm.vb"

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

    '   Total Lines: 213
    '    Code Lines: 148
    ' Comment Lines: 25
    '   Blank Lines: 40
    '     File Size: 8.99 KB


    '     Module Algorithm
    ' 
    '         Function: CalculateTableEntropy, CheckIfIsLeaf, CountKnownValues, CreateSmallerTable, GetAmountOfEdgesAndTotalPositivResults
    '                   GetGainForAllAttributes, GetRootNode, Learn
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.DecisionTree.Data
Imports Microsoft.VisualBasic.Math.Information
Imports stdNum = System.Math

Namespace DecisionTree

    ''' <summary>
    ''' Algorithm module for train a new decision tree model
    ''' </summary>
    Module Algorithm

        ''' <summary>
        ''' Create tree of <see cref="Tree.root"/>
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="edgeName"></param>
        ''' <returns></returns>
        Public Function Learn(data As DataTable, Optional edgeName As String = "") As TreeNode
            Dim root As TreeNode = data.GetRootNode(edgeName)
            Dim reducedTable As DataTable

            For Each item As String In root.attributes.differentAttributeNames
                ' if a leaf, leaf will be added in this method
                Dim isLeaf = CheckIfIsLeaf(root, data, item)

                ' make a recursive call as long as the node is not a leaf
                If Not isLeaf Then
                    reducedTable = CreateSmallerTable(data, item, root.index)
                    root.childNodes.Add(Learn(reducedTable, item))
                End If
            Next

            Return root
        End Function

        Private Function CheckIfIsLeaf(root As TreeNode, data As DataTable, attributeToCheck As String) As Boolean
            Dim isLeaf = True
            Dim allEndValues As New List(Of String)()

            ' get all leaf values for the attribute in question
            For i As Integer = 0 To data.rows.Count - 1
                If data.rows(i)(root.index).Equals(attributeToCheck) Then
                    allEndValues.Add(data.rows(i).decisions)
                End If
            Next

            ' check whether all elements of the list have the same value
            If allEndValues.Count > 0 AndAlso allEndValues.Any(Function(x) x <> allEndValues(0)) Then
                isLeaf = False
            End If

            ' create leaf with value to display and edge to the leaf
            If isLeaf Then
                root.childNodes.Add(New TreeNode(True, allEndValues(0), attributeToCheck))
            End If

            Return isLeaf
        End Function

        Private Function CreateSmallerTable(data As DataTable, edgePointingToNextNode As String, rootTableIndex As Integer) As DataTable
            ' create a new empty data table object
            ' add column titles
            Dim smallerData As New DataTable() With {
                .headers = data.headers.ToArray
            }
            Dim rows As New List(Of Entity)

            ' add rows which contain edgePointingToNextNode to new datatable
            For i As Integer = 0 To data.rows.Count - 1
                If data.rows(i)(rootTableIndex).Equals(edgePointingToNextNode) Then
                    Dim row As New Entity With {
                        .entityVector = data.rows(i).entityVector.ToArray
                    }

                    Call rows.Add(row)
                End If
            Next

            ' remove column which was already used as node       
            smallerData.rows = rows.ToArray
            smallerData.RemoveColumn(rootTableIndex)

            Return smallerData
        End Function

        <Extension>
        Private Function GetRootNode(data As DataTable, edge As String) As TreeNode
            Dim attributes As New List(Of Attributes)()
            Dim highestInformationGainIndex = -1
            Dim highestInformationGain = Double.MinValue
            Dim differentAttributenames As String()

            ' Get all names, amount of attributes and attributes for every column             
            For i As Integer = 0 To data.columns - 2
                differentAttributenames = DecisionTree.Attributes.GetDifferentAttributeNamesOfColumn(data, i)
                attributes.Add(New Attributes(data.headers(i), differentAttributenames))
            Next

            ' Calculate Entropy (S)
            Dim tableEntropy As Double = CalculateTableEntropy(data)

            For i As Integer = 0 To attributes.Count - 1
                attributes(i).informationGain = GetGainForAllAttributes(data, i, tableEntropy)

                If attributes(i).informationGain > highestInformationGain Then
                    highestInformationGain = attributes(i).informationGain
                    highestInformationGainIndex = i
                End If
            Next

            If highestInformationGainIndex = -1 Then
                Return New TreeNode(isleaf:=True, name:=data.headers(Scan0), edge:=edge) With {
                    .attributes = New Attributes With {
                        .differentAttributeNames = {},
                        .informationGain = highestInformationGain,
                        .name = ""
                    }
                }
            Else
                Return New TreeNode(
                    name:=attributes(highestInformationGainIndex).name,
                    tableIndex:=highestInformationGainIndex,
                    attributes:=attributes(highestInformationGainIndex),
                    edge:=edge
                )
            End If
        End Function

        Private Function GetGainForAllAttributes(data As DataTable, colIndex As Integer, entropyOfDataset As Double) As Double
            Dim totalRows = data.rows.Length
            Dim amountForDifferentValue = GetAmountOfEdgesAndTotalPositivResults(data, colIndex)
            Dim stepsForCalculation As New List(Of Double)()

            For Each item As Integer(,) In amountForDifferentValue
                ' helper for calculation
                Dim firstDivision = item(0, 1) / CDbl(item(0, 0))
                Dim secondDivision = (item(0, 0) - item(0, 1)) / CDbl(item(0, 0))

                ' prevent dividedByZeroException
                If firstDivision = 0.0 OrElse secondDivision = 0.0 Then
                    stepsForCalculation.Add(0.0)
                Else
                    stepsForCalculation.Add(-firstDivision * stdNum.Log(firstDivision, 2) - secondDivision * stdNum.Log(secondDivision, 2))
                End If
            Next

            Dim gain = stepsForCalculation.[Select](Function(t, i) amountForDifferentValue(i)(0, 0) / CDbl(totalRows) * t).Sum()
            gain = entropyOfDataset - gain

            Return gain
        End Function

        Private Function CalculateTableEntropy(data As DataTable) As Double
            Dim totalRows As Integer = data.rows.Length
            Dim amountForDifferentValue = data.GetAmountOfEdgesAndTotalPositivResults(data.columns - 1)
            Dim H As Double = amountForDifferentValue _
                .Select(Function(item) item(0, 0) / CDbl(totalRows)) _
                .ShannonEntropy

            Return H
        End Function

        <Extension>
        Private Function GetAmountOfEdgesAndTotalPositivResults(data As DataTable, indexOfColumnToCheck As Integer) As List(Of Integer(,))
            Dim foundValues As New List(Of Integer(,))()
            Dim knownValues = CountKnownValues(data, indexOfColumnToCheck)
            Dim array As Integer(,)

            For Each item As String In knownValues
                Dim amount = 0
                Dim positiveAmount = 0

                For i As Integer = 0 To data.rows.Length - 1
                    If data.rows(i)(indexOfColumnToCheck).Equals(item) Then
                        amount += 1

                        ' Counts the positive cases and adds the sum later 
                        ' to the array for the calculation
                        If data.rows(i).decisions.Equals(data.rows(0).decisions) Then
                            positiveAmount += 1
                        End If
                    End If
                Next

                array = {{amount, positiveAmount}}
                foundValues.Add(array)
            Next

            Return foundValues
        End Function

        Private Function CountKnownValues(data As DataTable, indexOfColumnToCheck As Integer) As IEnumerable(Of String)
            Dim knownValues As New List(Of String)()

            ' add the value of the first row to the list
            If data.rows.Length > 0 Then
                knownValues.Add(data.rows(0)(indexOfColumnToCheck))
            End If

            For j As Integer = 1 To data.rows.Length - 1
                Dim index = j
                Dim newValue = knownValues.All(Function(item) Not data.rows(index)(indexOfColumnToCheck).Equals(item))

                If newValue Then
                    knownValues.Add(data.rows(j)(indexOfColumnToCheck))
                End If
            Next

            Return knownValues
        End Function
    End Module
End Namespace

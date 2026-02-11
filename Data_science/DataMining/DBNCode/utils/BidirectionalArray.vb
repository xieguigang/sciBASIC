#Region "Microsoft.VisualBasic::968e95bde615aa86de03710ccdeb199e, Data_science\DataMining\DBNCode\utils\BidirectionalArray.vb"

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

    '   Total Lines: 46
    '    Code Lines: 30 (65.22%)
    ' Comment Lines: 9 (19.57%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (15.22%)
    '     File Size: 1.71 KB


    '     Class BidirectionalArray
    ' 
    '         Function: [get], add, containsValue, getIndex, size
    '                   ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace utils

    ''' <summary>
    ''' Implements an array that is also indexed by the values, which means that it
    ''' takes O(1) time to access an item both by its index and by its value.
    ''' Removing items is not possible. Adapted from
    ''' http://stackoverflow.com/a/7834138
    ''' </summary>
    ''' @param <T>
    '''            array type </param>
    Public Class BidirectionalArray(Of T)
        Private indexToValueMap As IList(Of T) = New List(Of T)()
        Private valueToIndexMap As IDictionary(Of T, Integer) = New Dictionary(Of T, Integer)()

        ''' <returns> false if the value is already present </returns>
        Public Overridable Function add(value As T) As Boolean
            If containsValue(value) Then
                Return False
            End If
            Dim size = indexToValueMap.Count
            indexToValueMap.Insert(size, value)
            valueToIndexMap(value) = size
            Return True
        End Function

        Public Overridable Function containsValue(value As T) As Boolean
            Return valueToIndexMap.ContainsKey(value)
        End Function

        Public Overridable Function getIndex(value As T) As Integer
            Return valueToIndexMap(value)
        End Function

        Public Overridable Function [get](index As Integer) As T
            Return indexToValueMap(index)
        End Function

        Public Overridable Function size() As Integer
            Return indexToValueMap.Count
        End Function

        Public Overrides Function ToString() As String
            Return "" & indexToValueMap.ToString()
        End Function
    End Class
End Namespace


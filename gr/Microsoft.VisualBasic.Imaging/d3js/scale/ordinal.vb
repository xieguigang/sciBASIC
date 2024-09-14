#Region "Microsoft.VisualBasic::ce8e399d9b54a81948430e01e198f734, gr\Microsoft.VisualBasic.Imaging\d3js\scale\ordinal.vb"

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

    '   Total Lines: 178
    '    Code Lines: 91 (51.12%)
    ' Comment Lines: 64 (35.96%)
    '    - Xml Docs: 90.62%
    ' 
    '   Blank Lines: 23 (12.92%)
    '     File Size: 6.94 KB


    '     Class OrdinalScale
    ' 
    '         Properties: binWidth, domainSize, type, Zero
    ' 
    '         Function: (+3 Overloads) domain, getTerms, lazyPositions, (+2 Overloads) range
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Linq

Namespace d3js.scale

    ''' <summary>
    ''' Unlike continuous scales, ordinal scales have a discrete domain and range. 
    ''' For example, an ordinal scale might map a set of named categories to a 
    ''' set of colors, or determine the horizontal positions of columns in a column 
    ''' chart.
    ''' (相当于cytoscape之中的离散型映射)
    ''' </summary>
    Public Class OrdinalScale : Inherits IScale(Of OrdinalScale)

        ' Dim factors As Factor(Of String)()
        Dim index As Index(Of String)
        Dim positions As Double()

        ''' <summary>
        ''' count of the term factors
        ''' </summary>
        ''' <returns></returns>
        Public Overrides ReadOnly Property domainSize As Double
            Get
                Return index.Count
            End Get
        End Property

        Public ReadOnly Property binWidth As Double
            Get
                Dim pos = lazyPositions()

                If pos.Length > 1 Then
                    Return pos(1) - pos(0)
                Else
                    Return _range.Length * 0.85
                End If
            End Get
        End Property

        Default Public Overrides ReadOnly Property Value(x As Double) As Double
            Get
                Return Me(x.ToString)
            End Get
        End Property

        Public Overrides ReadOnly Property Zero As Double
            Get
                Return _range.Min
            End Get
        End Property

        Default Public Overrides ReadOnly Property Value(term As String) As Double
            Get
                If Not index.NotExists(term) Then
                    Dim i As Integer = index(term)
                    Dim val As Double = lazyPositions(i)

                    Return val
                Else
                    Throw New MissingMemberException($"missing ordinal mapping: {term}!")
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property type As scalers
            Get
                Return scalers.ordinal
            End Get
        End Property

        Private Function lazyPositions() As Double()
            If positions.IsNullOrEmpty Then
                Dim delta As Double = _range.Length / index.Count
                Dim list As New List(Of Double)
                Dim x As Double = _range.Min + delta / 2

                For i As Integer = 0 To index.Count - 1
                    list.Add(x)
                    x += delta
                Next

                positions = list.ToArray
            End If

            Return positions
        End Function

        ''' <summary>
        ''' set the plot canvas region boundary
        ''' </summary>
        ''' <param name="values"></param>
        ''' <returns></returns>
        Public Overrides Function range(Optional values As IEnumerable(Of Double) = Nothing) As OrdinalScale
            _range = values.Range
            Return Me
        End Function

        ''' <summary>
        ''' set the plot canvas region boundary
        ''' </summary>
        ''' <param name="min"></param>
        ''' <param name="max"></param>
        ''' <returns></returns>
        Public Overloads Function range(min As Double, max As Double) As OrdinalScale
            _range = New DoubleRange(min, max)
            Return Me
        End Function

        Public Function getTerms() As Index(Of String)
            Return index
        End Function

        ''' <summary>
        ''' set the ordinal value range, the input numeric value will be transform as the string term factors in this function.
        ''' </summary>
        ''' <param name="values"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' the term ordinal value could be get from the <see cref="getTerms"/> function.
        ''' </remarks>
        Public Overrides Function domain(values As IEnumerable(Of Double)) As OrdinalScale
            Return domain(values.ToStringArray)
        End Function

        ''' <summary>
        ''' If domain is specified, sets the domain to the specified array of values. 
        ''' The first element in domain will be mapped to the first element in the range, 
        ''' the second domain value to the second range value, and so on. Domain values 
        ''' are stored internally in a map from stringified value to index; the resulting 
        ''' index is then used to retrieve a value from the range. Thus, an ordinal scale’s 
        ''' values must be coercible to a string, and the stringified version of the domain 
        ''' value uniquely identifies the corresponding range value. If domain is not specified, 
        ''' this method returns the current domain.
        '''
        ''' Setting the domain On an ordinal scale Is Optional If the unknown value Is implicit 
        ''' (the Default). In this Case, the domain will be inferred implicitly from usage by 
        ''' assigning Each unique value passed To the scale a New value from the range. Note 
        ''' that an explicit domain Is recommended To ensure deterministic behavior, As inferring 
        ''' the domain from usage will be dependent On ordering.
        ''' </summary>
        ''' <param name="tags"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' the term ordinal value could be get from the <see cref="getTerms"/> function.
        ''' </remarks>
        Public Overrides Function domain(tags As IEnumerable(Of String)) As OrdinalScale
            ' factors = values.factors
            'index = factors _
            '    .Select(Function(x) x.FactorValue) _
            '    .Indexing
            index = tags.Distinct.Indexing

            Return Me
        End Function

        ''' <summary>
        ''' set the ordinal value range, the input integer value will be transform as the string term factors in this function.
        ''' </summary>
        ''' <param name="values"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' the term ordinal value could be get from the <see cref="getTerms"/> function.
        ''' </remarks>
        Public Overrides Function domain(values As IEnumerable(Of Integer)) As OrdinalScale
            Return domain(values.ToStringArray)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(ordinal As OrdinalScale) As Double()
            Return ordinal._range _
                .AsEnumerable _
                .ToArray
        End Operator
    End Class
End Namespace

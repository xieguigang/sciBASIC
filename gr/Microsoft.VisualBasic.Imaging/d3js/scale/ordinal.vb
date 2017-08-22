#Region "Microsoft.VisualBasic::5139c06b49f70c812c086708b9f4a63d, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\d3js\scale\ordinal.vb"

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

Namespace d3js.scale

    ''' <summary>
    ''' Unlike continuous scales, ordinal scales have a discrete domain and range. 
    ''' For example, an ordinal scale might map a set of named categories to a 
    ''' set of colors, or determine the horizontal positions of columns in a column 
    ''' chart.
    ''' (相当于cytoscape之中的离散型映射)
    ''' </summary>
    Public Class OrdinalScale : Inherits IScale

        Public Overrides Function domain(values() As Double) As OrdinalScale
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
        ''' <param name="values"></param>
        ''' <returns></returns>
        Public Overrides Function domain(values() As String) As OrdinalScale
            Throw New NotImplementedException()
        End Function

        Public Overrides Function domain(values() As Integer) As OrdinalScale
            Return domain(values.ToStringArray)
        End Function

        Public Overrides Function rangeBands() As OrdinalScale
            Throw New NotImplementedException()
        End Function

        ''' <summary>
        ''' If range is specified, sets the range of the ordinal scale to the specified array of values. 
        ''' The first element in the domain will be mapped to the first element in range, the second 
        ''' domain value to the second range value, and so on. If there are fewer elements in the range 
        ''' than in the domain, the scale will reuse values from the start of the range. If range is 
        ''' not specified, this method returns the current range.
        ''' </summary>
        ''' <param name="values"></param>
        ''' <returns></returns>
        Public Overrides Function range(Optional values() As Double = Nothing) As OrdinalScale
            Throw New NotImplementedException()
        End Function

        Public Shared Narrowing Operator CType(ordinal As OrdinalScale) As Double()

        End Operator
    End Class
End Namespace

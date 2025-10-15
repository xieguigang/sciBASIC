#Region "Microsoft.VisualBasic::e3c533b2c9d73a770771dcfa676c2134, Data_science\Mathematica\Math\Math.Statistics\ShapleyValue\ShapleyApplication.vb"

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

    '   Total Lines: 54
    '    Code Lines: 8 (14.81%)
    ' Comment Lines: 39 (72.22%)
    '    - Xml Docs: 89.74%
    ' 
    '   Blank Lines: 7 (12.96%)
    '     File Size: 2.12 KB


    '     Interface ShapleyApplication
    ' 
    '         Properties: LastCoalitionReached
    ' 
    '         Function: (+3 Overloads) calculate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ShapleyValue

    ''' <summary>
    ''' facade of Shapley application
    ''' 
    ''' @author Franck Benault
    ''' 
    ''' @version	0.0.2
    ''' @since 0.0.2
    ''' 
    ''' </summary>
    Public Interface ShapleyApplication

        ''' <summary>
        ''' full calculation of the Shapley value
        ''' </summary>
        ''' <returns> partial result of the Shapley value </returns>
        Function calculate() As IDictionary(Of String, Double)

        ''' <summary>
        ''' complete the calculation of the Shapley value 
        ''' with the next set of coalitions choosen sequentially 
        ''' </summary>
        ''' <param name="nbCoalitions"> number of coalition taken in account for this calculation
        ''' </param>
        ''' <returns> partial result of the Shapley value
        ''' </returns>
        ''' <exception cref="Exception"> when different strategy are mixed
        '''  </exception>
        Function calculate(nbCoalitions As Long) As IDictionary(Of String, Double)

        ''' <summary>
        ''' complete the calculation of the Shapley value 
        ''' with the next set of coalitions which may be choosen randomly
        ''' </summary>
        ''' <param name="nbCoalitions"> number of coalition taken in account for this calculation </param>
        ''' <param name="strategy"> way to choose the next coalitions (sequential or random)  
        ''' </param>
        ''' <returns> partial result of the Shapley value
        ''' </returns>
        ''' <exception cref="Exception"> when different strategy are mixed
        '''  </exception>
        Function calculate(nbCoalitions As Long, strategy As CoalitionStrategy) As IDictionary(Of String, Double)

        ''' <summary>
        ''' check in the case of sequential strategy if the last coalition possible is reached
        ''' 
        ''' @return </summary>
        ''' <exception cref="Exception"> in the cas of non sequential strategy </exception>
        ReadOnly Property LastCoalitionReached As Boolean

    End Interface

End Namespace

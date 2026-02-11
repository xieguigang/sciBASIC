#Region "Microsoft.VisualBasic::cbeae9a141d78e0effbb32c0c44fb4a9, Data_science\DataMining\DBNCode\dbn\Attribute.vb"

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

    '   Total Lines: 58
    '    Code Lines: 12 (20.69%)
    ' Comment Lines: 34 (58.62%)
    '    - Xml Docs: 91.18%
    ' 
    '   Blank Lines: 12 (20.69%)
    '     File Size: 2.44 KB


    '     Interface Attribute
    ' 
    '         Properties: Name, Nominal, Numeric
    ' 
    '         Function: [get], add, getIndex, size, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace dbn
    ''' 
    ''' <summary>
    ''' An attribute is a representation of a random variable in a DBN. It can be
    ''' numeric (discrete valued) or nominal and takes a finite number of different
    ''' values. Values are indexed by sequential integers, which are used for
    ''' representing them.
    ''' 
    ''' @author josemonteiro
    ''' 
    ''' </summary>

    Public Interface Attribute

        ''' <summary>
        ''' Function that says if an attribute is Numeric. </summary>
        ''' <returns> boolean Returns true if the attribute is numeric. </returns>
        ReadOnly Property Numeric As Boolean

        ''' <summary>
        ''' Function that says if an attribute is Nominal. </summary>
        ''' <returns> boolean Returns true if the attribute is nominal. </returns>
        ReadOnly Property Nominal As Boolean

        ''' <summary>
        ''' Function that returns the number of possible values that an attribute can assume. </summary>
        ''' <returns> int the number of possible values that an attribute can assume. </returns>
        Function size() As Integer

        ''' <summary>
        ''' Function that returns the corresponding value of an attribute. </summary>
        ''' <param name="index"> The index of the corresponding value. </param>
        ''' <returns> String Corresponding value that the attribute assume. </returns>
        Function [get](index As Integer) As String

        ''' <summary>
        ''' Function that returns the corresponding index of an value that the attribute assume. </summary>
        ''' <param name="value"> Value that the attribute. </param>
        ''' <returns> int Corresponding index of the assumed value. </returns>
        Function getIndex(value As String) As Integer

        ''' <summary>
        ''' Adds a new value that the attribute assumes. </summary>
        ''' <param name="value"> Corresponding value that the attribute assumes. </param>
        ''' <returns> boolean Returns true if the values is added and false if the attribute already has this value. </returns>
        Function add(value As String) As Boolean

        Function ToString() As String

        ''' <summary>
        ''' Setter for the name of the attribute. </summary>
        ''' <param name="name"> Name of the attribute. </param>
        Property Name As String


    End Interface

End Namespace


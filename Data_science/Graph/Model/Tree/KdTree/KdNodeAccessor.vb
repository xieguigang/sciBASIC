#Region "Microsoft.VisualBasic::fa5b3394adfdf3433efd9aa46d1dc435, Data_science\Graph\Model\Tree\KdTree\KdNodeAccessor.vb"

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

    '   Total Lines: 40
    '    Code Lines: 18 (45.00%)
    ' Comment Lines: 16 (40.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (15.00%)
    '     File Size: 1.38 KB


    '     Class KdNodeAccessor
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace KdTree

    ''' <summary>
    ''' Helper class for access the node data by different dimensions
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public MustInherit Class KdNodeAccessor(Of T)

        Default Public Property DimensionAccess(x As T, dimName As String) As Double
            Get
                Return getByDimension(x, dimName)
            End Get
            Set(value As Double)
                Call setByDimension(x, dimName, value)
            End Set
        End Property

        Public MustOverride Function GetDimensions() As String()
        ''' <summary>
        ''' measuring of the node distance
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Public MustOverride Function metric(a As T, b As T) As Double
        Public MustOverride Function getByDimension(x As T, dimName As String) As Double
        Public MustOverride Sub setByDimension(x As T, dimName As String, value As Double)

        ''' <summary>
        ''' test node equals?
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Public MustOverride Function nodeIs(a As T, b As T) As Boolean

        ''' <summary>
        ''' create a new instance of target object
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride Function activate() As T

    End Class
End Namespace

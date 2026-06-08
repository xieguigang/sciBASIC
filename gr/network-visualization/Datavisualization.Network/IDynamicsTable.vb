#Region "Microsoft.VisualBasic::06477c8750607bdb0e4890208860adde, gr\network-visualization\Datavisualization.Network\IDynamicsTable.vb"

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

    '   Total Lines: 26
    '    Code Lines: 13 (50.00%)
    ' Comment Lines: 10 (38.46%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (11.54%)
    '     File Size: 906 B


    '     Class IDynamicsTable
    ' 
    '         Properties: Properties
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace FileStream.Generic

    ''' <summary>
    ''' The network graph object contains the dynamics property for contains the extra information of the object.
    ''' </summary>
    ''' <remarks>
    ''' contains a string value dictionary object
    ''' </remarks>
    Public MustInherit Class IDynamicsTable : Inherits DynamicPropertyBase(Of String)

        ''' <summary>
        ''' The dynamics property table of this network component
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Property Properties As Dictionary(Of String, String)
            Get
                Return MyBase.Properties
            End Get
            Set(value As Dictionary(Of String, String))
                MyBase.Properties = value
            End Set
        End Property
    End Class
End Namespace

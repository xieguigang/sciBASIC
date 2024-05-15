#Region "Microsoft.VisualBasic::a051d109f84ade76419bad0e8255b728, Data_science\MachineLearning\MachineLearning\ComponentModel\DataSet\SampleHelper.vb"

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

    '   Total Lines: 20
    '    Code Lines: 10
    ' Comment Lines: 6
    '   Blank Lines: 4
    '     File Size: 603 B


    '     Module SampleHelper
    ' 
    '         Function: dimension
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace ComponentModel.StoreProcedure

    <HideModuleName>
    Public Module SampleHelper

        ''' <summary>
        ''' get feature dimension
        ''' </summary>
        ''' <param name="samples"></param>
        ''' <param name="[dim]"></param>
        ''' <returns></returns>
        <Extension>
        Public Function dimension(samples As IEnumerable(Of SampleData), [dim] As Integer) As IEnumerable(Of Double)
            Return samples.Select(Function(si) si.features([dim]))
        End Function

    End Module
End Namespace

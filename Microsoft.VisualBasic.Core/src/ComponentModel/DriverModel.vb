#Region "Microsoft.VisualBasic::adcad34ffc4db772e17534e416b8591c, Microsoft.VisualBasic.Core\src\ComponentModel\DriverModel.vb"

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

    '   Total Lines: 14
    '    Code Lines: 5
    ' Comment Lines: 7
    '   Blank Lines: 2
    '     File Size: 324 B


    '     Interface ITaskDriver
    ' 
    '         Function: Run
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel

    ''' <summary>
    ''' Driver abstract model
    ''' </summary>
    Public Interface ITaskDriver

        ''' <summary>
        ''' Start run this driver object.
        ''' </summary>
        ''' <returns></returns>
        Function Run() As Integer
    End Interface
End Namespace

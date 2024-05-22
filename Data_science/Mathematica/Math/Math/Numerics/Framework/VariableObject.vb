#Region "Microsoft.VisualBasic::9f9f5fc9a4ee76fd8c055f5bdf6d441b, Data_science\Mathematica\Math\Math\Numerics\Framework\VariableObject.vb"

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
    '    Code Lines: 12 (60.00%)
    ' Comment Lines: 4 (20.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (20.00%)
    '     File Size: 666 B


    '     Class VariableObject
    ' 
    '         Properties: Id
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Language

Namespace Framework

    Public Class VariableObject : Inherits f64
        Implements INamedValue

        ''' <summary>
        ''' the unique id of current object variable 
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Property Id As String Implements IKeyedEntity(Of String).Key

        Public Overrides Function ToString() As String
            Return $"Dim {Id} As f64 = {Value}"
        End Function
    End Class
End Namespace

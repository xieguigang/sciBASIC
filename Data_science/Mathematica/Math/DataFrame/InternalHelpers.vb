#Region "Microsoft.VisualBasic::93b12635b715e4be73e1991fa1bde011, Data_science\Mathematica\Math\DataFrame\InternalHelpers.vb"

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
    '    Code Lines: 14 (70.00%)
    ' Comment Lines: 3 (15.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (15.00%)
    '     File Size: 926 B


    ' Module InternalHelpers
    ' 
    '     Function: PropertyNames, Vector
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' 这个模块是在删除了csv文件模块的依赖之后的进行重复实现某些csv文件模块的逻辑函数的帮助模块
''' </summary>
Friend Module InternalHelpers

    <Extension>
    Friend Function PropertyNames(Of DataSet As {INamedValue, DynamicPropertyBase(Of Double)})(data As DataSet()) As String()
        Return data.Select(Function(r) r.Properties.Keys).IteratesALL.Distinct.ToArray
    End Function

    <Extension>
    Friend Function Vector(Of DataSet As {INamedValue, DynamicPropertyBase(Of Double)})(data As IEnumerable(Of DataSet), name As String) As IEnumerable(Of Double)
        Return data.Select(Function(r) r.Properties(name))
    End Function
End Module

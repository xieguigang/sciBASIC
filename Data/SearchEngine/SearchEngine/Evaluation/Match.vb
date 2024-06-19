#Region "Microsoft.VisualBasic::de9dd79d4b0233a4da83179a6803246a, sciBASIC#\Data\SearchEngine\SearchEngine\Evaluation\Match.vb"

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

    '   Total Lines: 38
    '    Code Lines: 22
    ' Comment Lines: 10
    '   Blank Lines: 6
    '     File Size: 1.01 KB


    ' Structure Match
    ' 
    '     Properties: Success, x
    ' 
    '     Function: ToString
    ' 
    ' Delegate Function
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.Serialization
Imports System.Web.Script.Serialization
Imports KeyTupleValue = Microsoft.VisualBasic.ComponentModel.DataSourceModel.NamedValue(Of System.String)

''' <summary>
''' 匹配结果
''' </summary>
''' 
<KnownType(GetType(KeyTupleValue))>
Public Structure Match

    <ScriptIgnore>
    Public Property x As Object
    Dim score#
    Dim Field As KeyTupleValue

    Public ReadOnly Property Success As Boolean
        Get
            Return score > 0R
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return Field.ToString
    End Function

    Public Shared Narrowing Operator CType(m As Match) As Boolean
        Return m.Success
    End Operator
End Structure

''' <summary>
''' 调用这个方法计算出匹配结果
''' </summary>
''' <param name="def">数据定义缓存</param>
''' <param name="obj">数据实体</param>
''' <returns></returns>
Public Delegate Function IAssertion(def As IObject, obj As Object) As Match

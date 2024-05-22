#Region "Microsoft.VisualBasic::e5a222b781d498a1d810a60f4396b7ea, Microsoft.VisualBasic.Core\src\CommandLine\Reflection\Attributes\Grouping.vb"

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
    '    Code Lines: 21 (52.50%)
    ' Comment Lines: 12 (30.00%)
    '    - Xml Docs: 83.33%
    ' 
    '   Blank Lines: 7 (17.50%)
    '     File Size: 1.21 KB


    '     Class GroupAttribute
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '     Class GroupingDefineAttribute
    ' 
    '         Properties: Description
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace CommandLine.Reflection

    ''' <summary>
    ''' 应用于方法之上的，标注当前的这个方法的功能分组
    ''' </summary>
    ''' 
    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=True, Inherited:=True)>
    Public Class GroupAttribute : Inherits CLIToken

        Public Sub New(name As String)
            MyBase.New(name)
        End Sub

        Public Sub New(name As System.Enum)
            MyBase.New(name.Description)
        End Sub
    End Class

    ''' <summary>
    ''' 应用于命令行类型容器之上的，用于功能分组的详细描述信息
    ''' </summary>
    ''' 
    <AttributeUsage(AttributeTargets.Class, AllowMultiple:=True, Inherited:=True)>
    Public Class GroupingDefineAttribute : Inherits GroupAttribute

        ''' <summary>
        ''' 当前的这一功能分组的详细描述信息
        ''' </summary>
        ''' <returns></returns>
        Public Property Description As String

        Public Sub New(name As String)
            MyBase.New(name)
        End Sub

        Public Sub New(name As System.Enum)
            MyBase.New(name)
        End Sub
    End Class
End Namespace

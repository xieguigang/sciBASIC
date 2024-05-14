#Region "Microsoft.VisualBasic::b74918c1abe9b30f63774f8b47e27455, Microsoft.VisualBasic.Core\src\CommandLine\Reflection\Attributes\LastUpdatedAttribute.vb"

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

    '   Total Lines: 27
    '    Code Lines: 18
    ' Comment Lines: 3
    '   Blank Lines: 6
    '     File Size: 814 B


    '     Class LastUpdatedAttribute
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace CommandLine.Reflection

    ''' <summary>
    ''' 主要是用于帮助标记命令行命令的更新时间,了解哪些命令可能是已经过时了的
    ''' </summary>
    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=False, Inherited:=True)>
    Public Class LastUpdatedAttribute : Inherits Attribute

        Public ReadOnly [Date] As Date

        Sub New([date] As Date)
            Me.Date = [date]
        End Sub

        Sub New([date] As String)
            Me.Date = Date.Parse([date])
        End Sub

        Sub New(yy%, mm%, dd%, H%, M%, S%)
            Me.Date = New Date(yy, mm, dd, H, M, S)
        End Sub

        Public Overrides Function ToString() As String
            Return [Date].ToString
        End Function
    End Class
End Namespace

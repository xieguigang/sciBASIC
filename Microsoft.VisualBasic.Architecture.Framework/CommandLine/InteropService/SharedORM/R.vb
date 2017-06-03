#Region "Microsoft.VisualBasic::372ca7f6bacc3a4cf907325fc409eaf5, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\CommandLine\InteropService\SharedORM\R.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Namespace CommandLine.InteropService.SharedORM

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>因为会和Unix bash之中的``ls``命令之中的``-r``参数冲突，所以在这里命名为``Rlanguage``</remarks>
    Public Class RLanguage : Inherits CodeGenerator

        Public Sub New(CLI As Type)
            MyBase.New(CLI)
        End Sub

        Public Overrides Function GetSourceCode() As String
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace

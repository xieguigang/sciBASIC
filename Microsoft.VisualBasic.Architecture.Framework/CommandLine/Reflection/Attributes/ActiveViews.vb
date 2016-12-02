#Region "Microsoft.VisualBasic::e61dd6f3ed4c08397368f16e7769142f, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\CommandLine\Reflection\Attributes\ActiveViews.vb"

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

Namespace CommandLine.Reflection

    <AttributeUsage(AttributeTargets.Class Or AttributeTargets.Struct Or AttributeTargets.Enum, AllowMultiple:=False, Inherited:=True)>
    Public Class ActiveViews : Inherits Attribute

        Public ReadOnly Property Views As String
        ''' <summary>
        ''' Code type name in the markdown, default is ``json``
        ''' </summary>
        ''' <returns></returns>
        Public Property type As String = "json"

        Sub New(view As String)
            Views = view
        End Sub

        Public Overrides Function ToString() As String
            Return Views
        End Function
    End Class
End Namespace

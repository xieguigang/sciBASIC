#Region "Microsoft.VisualBasic::de6cfa0c844217dfe9ea5a05146b4413, Microsoft.VisualBasic.Core\ApplicationServices\Tools\Plugin\PluginAttribute.vb"

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

    '     Class PluginAttribute
    ' 
    '         Properties: UniqueKey
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ApplicationServices.Plugin

    <AttributeUsage(AttributeTargets.Method Or AttributeTargets.Class, AllowMultiple:=True, Inherited:=True)>
    Public Class PluginAttribute : Inherits Attribute

        Public ReadOnly Property UniqueKey As String

        Sub New(guid As String)
            UniqueKey = guid
        End Sub

        Public Overrides Function ToString() As String
            Return UniqueKey
        End Function
    End Class
End Namespace

#Region "Microsoft.VisualBasic::7ee2f8cff11dadab6afab790cc16069f, Microsoft.VisualBasic.Core\My\Framework\FrameworkConfig.vb"

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

    '     Class FrameworkConfigAttribute
    ' 
    '         Properties: Config, Name
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace My.FrameworkInternal

    <AttributeUsage(AttributeTargets.Class Or AttributeTargets.Module Or AttributeTargets.Struct,
                    AllowMultiple:=True,
                    Inherited:=True)>
    Public Class FrameworkConfigAttribute : Inherits Attribute

        ''' <summary>
        ''' The config name in ``/@set`` or ``config.ini`` file
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Name As String

        Public ReadOnly Property Config As String
            Get
                Return App.GetVariable(Name)
            End Get
        End Property

        Sub New(configName As String)
            Name = configName
        End Sub

        Public Overrides Function ToString() As String
            Return $"Dim {Name} As String = {App.GetVariable(Name)}"
        End Function
    End Class
End Namespace

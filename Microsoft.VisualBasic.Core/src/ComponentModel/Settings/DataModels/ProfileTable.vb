#Region "Microsoft.VisualBasic::25116c411d3081067fe2931bf057c3b6, Microsoft.VisualBasic.Core\src\ComponentModel\Settings\DataModels\ProfileTable.vb"

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

    '   Total Lines: 89
    '    Code Lines: 57 (64.04%)
    ' Comment Lines: 15 (16.85%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 17 (19.10%)
    '     File Size: 2.99 KB


    '     Class ProfileTable
    ' 
    '         Properties: Description, Name, Type, value
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: SetValue
    ' 
    '     Interface IProfileTable
    ' 
    '         Properties: edges, name, type, value
    ' 
    '     Module IniExtensions
    ' 
    '         Function: LoadIni
    ' 
    '         Sub: WriteIni
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.Settings.Inf
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.Settings

    Public Class ProfileTable : Implements INamedValue
        Implements IProfileTable

        Public Property Name As String Implements INamedValue.Key, IProfileTable.name
        Public Property value As String Implements IProfileTable.value
        Public Property Type As ValueTypes Implements IProfileTable.type
        Public Property Description As String Implements IProfileTable.edges

        Sub New()
        End Sub

        Sub New(x As BindMapping)
            Name = x.Name
            value = x.Value
            Type = x.Type
            Description = x.Description
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub SetValue(Of T As {New, IProfile})(config As Settings(Of T))
            Call config.Set(Name.ToLower, value)
        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

    Public Interface IProfileTable

        Property name As String
        ReadOnly Property value As String
        Property type As ValueTypes
        Property edges As String

    End Interface

    ''' <summary>
    ''' 读写Ini文件的拓展
    ''' </summary>
    Public Module IniExtensions

        ''' <summary>
        ''' The section name of the profile data is the type name
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x"></param>
        ''' <param name="path"></param>
        <Extension>
        Public Sub WriteIni(Of T As {New, IProfile})(x As IProfile, path As String)
            Dim settings As New Settings(Of T)(x)
            Dim ini As New IniFile(path)
            Dim name As String = GetType(T).Name

            For Each config As BindMapping In settings.AllItems
                Call ini.WriteValue(name, config.Name, config.Value)
            Next

            Call ini.Flush()
        End Sub

        ''' <summary>
        ''' The section name of the profile data is the type name
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="path"></param>
        ''' <returns></returns>
        <Extension>
        Public Function LoadIni(Of T As {New, IProfile})(path As String) As T
            Dim config As Settings(Of T) = Settings(Of T).CreateEmpty
            Dim ini As New IniFile(path)
            Dim name As String = GetType(T).Name

            For Each x As BindMapping In config.AllItems
                Dim value As String = ini.ReadValue(name, x.Name)
                Call x.Set(value)
            Next

            Return config.SettingsData
        End Function
    End Module
End Namespace

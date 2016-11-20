#Region "Microsoft.VisualBasic::e154124fe856c2bc0201266d29cc8053, ..\sciBASIC#\CLI_tools\vbproj\Template\PropertyGroup.vb"

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

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class PropertyGroup

    <XmlAttribute>
    Public Property Condition As String
    Public Property Configuration As ConditionValue
    Public Property Platform As ConditionValue
    Public Property ProjectGuid As String
    Public Property OutputType As String
    Public Property RootNamespace As String
    Public Property AssemblyName As String
    Public Property FileAlignment As String
    Public Property MyType As String
    Public Property TargetFrameworkVersion As String
    Public Property TargetFrameworkProfile
    Public Property StartupObject As String
    Public Property PublishUrl As String
    Public Property Install As String
    Public Property InstallFrom As String
    Public Property UpdateEnabled As String
    Public Property UpdateMode As String
    Public Property UpdateInterval As String
    Public Property UpdateIntervalUnits As String
    Public Property UpdatePeriodically As String
    Public Property UpdateRequired As String
    Public Property MapFileExtensions As String
    Public Property ApplicationRevision As String
    Public Property ApplicationVersion As String
    Public Property IsWebBootstrapper As String
    Public Property UseApplicationTrust As String
    Public Property BootstrapperEnabled As String
    Public Property DefineDebug As String
    Public Property Prefer32Bit As String
    Public Property OptionExplicit As String
    Public Property OptionCompare As String
    Public Property OptionStrict As String
    Public Property OptionInfer As String
    Public Property SignAssembly As String
    Public Property DelaySign As String
    Public Property DebugSymbols As String
    Public Property DefineTrace As String
    Public Property TargetZone As String
    Public Property OutputPath As String
    Public Property GenerateManifests As String
    Public Property ApplicationManifest As String
    Public Property DefineConstants As String
    Public Property RemoveIntegerChecks As String
    Public Property DocumentationFile As String
    Public Property Optimize As String
    Public Property NoWarn As String
    Public Property DebugType As String
    Public Property PlatformTarget As String
    Public Property WarningsAsErrors As String
    Public Property GenerateSerializationAssemblies As String
    Public Property CodeAnalysisRuleSet As String
    Public Property AutoGenerateBindingRedirects As String

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class

#Region "Microsoft.VisualBasic::a5dffcf545e9f7dc01bdb8f0f011fbec, vs_solutions\dev\VisualStudio\vbproj\PropertyGroup.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class PropertyGroup
    ' 
    '         Properties: ApplicationManifest, ApplicationRevision, ApplicationVersion, AssemblyName, AutoGenerateBindingRedirects
    '                     BootstrapperEnabled, CodeAnalysisRuleSet, Condition, Configuration, DebugSymbols
    '                     DebugType, DefineConstants, DefineDebug, DefineTrace, DelaySign
    '                     DocumentationFile, FileAlignment, GenerateManifests, GenerateSerializationAssemblies, Install
    '                     InstallFrom, IsWebBootstrapper, MapFileExtensions, MyType, NoWarn
    '                     Optimize, OptionCompare, OptionExplicit, OptionInfer, OptionStrict
    '                     OutputPath, OutputType, Platform, PlatformTarget, Prefer32Bit
    '                     ProjectGuid, PublishUrl, RemoveIntegerChecks, RootNamespace, SignAssembly
    '                     StartupObject, TargetFrameworkProfile, TargetFrameworkVersion, TargetZone, UpdateEnabled
    '                     UpdateInterval, UpdateIntervalUnits, UpdateMode, UpdatePeriodically, UpdateRequired
    '                     UseApplicationTrust, WarningsAsErrors
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization

Namespace vbproj

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
            Return Condition
        End Function
    End Class
End Namespace

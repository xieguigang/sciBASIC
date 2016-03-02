Imports System.ComponentModel
Imports System.Reflection
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection

Namespace Scripting.MetaData

    ''' <summary>
    ''' This attribute provides a more details information about a namepace package module in your scripting plugins.
    ''' </summary>
    <XmlType("PackageNamespace", [Namespace]:="Microsoft.VisualBasic.Architecture.Framework_v3.0_22.0.76.201__8da45dcd8060cc9a")>
    Public Class PackageNamespace : Inherits [Namespace]

        ''' <summary>
        ''' This plugins project's home page url.
        ''' </summary>
        ''' <returns></returns>
        Public Property Url As String
        ''' <summary>
        ''' Your name or E-Mail
        ''' </summary>
        ''' <returns></returns>
        Public Property Publisher As String
        Public Property Revision As Integer
        ''' <summary>
        ''' 这个脚本模块包的文献引用列表
        ''' </summary>
        ''' <returns></returns>
        Public Property Cites As String
        Public Property Category As APICategories = APICategories.SoftwareTools

        ''' <summary>
        ''' This attribute provides a more details information about a namepace package module in your scripting plugins.
        ''' </summary>
        ''' <param name="ns"></param>
        Public Sub New(ns As [Namespace])
            Me.Namespace = ns.Namespace
            Me.Description = ns.Description
        End Sub

        ''' <summary>
        ''' 拷贝自身
        ''' </summary>
        ''' <param name="base"></param>
        Sub New(base As PackageNamespace)
            Me.AutoExtract = base.AutoExtract
            Me.Category = base.Category
            Me.Cites = base.Cites
            Me.Description = base.Description
            Me.Namespace = base.Namespace
            Me.Publisher = base.Publisher
            Me.Revision = base.Revision
            Me.Url = base.Url
        End Sub

        ''' <summary>
        ''' This attribute provides a more details information about a namepace package module in your scripting plugins.
        ''' </summary>
        ''' <param name="[Namespace]"></param>
        Public Sub New([Namespace] As String)
            Me.Namespace = [Namespace]
        End Sub

        ''' <summary>
        ''' This attribute provides a more details information about a namepace package module in your scripting plugins.
        ''' </summary>
        Protected Sub New()
        End Sub
    End Class

    Public Enum APICategories As Integer
        ''' <summary>
        ''' API for facilities of the software development.
        ''' </summary>
        <Description("API for facilities of the software development.")>
        SoftwareTools = 0
        ''' <summary>
        ''' Analysis Tools API that applied on your scientific research or industry production on computer science.
        ''' </summary>
        <Description("Analysis Tools API that applied on your scientific research or industry production on computer science.")>
        ResearchTools = 2
        ''' <summary>
        ''' Something small utilities for facility the scripting, makes your programming more easily.
        ''' </summary>
        <Description("Something small utilities for facility the scripting, makes your programming more easily.")>
        UtilityTools = 4
        ''' <summary>
        ''' CLI program help manual.
        ''' </summary>
        <Description("CLI program help manual.")>
        CLI_MAN = 8
    End Enum
End Namespace
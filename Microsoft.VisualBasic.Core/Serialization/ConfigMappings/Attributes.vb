#Region "Microsoft.VisualBasic::eefc2f44fbac7a267e0e359d83213b50, Microsoft.VisualBasic.Core\Serialization\ConfigMappings\Attributes.vb"

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

    '     Class MappingsIgnored
    ' 
    ' 
    ' 
    '     Class UseCustomMapping
    ' 
    ' 
    ' 
    '     Structure NodeMapping
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection

Namespace Serialization

    ''' <summary>
    ''' 这个属性或者方法不会被用于映射
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Property Or AttributeTargets.Method, AllowMultiple:=False, Inherited:=False)>
    Public Class MappingsIgnored : Inherits Attribute
    End Class

    ''' <summary>
    ''' 不会使用系统自带的映射方法进行映射
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=False)>
    Public Class UseCustomMapping : Inherits Attribute
    End Class

    Public Structure NodeMapping

        ''' <summary>
        ''' 映射的文本文件源
        ''' </summary>
        ''' <remarks></remarks>
        Dim Source As PropertyInfo
        ''' <summary>
        ''' 映射操作的目标数据模型
        ''' </summary>
        ''' <remarks></remarks>
        Dim Mapping As PropertyInfo
        ''' <summary>
        ''' 从源映射到数据模型的类型转换
        ''' </summary>
        ''' <remarks></remarks>
        Dim SourceToMappingCasting As IStringParser
        ''' <summary>
        ''' 从数据模型映射到源的类型转换
        ''' </summary>
        ''' <remarks></remarks>
        Dim MappingToSourceCasting As IStringBuilder

        Public Overrides Function ToString() As String
            Return Source.Name & "   --->  " & Mapping.PropertyType.FullName
        End Function
    End Structure
End Namespace

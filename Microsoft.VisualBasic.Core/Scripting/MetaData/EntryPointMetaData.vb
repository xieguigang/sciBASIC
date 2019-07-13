#Region "Microsoft.VisualBasic::776b8416ea8cd286200447e3e5c93cf2, Microsoft.VisualBasic.Core\Scripting\MetaData\EntryPointMetaData.vb"

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

    '     Class FunctionReturns
    ' 
    '         Properties: Description, TypeRef
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetDescription, ToString
    ' 
    '     Class OverloadsSignatureHandle
    ' 
    '         Properties: FullName, TypeIDBrief
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class ImportsConstant
    ' 
    '         Properties: Name, TypeInfo
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection

Namespace Scripting.MetaData

    <AttributeUsage(AttributeTargets.ReturnValue, AllowMultiple:=False, Inherited:=True)>
    Public Class FunctionReturns : Inherits Attribute

        Public ReadOnly Property Description As String

        Sub New(Description As String)
            Me.Description = Description
        End Sub

        Public Overrides Function ToString() As String
            Return Description
        End Function

        Public Shared ReadOnly Property TypeRef As Type = GetType(FunctionReturns)

        ''' <summary>
        ''' Gets the description of the function returns value
        ''' </summary>
        ''' <param name="method"></param>
        ''' <returns></returns>
        Public Shared Function GetDescription(method As MethodInfo) As String
            Dim attrs As Object() = method.ReturnParameter.GetCustomAttributes(attributeType:=FunctionReturns.TypeRef, inherit:=True)
            If attrs.IsNullOrEmpty Then
                Return ""
            End If

            Dim value = DirectCast(attrs(Scan0), FunctionReturns).Description
            Return value
        End Function
    End Class

    ''' <summary>
    ''' 用于解决函数重载的函数数字签名的属性
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Class, AllowMultiple:=True, Inherited:=True)>
    Public Class OverloadsSignatureHandle : Inherits Attribute

        Public ReadOnly Property TypeIDBrief As String
        Public ReadOnly Property FullName As Type

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="TypeIdBrief">Brief name for the target signature type <paramref name="FullName"></paramref></param>
        ''' <param name="FullName">Target signature type for function overloads.</param>
        ''' <remarks></remarks>
        Sub New(TypeIdBrief As String, FullName As Type)
            _TypeIDBrief = TypeIdBrief.ToLower
            _FullName = FullName
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("({0}) {1}", _TypeIDBrief, _FullName.FullName)
        End Function
    End Class

    <AttributeUsage(AttributeTargets.Field, AllowMultiple:=False, Inherited:=True)>
    Public Class ImportsConstant : Inherits Attribute

        Public ReadOnly Property Name As String

        Sub New(<Parameter("imports.constant", "")> Optional Name As String = "")
            _Name = Name
        End Sub

        Public Overrides Function ToString() As String
            Return Name
        End Function

        Public Shared ReadOnly Property TypeInfo As Type = GetType(ImportsConstant)

    End Class
End Namespace

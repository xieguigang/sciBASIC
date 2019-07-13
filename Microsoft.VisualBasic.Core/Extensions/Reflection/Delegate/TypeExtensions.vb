#Region "Microsoft.VisualBasic::4c14fe4af8015f616f7a75846f1e2621, Microsoft.VisualBasic.Core\Extensions\Reflection\Delegate\TypeExtensions.vb"

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

    '     Module TypeExtensions
    ' 
    '         Function: CanBeAssignedFrom, ImplementInterface
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Emit.Delegates

    Public Module TypeExtensions

        ''' <summary>
        ''' <paramref name="source"/>能否转换至<paramref name="destination"/>类型？
        ''' </summary>
        ''' <param name="destination"></param>
        ''' <param name="source"></param>
        ''' <returns></returns>
        <Extension>
        Public Function CanBeAssignedFrom(destination As Type, source As Type) As Boolean
            If source Is Nothing OrElse destination Is Nothing Then
                Return False
            End If

            If destination Is source OrElse source.IsSubclassOf(destination) Then
                Return True
            End If

            If destination.IsInterface Then
                Return source.ImplementInterface(destination)
            End If

            If Not destination.IsGenericParameter Then
                Return False
            End If

            Dim constraints = destination.GetGenericParameterConstraints()
            Return constraints.All(Function(t1) t1.CanBeAssignedFrom(source))
        End Function

        ''' <summary>
        ''' 目标类型<paramref name="source"/>是否实现了制定的接口类型？
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="interfaceType">接口类型信息</param>
        ''' <returns></returns>
        <Extension> Public Function ImplementInterface(source As Type, interfaceType As Type) As Boolean
            While source IsNot Nothing
                Dim interfaces = source.GetInterfaces()
                If interfaces.Any(Function(i) i Is interfaceType OrElse i.ImplementInterface(interfaceType)) Then
                    Return True
                End If

                source = source.BaseType
            End While

            Return False
        End Function
    End Module
End Namespace

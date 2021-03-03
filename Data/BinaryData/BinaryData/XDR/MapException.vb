Imports System

Namespace Xdr
    Public Class MapException
        Inherits SystemException

        Public Sub New()
            MyBase.New()
        End Sub

        Public Sub New(message As String)
            MyBase.New(message)
        End Sub

        Public Sub New(message As String, innerEx As Exception)
            MyBase.New(message, innerEx)
        End Sub

        Public Shared Function ReadOne(type As Type, innerEx As Exception) As MapException
            Return New MapException(String.Format("can't read an instance of `{0}'", type.FullName), innerEx)
        End Function

        Friend Shared Function ReadVar(type As Type, max As UInteger, innerEx As SystemException) As Exception
            Return New MapException(String.Format("can't read collection of `{0}' (length <= {1})", type.FullName, max), innerEx)
        End Function

        Friend Shared Function ReadFix(type As Type, len As UInteger, innerEx As SystemException) As Exception
            Return New MapException(String.Format("can't read collection of `{0}' (length = {1})", type.FullName, len), innerEx)
        End Function

        Friend Shared Function WriteOne(type As Type, innerEx As SystemException) As Exception
            Return New MapException(String.Format("can't write an instance of `{0}'", type.FullName), innerEx)
        End Function

        Friend Shared Function WriteFix(type As Type, len As UInteger, innerEx As SystemException) As Exception
            Return New MapException(String.Format("can't write collection of `{0}' (length = {1})", type.FullName, len), innerEx)
        End Function

        Friend Shared Function WriteVar(type As Type, max As UInteger, innerEx As SystemException) As Exception
            Return New MapException(String.Format("can't write collection of `{0}' (length <= {1})", type.FullName, max), innerEx)
        End Function
    End Class
End Namespace

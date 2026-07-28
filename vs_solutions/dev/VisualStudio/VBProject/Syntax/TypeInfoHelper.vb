Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Syntax

    ''' <summary>
    ''' helpers to build <see cref="TypeInfo"/> clr type references from the
    ''' type name text that was extracted out of the VB.NET source code.
    '''
    ''' Because the type is only known by its textual name at parse time, the
    ''' <see cref="TypeInfo.assembly"/> and <see cref="TypeInfo.reference"/>
    ''' fields are left empty; <see cref="TypeInfo.isSystemKnownType"/> can
    ''' still be used to detect framework known types.
    ''' </summary>
    Public Module TypeInfoHelper

        ''' <summary>
        ''' build a clr type reference from a type name text (e.g. "Integer",
        ''' "System.String", "List(Of T)"). returns nothing for empty input.
        ''' </summary>
        Public Function TypeRef(name As String) As TypeInfo
            If String.IsNullOrWhiteSpace(name) Then
                Return Nothing
            End If

            Return New TypeInfo With {.fullName = name.Trim()}
        End Function

    End Module

End Namespace

#Region "Microsoft.VisualBasic::d0f2f77a01e83aa65f0fb79975f1dd5d, Microsoft.VisualBasic.Core\Serialization\DumpData\DumpNode.vb"

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

    '     Class DumpNode
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Data.Linq.Mapping

Namespace Serialization

    <AttributeUsage(AttributeTargets.Property Or AttributeTargets.Field, allowmultiple:=False, inherited:=True)>
    Public Class DumpNode : Inherits Attribute
        Public Shared ReadOnly [GetTypeId] As System.Type = GetType(DumpNode)
    End Class
End Namespace

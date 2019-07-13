#Region "Microsoft.VisualBasic::53ff3590ef80bd1b4b11178cea1981a2, Data_science\Mathematica\Math\ODE\ODEsSolver\ValueVector.vb"

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

    ' Class ValueVector
    ' 
    '     Properties: Y
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: ToString
    '     Operators: +
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class ValueVector : Inherits VBInteger

    Public Property Y As Dictionary(Of NamedCollection(Of Double))

    Default Public Overrides Property Index(name As Object) As Object
        Get
            Return Y(InputHandler.ToString(name)).Value(MyBase.Value)
        End Get
        Set(value As Object)
            Y(InputHandler.ToString(name)).Value(MyBase.Value) = value
        End Set
    End Property

    Sub New()
        Call MyBase.New(0)
    End Sub

    Public Overrides Function ToString() As String
        Return $"[{Value}] " & Y.Keys.ToArray.GetJson
    End Function

    ''' <summary>
    ''' Move pointer value
    ''' </summary>
    ''' <param name="v"></param>
    ''' <param name="n%"></param>
    ''' <returns></returns>
    Public Overloads Shared Operator +(v As ValueVector, n%) As ValueVector
        v.Value += n
        Return v
    End Operator
End Class

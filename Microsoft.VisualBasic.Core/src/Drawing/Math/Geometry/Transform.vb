Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Imaging.Math2D

    Public Class Transform

        ''' <summary>
        ''' angle for rotation
        ''' </summary>
        ''' <returns></returns>
        Public Property theta As Double
        ''' <summary>
        ''' translate x
        ''' </summary>
        ''' <returns></returns>
        Public Property tx As Double
        ''' <summary>
        ''' translate y
        ''' </summary>
        ''' <returns></returns>
        Public Property ty As Double
        ''' <summary>
        ''' scale x
        ''' </summary>
        ''' <returns></returns>
        Public Property scalex As Double
        ''' <summary>
        ''' scale y
        ''' </summary>
        ''' <returns></returns>
        Public Property scaley As Double

        Public Overrides Function ToString() As String
            Return $"rotate_theta:{theta.ToString("F2")}, translate=({tx.ToString("F2")},{ty.ToString("F2")}), scale=({scalex.ToString("F2")},{scaley.ToString("F2")})"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(args As (theta As Double, tx As Double, ty As Double, scalex As Double, scaley As Double)) As Transform
            Return New Transform With {
                .theta = args.theta,
                .tx = args.tx,
                .ty = args.ty,
                .scalex = args.scalex,
                .scaley = args.scaley
            }
        End Operator

    End Class
End Namespace
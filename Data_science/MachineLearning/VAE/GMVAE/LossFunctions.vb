#Region "Microsoft.VisualBasic::40771875f9ad365bbfdae672fc996ad2, Data_science\MachineLearning\VAE\GMVAE\LossFunctions.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 186
    '    Code Lines: 75
    ' Comment Lines: 92
    '   Blank Lines: 19
    '     File Size: 8.15 KB


    ' Class LossFunctions
    ' 
    '     Function: binary_cross_entropy, entropy, kl_categorical, kl_gaussian, labeled_loss
    '               log_normal, mean_squared_error
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math
Imports tf = Microsoft.VisualBasic.MachineLearning.TensorFlow

''' <summary>
''' Loss functions used for training our model
''' </summary>
Public Class LossFunctions

    Dim eps As Double = 0

    ''' <summary>
    ''' Binary Cross Entropy between the true and predicted outputs
    ''' 
    ''' ```
    ''' loss = (1/n) * -Σ(real*log(predicted) + (1 - real)*log(1 - predicted))
    ''' ```
    ''' </summary>
    ''' <param name="real">corresponding array containing the true labels</param>
    ''' <param name="logits">corresponding array containing the output logits</param>
    ''' <param name="average">whether to average the result to obtain a value</param>
    ''' <returns>depending on average parameters the result will be the mean
    ''' of all the sample losses Or an array with the losses per sample</returns>
    Public Function binary_cross_entropy(real As Vector, logits As Vector, Optional average As Boolean = True) As Vector
        If eps > 0 Then
            Dim max_val = std.Log(1 - eps) - std.Log(eps)
            logits = tf.clip_by_value(logits, -max_val, max_val)
        End If

        Dim loss = tf.reduce_sum(tf.nn.sigmoid_cross_entropy_with_logits(logits:=logits, labels:=real))

        If average Then
            Return Vector.Scalar(tf.reduce_mean(loss))
        Else
            Return loss
        End If
    End Function

    ''' <summary>
    ''' Mean Squared Error between the true and predicted outputs
    ''' 
    ''' ```
    ''' loss = (1/n)*Σ(real - predicted)^2
    ''' ```
    ''' </summary>
    ''' <param name="real">corresponding array containing the true labels</param>
    ''' <param name="predictions">corresponding array containing the predicted labels</param>
    ''' <param name="average">whether to average the result to obtain a value</param>
    ''' <returns>depending on average parameters the result will be the mean
    ''' of all the sample losses Or an array with the losses per sample</returns>
    Public Function mean_squared_error(real As Vector, predictions As Vector, Optional average As Boolean = True) As Double
        Dim loss = tf.square(real - predictions)

        If average Then
            Return tf.reduce_mean(loss)
        Else
            Return tf.reduce_sum(loss)
        End If
    End Function

    ''' <summary>
    ''' KL Divergence between the posterior and a prior gaussian distribution (N(0,1))
    ''' 
    ''' ```
    ''' loss = (1/n) * -0.5 * Σ(1 + log(σ^2) - σ^2 - μ^2)
    ''' ```
    ''' </summary>
    ''' <param name="mean">corresponding array containing the mean of our inference model</param>
    ''' <param name="logVar">corresponding array containing the log(variance) of our inference model</param>
    ''' <param name="average">whether to average the result to obtain a value</param>
    ''' <returns>depending on average parameters the result will be the mean
    ''' of all the sample losses Or an array with the losses per sample</returns>
    Public Function kl_gaussian(mean As Vector, logVar As Vector, Optional average As Boolean = True) As Double
        Dim loss = -0.5 * tf.reduce_sum(1 + logVar - tf.exp(logVar) - tf.square(mean + eps))

        If average Then
            Return tf.reduce_mean(loss)
        Else
            Return tf.reduce_sum(loss)
        End If
    End Function

    ''' <summary>
    ''' KL Divergence between the posterior and a prior uniform distribution (U(0,1))
    ''' 
    ''' ```
    ''' loss = (1/n) * Σ(qx * log(qx/px)), because we use a uniform prior px = 1/k 
    ''' loss = (1/n) * Σ(qx * (log(qx) - log(1/k)))
    ''' ```
    ''' </summary>
    ''' <param name="qx">corresponding array containing the probs of our inference model</param>
    ''' <param name="log_qx">corresponding array containing the log(probs) of our inference model</param>
    ''' <param name="k">number of classes</param>
    ''' <param name="average">whether to average the result to obtain a value</param>
    ''' <returns>depending on average parameters the result will be the mean
    ''' of all the sample losses Or an array with the losses per sample</returns>
    Public Function kl_categorical(qx As Vector,
                                   log_qx As Vector,
                                   k As Double,
                                   Optional average As Boolean = True) As Double

        Dim loss As Vector = tf.reduce_sum(qx * (log_qx - tf.log(1 / k)))

        If average Then
            Return tf.reduce_mean(loss)
        Else
            Return tf.reduce_sum(loss)
        End If
    End Function

    ''' <summary>
    ''' Logarithm of normal distribution with mean=mu and variance=var
    ''' 
    ''' ```
    ''' log(x|μ, σ^2) = loss = -0.5 * Σ log(2π) + log(σ^2) + ((x - μ)/σ)^2
    ''' ```
    ''' </summary>
    ''' <param name="x">corresponding array containing the input</param>
    ''' <param name="mu">corresponding array containing the mean </param>
    ''' <param name="var">corresponding array containing the variance</param>
    ''' <returns>depending on average parameters the result will be the mean
    ''' of all the sample losses Or an array with the losses per sample</returns>
    Public Function log_normal(x As Vector, mu As Vector, var As Vector) As Double
        If eps > 0 Then
            var = var + eps
        End If

        Return -0.5 * tf.reduce_sum(
            tf.log(2 * std.PI) + tf.log(var) + tf.square(x - mu) / var)
    End Function

    ''' <summary>
    ''' Variational loss when using labeled data without considering reconstruction loss
    ''' 
    ''' ```
    ''' loss = log q(z|x,y) - log p(z) - log p(y)
    ''' ```
    ''' </summary>
    ''' <param name="z">array containing the gaussian latent variable</param>
    ''' <param name="z_mu">array containing the mean of the inference model</param>
    ''' <param name="z_var">array containing the variance of the inference model</param>
    ''' <param name="z_mu_prior">array containing the prior mean of the generative model</param>
    ''' <param name="z_var_prior">array containing the prior variance of the generative mode</param>
    ''' <param name="average">whether to average the result to obtain a value</param>
    ''' <returns>depending on average parameters the result will be the mean
    ''' of all the sample losses Or an array with the losses per sample</returns>
    Public Function labeled_loss(z As Vector,
                                 z_mu As Vector,
                                 z_var As Vector,
                                 z_mu_prior As Vector,
                                 z_var_prior As Vector,
                                 Optional average As Boolean = True) As Vector

        Dim loss = log_normal(z, z_mu, z_var) - log_normal(z, z_mu_prior, z_var_prior)
        loss = loss - std.Log(0.1)

        If average Then
            Return Vector.Scalar(tf.reduce_mean(loss))
        Else
            Return loss
        End If
    End Function

    ''' <summary>
    ''' Entropy loss
    ''' 
    ''' ```
    ''' loss = (1/n) * -Σ targets*log(predicted)
    ''' ```
    ''' </summary>
    ''' <param name="logits">corresponding array containing the logits of the categorical variable</param>
    ''' <param name="targets">corresponding array containing the true labels</param>
    ''' <param name="average">whether to average the result to obtain a value</param>
    ''' <returns>depending on average parameters the result will be the mean
    ''' of all the sample losses Or an array with the losses per sample</returns>
    Public Function entropy(logits As Vector, targets As Vector, Optional average As Boolean = True) As Vector
        Dim log_q = tf.nn.log_softmax(logits)
        Dim e As Vector = tf.reduce_sum(targets * log_q)

        If average Then
            Return -tf.reduce_mean(e)
        Else
            Return e
        End If
    End Function
End Class

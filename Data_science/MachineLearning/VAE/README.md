# Variational Autoencoders: Graph VAE, Gaussian Mixture VAE and GMM

Generative modelling library for sciBASIC#: a graph variational autoencoder with GCN encoder and inner-product decoder, a Gaussian mixture VAE with categorical latents, and an EM-trained Gaussian mixture model.

## Overview
- `GaussianMixtureModel` fits p(x) = sum_k pi_k * N(x | mu_k, Sigma_k) by Expectation-Maximisation, with `Random` or `KMeansPP` initialisation, full or diagonal covariance, regularisation and a log-likelihood convergence history.
- `GMVAE` adds a categorical latent y on top of the continuous latent z, so one model does clustering and generation: `Fit`, `Encode`, `Reconstruct`, `Generate` (optionally from one component) and `PredictClusters`.
- `GraphVAE` encodes a graph (adjacency matrix + node features) with two GCN layers, reparameterises z = mu + sigma * eps, and decodes adjacency by inner product and features by a linear layer; it trains with hand-written backpropagation and Adam via `Train`.
- Everything is built on the sciBASIC# `Tensor` type from the `TensorFlow` package; `MathUtils` and `GraphUtils` hold the shared math, activation, loss and adjacency-normalisation helpers.

## Key Types
- `Microsoft.VisualBasic.MachineLearning.VariationalAutoencoder.GaussianMixtureModel` — EM-trained Gaussian mixture for clustering, density estimation and sampling.
- `Microsoft.VisualBasic.MachineLearning.VariationalAutoencoder.GMVAE` — Gaussian mixture variational autoencoder with categorical and continuous latents.
- `Microsoft.VisualBasic.MachineLearning.VariationalAutoencoder.GraphVAE` — graph variational autoencoder trained on a list of (adjacency, features) graphs.
- `Microsoft.VisualBasic.MachineLearning.VariationalAutoencoder.GCNLayer` — graph convolution layer used by the GraphVAE encoder.
- `Microsoft.VisualBasic.MachineLearning.VariationalAutoencoder.LinearLayer` — dense linear layer with Adam update used by encoder and decoder.
- `Microsoft.VisualBasic.MachineLearning.VariationalAutoencoder.GraphUtils` — adjacency normalisation, sigmoid/ReLU, BCE and KL divergence helpers.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports Microsoft.VisualBasic.MachineLearning.VariationalAutoencoder

Dim X As New Tensor(100, 2)      ' 100 samples, 2 features
' ... fill X.Data ...

' EM clustering / density estimation
Dim gmm As New GaussianMixtureModel(nComponents:=3, maxIter:=200)
Call gmm.Fit(X)
Dim clusters As Integer() = gmm.Predict(X)
Dim posterior As Tensor = gmm.PredictProba(X)

' mixture VAE: cluster and generate
Dim vae As New GMVAE(inputDim:=2, latentDim:=8, nComponents:=3)
Call vae.Fit(X, epochs:=100, learningRate:=0.001)
Dim sample As Tensor = vae.Generate(n:=10)
```

## Package
- Assembly: `Microsoft.VisualBasic.MachineLearning.VAE`
- TargetFramework: `net10.0`
- Tags: `scibasic;variational-autoencoder;gmm;generative-model;clustering`

## License
GPL-3.0-or-later

# Fuzzy C-Means Clustering Algorithm

In our previous article, we described the basic concept of fuzzy clustering and we showed how to compute fuzzy clustering. In this current article, we’ll present the fuzzy c-means clustering algorithm, which is very similar to the k-means algorithm and the aim is to minimize the objective function defined as follow:

∑j=1k∑xi∈Cjumij(xi−μj)2

Where,

uij is the degree to which an observation xi belongs to a cluster cj
μj is the center of the cluster j
uij is the degree to which an observation xi belongs to a cluster cj
m is the fuzzifier.
It can be seen that, FCM differs from k-means by using the membership values uij and the fuzzifier m.

The variable umij is defined as follow:

umij=1∑l=1k(|xi−cj||xi−ck|)2m−1

The degree of belonging, uij, is linked inversely to the distance from x to the cluster center.

The parameter m is a real number greater than 1 (1.0<m<∞) and it defines the level of cluster fuzziness. Note that, a value of m close to 1 gives a cluster solution which becomes increasingly similar to the solution of hard clustering such as k-means; whereas a value of m close to infinite leads to complete fuzzyness.

Note that, a good choice is to use m = 2.0 (Hathaway and Bezdek 2001).

In fuzzy clustering the centroid of a cluster is he mean of all points, weighted by their degree of belonging to the cluster:

Cj=∑x∈Cjumijx∑x∈Cjumij

Where,

Cj is the centroid of the cluster j
uij is the degree to which an observation xi belongs to a cluster cj
The algorithm of fuzzy clustering can be summarize as follow:

Specify a number of clusters k (by the analyst)
Assign randomly to each point coefficients for being in the clusters.
Repeat until the maximum number of iterations (given by “maxit”) is reached, or when the algorithm has converged (that is, the coefficients’ change between two iterations is no more than ϵ, the given sensitivity threshold):
Compute the centroid for each cluster, using the formula above.
For each point, compute its coefficients of being in the clusters, using the formula above.
The algorithm minimizes intra-cluster variance as well, but has the same problems as k-means; the minimum is a local minimum, and the results depend on the initial choice of weights. Hence, different initializations may lead to different results.

Using a mixture of Gaussians along with the expectation-maximization algorithm is a more statistically formalized method which includes some of these ideas: partial membership in classes.
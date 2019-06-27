library("FactoMineR")
library("factoextra")

data(decathlon2)
data <- decathlon2[1:23, 1:10]

library("FactoMineR")
res.pca <- PCA(data, graph = FALSE)

eig.val <- get_eigenvalue(res.pca)
eig.val
#        eigenvalue variance.percent cumulative.variance.percent
# Dim.1   4.1242133        41.242133                    41.24213
# Dim.2   1.8385309        18.385309                    59.62744
# Dim.3   1.2391403        12.391403                    72.01885
# Dim.4   0.8194402         8.194402                    80.21325
# Dim.5   0.7015528         7.015528                    87.22878
# Dim.6   0.4228828         4.228828                    91.45760
# Dim.7   0.3025817         3.025817                    94.48342
# Dim.8   0.2744700         2.744700                    97.22812
# Dim.9   0.1552169         1.552169                    98.78029
# Dim.10  0.1219710         1.219710                   100.00000


fviz_eig(res.pca, addlabels = TRUE, ylim = c(0, 50))

var <- get_pca_var(res.pca)

fviz_pca_var(res.pca, col.var = "black")

library("corrplot")
corrplot(var$cos2, is.corr=FALSE)

fviz_pca_var(res.pca, col.var = "contrib",
     gradient.cols = c("#00AFBB", "#E7B800", "#FC4E07")
     )
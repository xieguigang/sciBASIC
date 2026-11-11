# 校准探针：比较 R 的 lambda.max 与统一 KKT 公式，确定各 family 的目标函数缩放。
suppressPackageStartupMessages(library(glmnet))

probe <- function(label, x, y, family) {
    x <- as.matrix(x)
    n <- length(y)
    p <- ncol(x)
    xc <- x - matrix(colMeans(x), nrow = n, ncol = p, byrow = TRUE)
    xs <- sqrt(colSums(xc^2) / n)
    xs[xs == 0] <- 1
    z <- xc / matrix(xs, nrow = n, ncol = p, byrow = TRUE)
    score <- max(abs(as.vector(crossprod(z, y - mean(y))))) / n
    fit <- glmnet(x, y, family = family)
    cat(sprintf("%s: lambda1=%.12g score=%.12g ratio=%.12g nlambda=%d dev1=%.8g devLast=%.8g nulldev=%.8g\n",
                label, fit$lambda[1], score, fit$lambda[1] / score,
                length(fit$lambda), fit$dev.ratio[1], tail(fit$dev.ratio, 1), fit$nulldev))
}

data(QuickStartExample)
probe("gaussian", QuickStartExample$x, QuickStartExample$y, "gaussian")
data(BinomialExample)
probe("binomial", BinomialExample$x, BinomialExample$y, "binomial")
data(PoissonExample)
probe("poisson", PoissonExample$x, PoissonExample$y, "poisson")

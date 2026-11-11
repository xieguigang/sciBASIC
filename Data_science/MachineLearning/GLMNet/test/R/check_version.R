# GLMNet 5.1 复刻自检：核验本地 R / glmnet 版本，并打印 R 端的默认参数语义。
# 该脚本只读，不安装任何软件包、不写任何文件；对拍基准由 export_golden.R 生成。
suppressPackageStartupMessages(library(glmnet))

v <- as.character(utils::packageVersion("glmnet"))
cat(sprintf("R.version=%s\n", as.character(getRversion())))
cat(sprintf("glmnet.version=%s\n", v))

if (!identical(substr(v, 1, 3), "5.1")) {
    cat("ERROR: glmnet 5.1 is required to generate the golden data.\n")
    quit(status = 2)
}

f <- formals(glmnet)
g <- formals(cv.glmnet)
show <- function(name, value) cat(sprintf("%s=%s\n", name, paste(deparse(value), collapse = " ")))

show("glmnet.alpha", f$alpha)
show("glmnet.nlambda", f$nlambda)
show("glmnet.lambda.min.ratio", f$lambda.min.ratio)
show("glmnet.standardize", f$standardize)
show("glmnet.intercept", f$intercept)
show("glmnet.thresh", f$thresh)
show("glmnet.dfmax", f$dfmax)
show("glmnet.pmax", f$pmax)
show("glmnet.maxit", f$maxit)
show("glmnet.type.gaussian", f$type.gaussian)
show("glmnet.type.multinomial", f$type.multinomial)
show("glmnet.type.logistic", f$type.logistic)
show("glmnet.standardize.response", f$standardize.response)
show("glmnet.relax", f$relax)
show("cv.glmnet.nfolds", g$nfolds)
show("cv.glmnet.grouped", g$grouped)
show("cv.glmnet.keep", g$keep)
show("cv.glmnet.type.measure", g$type.measure)

gl <- glmnet.control()
cat(sprintf("control.names=%s\n", paste(names(gl), collapse = ",")))
cat(sprintf("control.eps=%s\n", gl$eps))
cat(sprintf("control.devmax=%s\n", gl$devmax))

# 实测 gaussian 路径的 lambda 网格与偏差口径，用于校准 VB 内核
data(QuickStartExample)
x <- as.matrix(QuickStartExample$x)
y <- QuickStartExample$y
set.seed(1)
fit <- glmnet(x, y)
n <- length(y)
ymean <- mean(y)
p <- ncol(x)
xm <- colMeans(x)
xc <- x - matrix(xm, nrow = n, ncol = p, byrow = TRUE)
xs <- sqrt(colSums(xc^2) / n)
xs[xs == 0] <- 1
z <- xc / matrix(xs, nrow = n, ncol = p, byrow = TRUE)
score <- max(abs(as.vector(crossprod(z, y - ymean)))) / n
cat(sprintf("probe.n=%d p=%d nlambda=%d\n", n, ncol(x), length(fit$lambda)))
cat(sprintf("probe.lambda.max.R=%.12g\n", fit$lambda[1]))
cat(sprintf("probe.lambda.max.formula=%.12g\n", score))
cat(sprintf("probe.lambda.last=%.12g\n", tail(fit$lambda, 1)))
cat(sprintf("probe.min.ratio=%.12g\n", tail(fit$lambda, 1) / fit$lambda[1]))
cat(sprintf("probe.nulldev=%.12g\n", fit$nulldev))
cat(sprintf("probe.dev.ratio.1=%.12g\n", fit$dev.ratio[1]))
cat(sprintf("probe.dev.ratio.last=%.12g\n", tail(fit$dev.ratio, 1)))
cat(sprintf("probe.df.1=%d df.last=%d\n", fit$df[1], tail(fit$df, 1)))
cat("OK: glmnet 5.1 environment verified.\n")

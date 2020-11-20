#' s.class create a s.class plot for a dudi object (ade4)
#'
#' @param dfxy [data.frame]: A data frame containing the two columns for the axes
#' @param fac [vector(character)]: A factor partitioning the rows of the data frame in classes.
#' @param xax [numeric]: The column number of x in ‘dfxy’. Component from 'dudi.pca' object for x axis.
#' @param yax [numeric]: The column number of y in ‘dfxy’. Component from 'dudi.pca' object for y axis.
#' @param lab.names [vector(character)]: Should labels be printed.
#' @param lab.extreme [logical]: Should outlier's labels be printed.
#' @param thresh.extreme [numeric]: Outlier's scale.
#' @param drawEllipse [logical]: Draw an ellipse to represent variance.
#' @param cellipse [numeric]: Size factor for the ellipse.
#' @param drawSegment [logical]: Draw segment from center of each groups defined by 'fac'.
#' @param bw [logical]: Turn the Grey theme into a a Black&White theme.
#' @param noGrid [logical]: A grid in the background of the plot should be drawn.
#' @param base_size [numeric]: Font size.
#' @return A s.class plot in ggplot2 format.
#' @export
# @examples
# s.class()
s.class <- function (dfxy, fac, xax = 1, yax = 2, lab.names = "", lab.extreme = FALSE, thresh.extreme = 2, drawEllipse = TRUE, cellipse = 1, drawSegment = TRUE, bw = TRUE, noGrid = FALSE, base_size = 12) {
    is.installed <- function (mypkg) {
        is.element(mypkg, installed.packages()[,1])
    }
    if (is.installed("ggplot2") & is.installed("grid")) {
    # if (require("ggplot2") & require("grid")) {
        # require("ggplot2")
        # require("grid")
        # require("scales")
        data <- data.frame(cbind(x = dfxy[, xax], y = dfxy[, yax]), row.names = rownames(dfxy))
        if (any(lab.names==FALSE)) {
            lab.names <- ""
        } else {}
        data[, "label"] <- lab.names
        data[, "class"] <- as.factor(fac)
        centroids <- aggregate(cbind(x, y) ~ class, data = data, mean)
        colnames(centroids) <- paste0(colnames(centroids), c("", ".centroid", ".centroid"))
        data <- merge(data, centroids, by = "class")
        data[, "dist"] <- sqrt((data[, "x"]-data[, "x.centroid"])^2 + (data[, "y"]-data[, "y.centroid"])^2)
        data <- do.call("rbind", by(data, data[, "class"], function (iDist) {
            cbind(iDist, close = iDist[, "dist"]>quantile(iDist[, "dist"], c(0.75))+thresh.extreme*diff(quantile(iDist[, "dist"], c(0.25, 0.75))))
        }))
        rownames(data) <- NULL

        p <- ggplot(data = data) + theme_grey(base_size = base_size)
        if (bw) {
            blackwhite <- function (base_size = 12, base_family = "", noGrid = FALSE) {
                if (noGrid) {
                    noGridColour <- c("transparent", "transparent") # "white"
                } else {
                    noGridColour <- c("grey90", "grey98")
                }
                theme_grey(base_size = base_size, base_family = base_family) %+replace%
                    theme(
                        axis.text = element_text(size = rel(0.8)),
                        axis.ticks = element_line(colour = "black"),
                        legend.background = element_rect(fill = "white", colour = "black"),
                        legend.key = element_rect(fill = "white", colour = "black"),
                        legend.position = "right",
                        legend.justification = "center",
                        legend.box = NULL,
                        panel.background = element_rect(fill = "white", colour = NA),
                        panel.border = element_rect(fill = NA, colour = "black"),
                        panel.grid.major = element_line(colour = noGridColour[1], size = 0.2),
                        panel.grid.minor = element_line(colour = noGridColour[2], size = 0.5),
                        strip.background = element_rect(fill = "grey80", colour = "black", size = 0.2)
                    )
            }
            p <- p + blackwhite(base_size = base_size, noGrid = noGrid)
        } else {}
        if (length(unique(fac))<=6)  {
            p <- p + scale_colour_manual(values = c("dodgerblue", "firebrick2", "springgreen3", "maroon2", "goldenrod2", "deepskyblue"))
        } else {}
        p <- p + geom_hline(aes(yintercept = 0)) + geom_vline(aes(xintercept = 0))
        p <- p + geom_point(aes_string(x = "x", y = "y", colour = "class", shape = "class"))

        if (drawSegment) {
            p <- p + geom_point(data = centroids, aes_string(x = "x.centroid", y = "y.centroid", colour = "class"), colour = "transparent")
            p <- p + geom_segment(aes_string(x = "x.centroid", y = "y.centroid", xend = "x", yend = "y", colour = "class"))
        } else {}

        if (drawEllipse && is.installed("ellipse")) {
        # if (drawEllipse & require("ellipse")) {
            # require(ellipse)
            dataEllipse <- data.frame()
            for (g in levels(data[, "class"])) {
                dataEllipse <- rbind(dataEllipse,
                    cbind(as.data.frame(with(data[data[, "class"]==g,], ellipse(cor(x, y), scale = cellipse*c(sd(x), sd(y)), centre = c(mean(x), mean(y))))), class = g))
            }
            colnames(dataEllipse) <- c("xEllipse", "yEllipse", "classEllipse")
            p <- p + geom_path(data = dataEllipse, aes_string(x = "xEllipse", y = "yEllipse", colour = "classEllipse"))
        } else {
            if (drawEllipse) {
                warning("[s.class] ellipse can not be drawn! 'ellipse' package is missing.")
            } else {}
        }
        if (lab.extreme & any(data[, "close"])) {
            p <- p + geom_text(data = data[data[, "close"]==TRUE, ], aes_string(x = "x", y = "y", label = "label"), colour = "black", hjust = 0.5, vjust = 0.5, size = rel(4))
        } else {
            p <- p + geom_text(data = data, aes_string(x = "x", y = "y", label = "label"), colour = "black", hjust = 0.5, vjust = 0.5, size = rel(4))
        }
        # p <- p + labs(x = NULL, y = NULL)
        # p <- p + theme(
            # axis.title = element_blank(),
            # axis.text = element_blank(),
            # axis.ticks = element_blank(),
            # axis.ticks.length = unit(0, "cm"),
            # axis.ticks.margin = unit(0, "cm"),
            # plot.margin = unit(c(0, 0, 0, 0), "cm")
        # )
        # p <- p + annotate("text", x = -Inf, y = -Inf, label = paste0("xax = ", xax, "; yax = ", yax), hjust = -0.05, vjust = -0.5, size = rel(5))
        p <- p + labs(x = paste("Component", xax), y = paste("Component", yax))

        if (any(nchar(levels(fac))<=2)) {
            p <- p + theme(legend.position = "none")
            if (drawEllipse | drawSegment) {
                p <- p + geom_point(data = centroids, aes_string(x = "x.centroid", y = "y.centroid"), fill = "white", colour = "grey30", shape = 22, size = rel(8.5))
                p <- p + geom_text(data = centroids, aes_string(x = "x.centroid", y = "y.centroid", label = "class", colour = "class"), hjust = 0.5, vjust = 0.5, size = rel(4))
            } else {}
        } else {
             p <- p + theme(legend.title = element_blank())
            # warning("[s.class] 'fac' have more than 2 characters. Labels might not be displayed properly.")
        }


        lim <- apply(data[, c("x", "y")], 2, range)
        lims <- rbind(apply(lim, 2, median)-max(apply(lim, 2, diff))/2,
            apply(lim, 2, median)+max(apply(lim, 2, diff))/2)
        lims <- lims+apply(lims, 2, diff)*0.05*c(-1, 1)
        if (drawEllipse) {
            ellipseLims <- apply(dataEllipse[, c("xEllipse", "yEllipse")], 2, range)
            newLimsMax <- matrix(mapply(max, lims, ellipseLims), ncol = 2, dimnames = dimnames(lims))
            newLimsMin <- matrix(mapply(min, lims, ellipseLims), ncol = 2, dimnames = dimnames(lims))
            newLims <- rbind(newLimsMin[1, ], newLimsMax[2, ])
            Breaks <- apply(newLims, 2, pretty_breaks())
            if (is.matrix(Breaks)) {
                xBreaks <- Breaks[, 1]
                yBreaks <- Breaks[, 2]
                breakStep <- min(unique(as.vector(diff(Breaks))))
            } else {
                xBreaks <- Breaks[[1]]
                yBreaks <- Breaks[[2]]
                breakStep <- max(unique(unlist(sapply(Breaks, diff))))
            }
            if (breakStep>diff(range(xBreaks))/2 || breakStep>diff(range(yBreaks))/2) {
                breakStep <- min(unique(unlist(sapply(Breaks, diff))))
            } else {}
            if (findInterval(0, xBreaks)>0) {
                xBreaks <- sort(unique(c(
                    sign(min(xBreaks)) * seq(0, abs(min(xBreaks)), breakStep),
                    seq(0, max(xBreaks), breakStep)
                )))
            } else {
                xBreaks <- seq(min(xBreaks), max(xBreaks), breakStep)
            }
            if (findInterval(0, yBreaks)>0) {
                yBreaks <- sort(unique(c(
                    sign(min(yBreaks)) * seq(0, abs(min(yBreaks)), breakStep),
                    seq(0, max(yBreaks), breakStep)
                )))
            } else {
                yBreaks <- seq(min(yBreaks), max(yBreaks), breakStep)
            }
            p <- p + scale_x_continuous(breaks = xBreaks, limits = newLims[, "x"]) + scale_y_continuous(breaks = yBreaks, limits = newLims[, "y"])
        } else {
            Breaks <- apply(lims, 2, pretty_breaks())
            if (is.matrix(Breaks)) {
                xBreaks <- Breaks[, 1]
                yBreaks <- Breaks[, 2]
            } else {
                xBreaks <- Breaks[[1]]
                yBreaks <- Breaks[[2]]
            }
            p <- p + scale_x_continuous(breaks = xBreaks, limits = lims[, "x"]) + scale_y_continuous(breaks = yBreaks, limits = lims[, "y"])
        }
        return(p)
    } else {
        stop("[s.class] 'ggplot2' and 'grid' packages must be installed.")
    }
}
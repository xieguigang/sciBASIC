#' manhattan create a Manhanttan plot
#'
#' @param data [data.frame]: A 'data.frame' which include some columns: chromosome, position and pvalues.
#' @param chr [character]: Name of the 'chr' (chromosome) column in 'data'.
#' @param position [character]: Name of the 'position' column in 'data'.
#' @param y [character]: Name of the 'p-value' column in 'data' (or anything you want to plot with a manhattan plot).
#' @param title [character]: The text for the title.
#' @param xlab [character]: The text for the x axis.
#' @param ylab [character]: The text for the y axis.
#' @param sep [numeric]: Width of the space between bars.
#' @param colour [vector(character or numeric)]: A vector of colors to use. It should be of the same length as the number of chromosome.
#' @param bw [logical]: Turn the Grey theme into a a Black&White theme.
#' @param noGrid [logical]: A grid in the background of the plot should be drawn.
#' @param base_size [numeric]: Font size.
#' @return A Manhattan plot in ggplot2 format.
#' @export
# @examples
# manhattan()
manhattan <- function (data, chr, position, y, title = "Manhattan plot", xlab = "Chromosomes", ylab = "P-Value", sep = 0.02, colour = sapply(c(seq(0.5, 1, by = 1/23), seq(0, 0.5, by = 1/23)), hsv, s = 0.8, v = 1), bw = TRUE, noGrid = FALSE, base_size = 12) {
    data[, chr] <- gsub("chr", "", tolower(data[, chr]))
    data[, chr] <- factor(toupper(data[, chr]), levels = c(seq(22), "X", "Y"))
    data <- data[order(data[, chr], data[, position]), ]
    notNA <- apply(data[, c(chr, position)], 1, function (iRow) {any(is.na(iRow))})
    data <- data[which(!notNA), ]
    chrSize <- table(data[, chr])
    if (length(chrSize)!=24 | any(chrSize==0)) {
        CHR <- c(seq(22), "X", "Y")
        equalZero <- names(which(chrSize==0))
        notIn <- CHR[which(!CHR %in% names(chrSize))]
        chr2Add <- unique(c(equalZero, notIn))
        newLines <- data.frame(do.call("rbind", lapply(chr2Add, function (iChr) {
            newLine <- matrix(as.numeric(rep(NA, ncol(data))), ncol = ncol(data), dimnames = list(NULL, colnames(data)))
            newLine <- data.frame(newLine)
            newLine[1, chr] <- iChr
            return(newLine)
        })))
        colnames(newLines) <- colnames(data)
        data <- rbind(data, newLines)
        data <- data[order(data[, chr], data[, position]), ]
    } else {}
    chrSize <- chrSize[chrSize!=0]
    data <- data[!is.na(data[, y]), ]
    chrSizeNew <- table(data[, chr])
    chrSizeNew <- chrSizeNew[chrSizeNew!=0]
    chrStep <- floor(sum(chrSizeNew) * sep)
    data[, "xPos"] <- unlist(sapply(seq_along(chrSizeNew), function (iSize) {
        if (chrSizeNew[iSize]!=0) {
            xPos <- seq_len(chrSizeNew[iSize])
            range(xPos)
            if (iSize>1) {
                xPos <- xPos + sum(chrSizeNew[seq(iSize-1)]) + chrStep*(iSize-1)
                range(xPos)
            } else {}
            return(xPos)
        } else {}
    }), use.names = FALSE)
    avoidZero <- rep(0, length(chrSize))
    avoidZero[which(chrSize==0)] <- chrStep
    whichIsCenter <- ceiling(c(cumsum(chrSizeNew) - diff(c(0, cumsum(chrSizeNew)))/2))
    xBreaks <- data[whichIsCenter, "xPos"]
    p <- ggplot(data = data, aes_string(x = "xPos", y = y, colour = chr))
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
    p <- p + theme(
            panel.background = element_rect(colour = "black"),
            panel.grid.major.x = element_blank(),
            panel.grid.minor.x = element_blank(),
            legend.position = "none"
        ) +
        geom_point(size = 1.5, shape = 1, na.rm = TRUE) +
        scale_colour_manual(values = colour) +
        scale_x_continuous(
            breaks = xBreaks,
            labels = names(chrSize),
            limits = c(min(data[, "xPos"]), max(data[, "xPos"])+sum(avoidZero)),
            expand = c(0.01, 0.01)
        ) +
        labs(title = title, y = ylab, x = xlab)
    # suppressWarnings(print(p))
    return(p)
}
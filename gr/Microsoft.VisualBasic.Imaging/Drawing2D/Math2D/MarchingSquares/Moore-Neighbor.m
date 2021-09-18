function boundary = traceit( input )
% https://www.mathworks.com/matlabcentral/fileexchange/42144-moore-neighbor-boundary-trace
% 
% This function uses Moore-Neighbor Tracing to provide an organized list of
% points in the trace of a boundary for an object in a binary image.
%
% The format of the output is:
%
%   [ x1  y1 ]
%   [ x2  y2 ]
%   [ ..  .. ]
%   [ xn  yn ]
%
% The input image provided should be a binary image. If the image is not
% binary, results can not be guaranteed.
%
% To visualize the results of this function, the following may be useful:
%
%     boundary = traceit( input );
%     imshow( input );
%     hold on;
%     plot( boundary( :, 1 ), boundary( :, 2 ), 'r' );
%     hold off;
%
% If more than one connected object is present in the binary image, you'll
% need to modify the code to find an initial entry for each object. This
% was beyond the scope of my needs so I have not included that feature in
% this code.
%
% For a better explanation of the Moore-Neighbor Tracing algorithm, check
% out the following website: <a href="matlab: 
% web('http://www.imageprocessingplace.com/downloads_V3/root_downloads/tutorials/contour_tracing_Abeer_George_Ghuneim/moore.html')">Image Processing Place</a>
%
% See also BWTRACEBOUNDARY, BWMORPH, IMSHOW, PLOT

    % Pad the input image with a 1-px border.
    binary = logical( input );
    [ rows, columns ] = size( binary );
    padded( rows + 2, columns + 2 ) = 0;
    padded( 2 : rows + 1, 2 : columns + 1 ) = binary;
    
    % Remove interior pixels with all 4-connected neighbors.
    N = circshift( padded, [  0  1 ] );
    S = circshift( padded, [  0 -1 ] );
    E = circshift( padded, [ -1  0 ] );
    W = circshift( padded, [  1  0 ] );
    boundary_image = padded - ( padded + N + S + E + W == 5 );

    % To prevent reallocating boundary, we need to initialize it.
    boundary_size = sum( boundary_image( : ) ) + 1;
    boundary( boundary_size, 2 ) = 0;

    % Scan for the first pixel, Left-to-Right & Top-to-Bottom.
    for i = 1 : rows
        for j = 1 : columns
           if binary( i, j ) == 1
               break;
           end
        end
        if binary( i, j ) == 1
            break;
        end
    end

    % Set this pixel ( w/ padded offset ) as the initial entry point.
    initial_entry = [ j, i ] + 1;

    % Designate a directional offset array for search positions.
    % [ 2 ][ 3 ][ 4 ]
    % [ 1 ][ X ][ 5 ]
    % [ 8 ][ 7 ][ 6 ]
    % Column 1: x-axis offset // Column 2: y-axis offset
    neighborhood = [ -1 0; -1 -1; 0 -1; 1 -1; 1 0; 1 1; 0 1; -1 1 ];
    exit_direction = [ 7 7 1 1 3 3 5 5 ];
    
    % Find the first point in the boundary, Moore-Neighbor of entry point.
    for n = 1 : 8 % 8-connected neighborhood
        c = initial_entry + neighborhood( n, : );
        if padded( c( 2 ), c( 1 ) ) == 1
            initial_position = c;
            break;
        end
    end

    % Set next direction based on found pixel ( i.e. 3 -> 1).
    initial_direction = exit_direction( n );

    % Start the boundary set with this pixel.
    boundary( 1, : ) = initial_position;
    
    % Initialize variables for boundary search.
    position = initial_position;
    direction = initial_direction;
    boundary_size = 1;
    
    % Return a list of the ordered boundary pixels.
    while true
        
        % Find the next neighbor with a clockwise search.
        for n = circshift( 1 : 8, [ 0, 1 - direction ] )
            c = position + neighborhood( n, : );
            if padded( c( 2 ), c( 1 ) ) == 1
                position = c;
                break;
            end
        end
        
        % Neighbor found, save its information.
        direction = exit_direction( n );
        boundary_size = boundary_size + 1;
        boundary( boundary_size, : ) = position;
        
        % Entered the initial pixel the same way twice, the end.
        if all( position == initial_position ) &&...
              ( direction == initial_direction )
           break;
        end
    end
    
    % Remove the offset caused by the padding.
    boundary = boundary - 1;
end
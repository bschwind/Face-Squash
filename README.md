Face-Squash
===========

A demo to distort an image by stretching a user-defined quad over an image and mapping the values back to a rectangle.

The main idea behind the algorithm is interpolating over a quad, as shown below. You then use those interpolated values as texture coordinates to sample the texture.

![Alt text](https://raw.github.com/bschwind/Face-Squash/master/QuadInterpolate.png "Optional title")

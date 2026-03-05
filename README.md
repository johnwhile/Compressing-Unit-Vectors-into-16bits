# 16-Bit Spherical Coordinate Compression Methods using Discrete Spherical Coordinates (DSC).

Unit vectors is a vector with a magnitude (length) of exactly 1, used primarily to indicate direction without contributing to the magnitude of a scalar quantity. They are widely used in 3D graphics normal, tangents, etc...
https://en.wikipedia.org/wiki/Unit_vector
This proposed method is able to convert a unit vector, usually composed of 3 floating point numbers (x,y,z) for a total size of 4x3x8=96bit, into a 16-bit number, with a perfect homogeneous radial distribution.

### Coordinate system
Before describing the method it is necessary to define a reference system that I used for my directx11 graphics engine: left handles coordinate system. The method can be used for any reference system.

![](https://github.com/johnwhile/Compressing-Unit-Vectors-into-16bits/blob/main/coord.jpg)

This function converts any vector to polar coordinates.
```c#
static (float theta, float phi) spherical(Vector3f cartesian);
```


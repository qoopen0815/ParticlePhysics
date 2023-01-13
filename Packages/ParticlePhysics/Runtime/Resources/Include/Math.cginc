#ifndef MATH_INCLUDED
#define MATH_INCLUDED

#define QUATERNION_IDENTITY float4(0, 0, 0, 1)

#ifndef PI
#define PI 3.14159265359f
#endif 

// Quaternion multiplication
// http://mathworld.wolfram.com/Quaternion.html
inline float4 qmul(float4 q1, float4 q2)
{
	return float4(
        q2.xyz * q1.w + q1.xyz * q2.w + cross(q1.xyz, q2.xyz),
        q1.w * q2.w - dot(q1.xyz, q2.xyz)
    );
}

// Vector rotation with a quaternion
// http://mathworld.wolfram.com/Quaternion.html
inline float3 rotate_vector(float3 v, float4 r)
{
	float4 r_c = r * float4(-1, -1, -1, 1);
	return qmul(r, qmul(float4(v, 0), r_c)).xyz;
}

// http://marupeke296.sakura.ne.jp/DXG_No58_RotQuaternionTrans.html
inline float4x4 quaternion_to_matrix(float4 quat)
{
	float4x4 m = float4x4(float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0));

	float x = quat.x, y = quat.y, z = quat.z, w = quat.w;
	float x2 = x + x, y2 = y + y, z2 = z + z;
	float xx = x * x2, xy = x * y2, xz = x * z2;
	float yy = y * y2, yz = y * z2, zz = z * z2;
	float wx = w * x2, wy = w * y2, wz = w * z2;

	m[0][0] = 1.0 - (yy + zz);
	m[0][1] = xy - wz;
	m[0][2] = xz + wy;

	m[1][0] = xy + wz;
	m[1][1] = 1.0 - (xx + zz);
	m[1][2] = yz - wx;

	m[2][0] = xz - wy;
	m[2][1] = yz + wx;
	m[2][2] = 1.0 - (xx + yy);

	m[3][3] = 1.0;

	return m;
}

inline float4 euler_to_quaternion(float3 e)
{
	// e must be radian.
	double cr = cos(e.x * 0.5);
	double sr = sin(e.x * 0.5);
	double cp = cos(e.y * 0.5);
	double sp = sin(e.y * 0.5);
	double cy = cos(e.z * 0.5);
	double sy = sin(e.z * 0.5);

	float4 q = float4(
		sr * cp * cy - cr * sp * sy,
		cr * sp * cy + sr * cp * sy,
		cr * cp * sy - sr * sp * cy,
		cr * cp * cy + sr * sp * sy
	);

	return q;
}

#endif

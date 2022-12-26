#ifndef MATH_INCLUDED
#define MATH_INCLUDED

// Quaternion multiplication
// http://mathworld.wolfram.com/Quaternion.html
inline float4 qmul(float4 q1, float4 q2)
{
	return float4(
        q2.xyz * q1.w + q1.xyz * q2.w + cross(q1.xyz, q2.xyz),
        q1.w * q2.w - dot(q1.xyz, q2.xyz)
    );
}

// http://marupeke296.sakura.ne.jp/DXG_No58_RotQuaternionTrans.html
inline float4x4 QuaternionToRotateMatrix(float4 q)
{
	float4x4 mat = float4x4(
		1 - 2 * q.y * q.y - 2 * q.z * q.z, 2 * q.x * q.y + 2 * q.w * q.z, 2 * q.x * q.z - 2 * q.w * q.y, 0,
		2 * q.x * q.y - 2 * q.w * q.z, 1 - 2 * q.x * q.x - 2 * q.z * q.z, 2 * q.y * q.z + 2 * q.w * q.x, 0,
		2 * q.x * q.z + 2 * q.w * q.y, 2 * q.y * q.z - 2 * q.w * q.x, 1 - 2 * q.x * q.x - 2 * q.y * q.y, 0,
		0, 0, 0, 1
	);
	return mat;
}

inline float4 EulerToQuaternion(float3 e)
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

// Vector rotation with a quaternion
// http://mathworld.wolfram.com/Quaternion.html
inline float3 RotateVector(float3 v, float4 r)
{
	float4 r_c = r * float4(-1, -1, -1, 1);
	return qmul(r, qmul(float4(v, 0), r_c)).xyz;
}

#endif

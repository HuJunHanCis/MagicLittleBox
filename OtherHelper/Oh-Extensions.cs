using System;
using System;
using System.Runtime.CompilerServices;
using MathNet.Numerics.LinearAlgebra;
using static System.Math;
using System;
using System.Runtime.CompilerServices;
using System;
using System.Runtime.CompilerServices;
using MathNet.Numerics.LinearAlgebra;
using ABB.Robotics.Controllers.RapidDomain;

namespace OtherHelper
{
    public static class AaExtensions
    {

        public static short ToInt16(this bool bl) => Convert.ToInt16(bl ? 1 : 0);

        public static int ToInt32(this string str) => Convert.ToInt32(str);

        public static float ToFloat(this double d) => Convert.ToSingle(d);

        public static double ToDouble(this string str) => Convert.ToDouble(str);

        public static double ToDouble(this int vaule) => Convert.ToDouble(vaule);
        
        private static double ToRad(double deg) => deg * PI / 180.0;
        
    }
    
    public static class AaMath
    {
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ToRadian(this double degree)
        {
            return degree / 180 * Math.PI;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ToDegree(this double radian)
        {
            return radian / Math.PI * 180;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double NormaliseAngleInRadian(this double radian)
        {
            return Math.Atan2(Math.Sin(radian), Math.Cos(radian));
        }
        
        public static Matrix<double> DhOrigin(double alphai, double ai, double di, double thethai)
        {
            alphai = alphai.ToRadian();// / 180 * Math.PI;
            thethai = thethai.ToRadian();// / 180 * Math.PI;
            var m = Matrix<double>.Build.Dense(4, 4);
            m[0, 0] = Math.Cos(thethai);
            m[0, 1] = -Math.Sin(thethai) * Math.Cos(alphai);
            m[0, 2] = Math.Sin(thethai) * Math.Sin(alphai);
            m[0, 3] = ai * Math.Cos(thethai);
            m[1, 0] = Math.Sin(thethai);
            m[1, 1] = Math.Cos(thethai) * Math.Cos(alphai);
            m[1, 2] = -Math.Cos(thethai) * Math.Sin(alphai);
            m[1, 3] = ai * Math.Sin(thethai);
            m[2, 0] = 0;
            m[2, 1] = Math.Sin(alphai);
            m[2, 2] = Math.Cos(alphai);
            m[2, 3] = di;
            m[3, 0] = 0;
            m[3, 1] = 0;
            m[3, 2] = 0;
            m[3, 3] = 1;
            return m;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector<double> CrossProduct3D(this Vector<double> a, Vector<double> b)
        {
            if (a.Count != 3 || b.Count != 3)
            {
                throw new ArgumentException("只能用于3维向量");
            }
            Vector<double> c = Vector<double>.Build.Dense(new double[] { a[1] * b[2] - a[2] * b[1], -a[0] * b[2] + a[2] * b[0], a[0] * b[1] - a[1] * b[0] });
            return c;
        }
        
        // 向量安全归一化：长度为 0 则原样返回，避免 NaN
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MathNet.Numerics.LinearAlgebra.Vector<double> SafeNorm(this MathNet.Numerics.LinearAlgebra.Vector<double> v)
        {
            var n = v.L2Norm();
            return n > 0 ? v / n : v;
        }
        
        
        
    }
    
    public static class AaAbbSolver
    {
        public static VectorBuilder<double> V = Vector<double>.Build;
        public static VectorBuilder<float> VF = Vector<float>.Build;
        public static Matrix<double> GetPosMatrixFromQuatAndTrans(Vector<double> quat, Vector<double> trans)
        {
            return GetPosMatrixFromRotAndTrans(quat.Quat2Rotation(), trans);
        }
        public static Matrix<double> GetPosMatrixFromEulerAndTrans(Vector<double> euler, Vector<double> trans)
        {
            return GetPosMatrixFromRotAndTrans(euler.Euler2Rotation(), trans);
        }
        public static Matrix<double> GetPosMatrixFromRotAndTrans(Matrix<double> rot, Vector<double> trans)
        {
            Matrix<double> m = Matrix<double>.Build.Dense(4, 4);
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    m[i, j] = rot[i, j];
                }
            }
            m[0, 3] = trans[0];
            m[1, 3] = trans[1];
            m[2, 3] = trans[2];
            m[3, 3] = 1;
            return m;
        }
        public static Matrix<double> GetPosMatrixFromPosition(this Vector<double> pos)
        {
            var r1 = Matrix<double>.Build.Dense(3, 3);

            if (pos.Count == 7)
            {
                r1 = Vector<double>.Build.Dense(new double[] { pos[3], pos[4], pos[5], pos[6] }).Quat2Rotation();
            }
            else
            {
                r1 = Vector<double>.Build.Dense(new double[] { pos[3], pos[4], pos[5] }).Euler2Rotation();
            }
            var t1 = Vector<double>.Build.Dense(new double[] { pos[0], pos[1], pos[2] });
            var m1 = GetPosMatrixFromRotAndTrans(r1, t1);
            return m1;
        }
        
        public static bool IsJointInRange(double joint, JointRange jointRange, double tolerance = 2)
        {
            return joint < jointRange.UpperLimit - 2 && joint > jointRange.LowerLimit + 2;
        }
        public static bool IsJointsInRange(this Vector<double> joints, RobotJointsLimits robotJointsLimits)
        {
            return IsJointInRange(joints[0], robotJointsLimits.Joint1)
                   && IsJointInRange(joints[1], robotJointsLimits.Joint2)
                   && IsJointInRange(joints[2], robotJointsLimits.Joint3)
                   && IsJointInRange(joints[3], robotJointsLimits.Joint4)
                   && IsJointInRange(joints[4], robotJointsLimits.Joint5)
                   && IsJointInRange(joints[5], robotJointsLimits.Joint6);
        }
        
        public static Matrix<double> RobTarget2Matrix(this RobTarget robTarget)
        {
            var trans = Vector<double>.Build.DenseOfArray(new double[] { robTarget.Trans.X, robTarget.Trans.Y, robTarget.Trans.Z });
            var quad = Vector<double>.Build.DenseOfArray(new double[] { robTarget.Rot.Q1, robTarget.Rot.Q2, robTarget.Rot.Q3, robTarget.Rot.Q4 });
            return GetPosMatrixFromQuatAndTrans(quad, trans);
        }
        public static RobTarget Matrix2RobotTarget(this Matrix<double> m)
        {
            RobTarget robTarget = new RobTarget();
            robTarget.Trans.X = m[0, 3].ToFloat();
            robTarget.Trans.Y = m[1, 3].ToFloat();
            robTarget.Trans.Z = m[2, 3].ToFloat();
            var q = m.Rotation2Quad();
            robTarget.Rot.Q1 = q[0];
            robTarget.Rot.Q2 = q[1];
            robTarget.Rot.Q3 = q[2];
            robTarget.Rot.Q4 = q[3];
            return robTarget;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix<double> Euler2Rotation(this Vector<double> eulerAngles)
        {
            double roll = eulerAngles[2].ToRadian();
            double pitch = eulerAngles[1].ToRadian();
            double yaw = eulerAngles[0].ToRadian();

            double cr = Math.Cos(roll);
            double sr = Math.Sin(roll);
            double cp = Math.Cos(pitch);
            double sp = Math.Sin(pitch);
            double cy = Math.Cos(yaw);
            double sy = Math.Sin(yaw);
            Matrix<double> RIb = Matrix<double>.Build.Dense(3, 3);
            RIb[0, 0] = cy * cp;
            RIb[0, 1] = cy * sp * sr - sy * cr;
            RIb[0, 2] = sy * sr + cy * cr * sp;
            RIb[1, 0] = sy * cp;
            RIb[1, 1] = cy * cr + sy * sr * sp;
            RIb[1, 2] = sp * sy * cr - cy * sr;
            RIb[2, 0] = -sp;
            RIb[2, 1] = cp * sr;
            RIb[2, 2] = cp * cr;
            return RIb;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix<double> Quat2Rotation(this Vector<double> q)
        {
            Matrix<double> rot = Matrix<double>.Build.Dense(3, 3);
            rot[0, 0] = q[0] * q[0] + q[1] * q[1] - q[2] * q[2] - q[3] * q[3];
            rot[0, 1] = 2 * (q[1] * q[2] - q[0] * q[3]);
            rot[0, 2] = 2 * (q[1] * q[3] + q[0] * q[2]);

            rot[1, 0] = 2 * (q[1] * q[2] + q[0] * q[3]);
            rot[1, 1] = q[0] * q[0] - q[1] * q[1] + q[2] * q[2] - q[3] * q[3];
            rot[1, 2] = 2 * (q[2] * q[3] - q[0] * q[1]);

            rot[2, 0] = 2 * (q[1] * q[3] - q[0] * q[2]);
            rot[2, 1] = 2 * (q[2] * q[3] + q[0] * q[1]);
            rot[2, 2] = q[0] * q[0] - q[1] * q[1] - q[2] * q[2] + q[3] * q[3];
            return rot;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector<double> Rotation2Quad(this Matrix<double> R2)
        {
            int signM(double Ss)
            {
                if (Ss >= 0)
                    return 1;
                else
                    return -1;
            }

            var R = R2.SubMatrix(0, 3, 0, 3);
            var trace = R.Trace();
            if (trace < -1)
            {
                trace = -1;
            }

            var s = Math.Sqrt(trace + 1) / 2.0;
            var kx = R[2, 1] - R[1, 2]; // Oz-Ay
            var ky = R[0, 2] - R[2, 0]; // Ax-Nz
            var kz = R[1, 0] - R[0, 1]; // Ny-Ox

            //Equation 7
            var k = R.Diagonal().MaximumIndex();
            var sgn = 0;
            var kx1 = 0.0;
            var ky1 = 0.0;
            var kz1 = 0.0;
            switch (k)
            {
                case 1:
                    kx1 = R[0, 0] - R[1, 1] - R[2, 2] + 1; // Nx - Oy - Az + 1
                    ky1 = R[1, 0] + R[0, 1]; // Ny + Ox
                    kz1 = R[2, 0] + R[0, 2]; // Nz + Ax
                    sgn = signM(kx);
                    break;

                case 2:
                    kx1 = R[1, 0] + R[0, 1]; // Ny + Ox
                    ky1 = R[1, 1] - R[0, 0] - R[2, 2] + 1; // Oy - Nx - Az + 1
                    kz1 = R[2, 1] + R[1, 2]; // Oz + Ay
                    sgn = signM(ky);
                    break;
                case 3:
                    kx1 = R[2, 0] + R[0, 2]; // Nz + Ax
                    ky1 = R[2, 1] + R[1, 2]; // Oz + Zy
                    kz1 = R[2, 2] - R[0, 0] - R[1, 1] + 1; //Az - Nx - Oy + 1
                    sgn = signM(kz);
                    break;
            }

            //Equation 8
            kx = kx + sgn * kx1;
            ky = ky + sgn * ky1;
            kz = kz + sgn * kz1;

            var v = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.DenseOfArray(new double[] { kx, ky, kz });
            var nm = v.L2Norm();
            if (nm == 0) //% handle special case of null quaternion
            {
                s = 1;
                v[0] = 0;
                v[1] = 0;
                v[2] = 0;
            }
            else
            {
                v = v * Math.Sqrt(1 - s * s) / nm;
            }

            var q = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.DenseOfArray(new double[]
                { s, v[0], v[1], v[2] });

            return q;
        }
    }
}
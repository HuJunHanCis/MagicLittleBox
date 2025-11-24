using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using ABB.Robotics.Controllers.RapidDomain;
using OtherHelper;
using static OtherHelper.AaMath;
using static OtherHelper.AaAbbSolver;

namespace RobotHelper
{
    public class ArAbbSolver
    {
        
        public readonly double A1;
        public readonly double A2;
        public readonly double A3;
        public readonly double D1;
        public readonly double D4;
        public readonly double D6;
        public readonly Vector<double> JointsAlpha;
        public readonly Vector<double> JointsTheta;
        public readonly Vector<double> JointsA;
        public readonly Vector<double> JointsD;
        
        public ArAbbSolver(
            double a1 = 175, double a2 = 1095, double a3 = 175, 
            double d1 = 495, double d4 = 1270, double d6 = 135)
        {
            this.A1 = a1;
            this.A2 = a2;
            this.A3 = a3;
            this.D1 = d1;
            this.D4 = d4;
            this.D6 = d6;
            JointsAlpha = Vector<double>.Build.Dense(new double[] { -90, 0, -90, 90, -90, 0 });
            JointsTheta = Vector<double>.Build.Dense(new double[] { 0, -90, 0, 0, 0, 180 });
            JointsA = Vector<double>.Build.Dense(new double[] { a1, a2, a3, 0, 0, 0 });
            JointsD = Vector<double>.Build.Dense(new double[] { d1, 0, 0, d4, 0, d6 });
        }
        
        public static class RobotsJointsRange
        {
            public readonly static RobotJointsLimits Irb4600_40255 = new RobotJointsLimits(
                new JointRange(-180, 180, 175), 
                new JointRange(-90, 150, 175), 
                new JointRange(-180, 75, 175), 
                new JointRange(-400, 400, 250), 
                new JointRange(-125, 120, 250), 
                new JointRange(-400, 400, 360)
                );
        }
        
        public Matrix<double> Dh(int i, double thethai)
        {
            return DhOrigin(JointsAlpha[i], JointsA[i], JointsD[i], JointsTheta[i] + thethai);
        }
        
        public bool IsSingular(Vector<double> jointAngles, double tolerance=0.0001)
        {
            tolerance = tolerance*Math.Pow(10,9);
            var jac = Jacobian(jointAngles);
            return (Math.Abs(jac.Determinant()) < tolerance);
        }
        public bool IsSingular(float jointAngle1, float jointAngle2, float jointAngle3, float jointAngle4, float jointAngle5, float jointAngle6, double tolerance)
        {
            var jointAngles = Vector<float>.Build.Dense(new float[] { jointAngle1, jointAngle2, jointAngle3, jointAngle4, jointAngle5, jointAngle6 }).ToDouble();
            return IsSingular(jointAngles, tolerance);
        }
        
        public Matrix<double> ForwardKinematics(JointTarget jointTarget ,int axisNumber=6)
        {
            var jV = Vector<double>.Build.DenseOfArray(new double[] {jointTarget.RobAx.Rax_1, jointTarget.RobAx.Rax_2, jointTarget.RobAx.Rax_3,jointTarget.RobAx.Rax_4,jointTarget.RobAx.Rax_5,jointTarget.RobAx.Rax_6});
            return ForwardKinematics(jV,axisNumber);
        }
        public Matrix<double> ForwardKinematics(float[] jointsAngles, int axisNumber=6)
        {
            var jV = Vector<double>.Build.DenseOfArray(jointsAngles.Select(x=>Convert.ToDouble(x)).ToArray());
            return ForwardKinematics(jV, axisNumber);
        }
        public Matrix<double> ForwardKinematics(double[] jointsAngles, int axisNumber=6)
        {
            var jV = Vector<double>.Build.DenseOfArray(jointsAngles);
            return ForwardKinematics(jV, axisNumber);
        }
        public Matrix<double> ForwardKinematics(Vector<double> jointsAngles, int axisNumber = 6)
        {
            var thetes = jointsAngles + JointsTheta;
            return fkRobot(thetes, axisNumber);
        }
        
        private Matrix<double> fkRobot(Vector<double> thetas, int axisNumber)
        {
            Matrix<double> m = Matrix<double>.Build.DenseIdentity(4, 4);
            for (int i = 0; i < axisNumber; i++)
            {
                var m1 = DhOrigin(JointsAlpha[i], JointsA[i], JointsD[i], thetas[i]);
                m = m*m1;
            }
            return m;
        }
        
        public Matrix<double> Jacobian(Vector<double> thete)
        {
            var R0 = Matrix<double>.Build.DenseIdentity(3, 3);
            var z0 = Vector<double>.Build.Dense(new double[] { 0, 0, 1 });
            var Z = Matrix<double>.Build.Dense(3, 6, 0);
            var A = Matrix<double>.Build.DenseIdentity(4, 4);
            
            for (var i = 0; i < 6; i++)
            {
                var zi = R0 * z0;
                Z.SetColumn(i,zi);
                A = Dh(i, thete[i]);
                R0 = R0 * A.SubMatrix(0, 3, 0, 3);
            }
            
            var T = ForwardKinematics(thete);
            var pn = Matrix<double>.Build.Dense(3, 6, 0);
            var Ti = Matrix<double>.Build.DenseIdentity(4, 4);
            var v = Ti.Column(3).SubVector(0, 3);
            var v0 = Vector<double>.Build.Dense(4, 0);
            v0[3] = 1;
            for (var i = 0;i<6;i++)
            {
                pn.SetColumn(i, T.Column(3).SubVector(0,3)-v.SubVector(0,3));
                A = Dh(i, thete[i]);
                Ti = Ti * A;

                v = Ti * v0; 
            }
            var J = Matrix<double>.Build.Dense(6, 6, 0);
            for (var i = 0; i < 6; i++)
            {
                var v1 = Z.Column(i).CrossProduct3D(pn.Column(i));
                var v2 = Z.Column(i);
                var Ji = Vector<double>.Build.Dense(new double[] { v1[0], v1[1], v1[2], v2[0], v2[1], v2[2] });
                J.SetColumn(i, Ji);
            }
            return J;
        }
        
        private double[] SolveForTheta3(double q1, Vector<double> Pm)
        {
            var l2 = A2;
            var l3 = D4;
            var a2 = A3;
            var l4 = Math.Sqrt(a2 * a2 + l3 * l3);
            var phi = Math.Acos((a2 * a2 + l4 * l4 - l3 * l3) / (2 * a2 * l4)); //phi=acos((A2^2+L4^2-L3^2)/(2*A2*L4));
            //given euler is known, compute first DH transformation
            var t01 = Dh(0, q1);

            Vector<double> pm1 = Vector<double>.Build.Dense(4);
            pm1[0] = Pm[0];
            pm1[1] = Pm[1];
            pm1[2] = Pm[2];
            pm1[3] = 1;
            //Express Pm in the reference system 1, for convenience
            var p1 = t01.Inverse() * pm1;
            var r = Math.Sqrt(p1[0] * p1[0] + p1[1] * p1[1]);// sqrt(p1(1) ^ 2 + p1(2) ^ 2);
            var l = (l2 * l2 + l4 * l4 - r * r) / (2 * l2 * l4);
            if (l < -1 || l > 1)
            {
                return null;
            }
            var eta = Math.Acos(l);
            var q3_1 = Math.PI - phi - eta;
            var q3_2 = Math.PI - phi + eta;
            return new double[] { q3_1, q3_2 };
        }
        private double[] SolveForTheta2(double q1, Vector<double> Pm)
        {
            var d = JointsD;
            var a = JointsA;
            var l2 = A2;
            var l3 = Math.Sqrt(D4 * D4 + A3 * A3);
            var T01 = Dh(0, q1);
            Vector<double> pm1 = Vector<double>.Build.Dense(4);
            pm1[0] = Pm[0];
            pm1[1] = Pm[1];
            pm1[2] = Pm[2];
            pm1[3] = 1;
            var p1 = T01.Inverse() * pm1;
            var r = Math.Sqrt(p1[0] * p1[0] + p1[1] * p1[1]);
            var beta = Math.Atan2(-p1[1], p1[0]);
            var l = (l2 * l2 + r * r - l3 * l3) / (2 * r * l2);
            if (l < -1 || l > 1)
            {
                return null;
            }
            var gamma = Math.Acos(l);
            var q2_1 = Math.PI / 2 - beta - gamma;
            var q2_2 = Math.PI / 2 - beta + gamma;
            return new double[] { q2_1, q2_2 };
        }
        
        private double[] SolveSphericalWrist(Vector<double> q, Matrix<double> T, bool wrist, bool method = false)
        {
            // method = true: algebraic method
            double q4 = 0;
            double q5 = 0;
            double q6 = 0;
            if (!method)
            {
                var A01 = Dh(0, q[0].ToDegree());
                var A12 = Dh(1, q[1].ToDegree());
                var A23 = Dh(2, q[2].ToDegree());
                var Q = A23.Inverse() * A12.Inverse() * A01.Inverse() * T;
                // detect the degenerate case when q(5) = 0, this leads to zeros in Q13, Q23, Q31 and Q32 and Q33 = 1
                var thresh = 1e-12;
                // detect if q(5) == 0, this happens when cos(q5) in the matrix Q is close to 1
                if (Math.Abs(Q[2, 2] - 1) > thresh)
                {
                    //normal solution
                    if (!wrist) // wrsit==false, wrist up
                    {
                        q4 = Math.Atan2(Q[1, 2], Q[0, 2]);
                        q6 = Math.Atan2(-Q[2, 1], Q[2, 0]);
                    }
                    else //wrist down
                    {
                        q4 = Math.Atan2(Q[1, 2], Q[0, 2]) + Math.PI;
                        q6 = Math.Atan2(-Q[2, 1], Q[2, 0]) + Math.PI;
                    }

                    double cq5 = 0;
                    double sq5 = 0;
                    if (Math.Abs(Math.Cos(q6 + q4)) > thresh)
                    {
                        cq5 = (-Q[0, 0] - Q[1, 1]) / (Math.Cos(q4 + q6)) - 1;
                    }
                    if (Math.Abs(Math.Sin(q6 + q4)) > thresh)
                    {
                        cq5 = (Q[0, 1] - Q[1, 0]) / Math.Sin(q4 + q6) - 1;
                    }

                    if (Math.Abs(Math.Sin(q6)) > thresh)
                    {
                        sq5 = Q[2, 1] / Math.Sin(q6);
                    }
                    if (Math.Abs(Math.Cos(q6)) > thresh)
                    {
                        sq5 = -Q[2, 0] / Math.Cos(q6);
                    }
                    q5 = Math.Atan2(sq5, cq5);
                }
                else // degenerate solution, in this case, q4 cannot be determined, so q(4) = 0 is assigned
                {
                    if (!wrist) // wrist == false, wrist up
                    {
                        q4 = 0;
                        q5 = 0;
                        q6 = Math.Atan2(Q[0, 1] - Q[1, 0], -Q[0, 0] - Q[1, 1]);
                    }
                    else //wrist = true, wrist down
                    {
                        q4 = -Math.PI;
                        q5 = 0;
                        q6 = Math.Atan2(Q[0, 1] - Q[1, 0], -Q[0, 0] - Q[1, 1]) + Math.PI;
                    }
                }
            }
            else // Geometry method, has some problem that need to be solved; 有问题2024.1124
            {
                //Obtain the position and orientation of the system 3 using the already computed joints euler, q2 and q3
                double cq4 = 0;
                double sq4 = 0;
                double cq5 = 0;
                double sq5 = 0;
                double cq6 = 0;
                double sq6 = 0;
                var T01 = Dh(0, q[0].ToDegree());
                var T12 = Dh(1, q[1].ToDegree());
                var T23 = Dh(2, q[2].ToDegree());
                var T03 = T01 * T12 * T23;

                var x3 = Vector<double>.Build.Dense(new double[3] { T03[0, 0], T03[1, 0], T03[2, 0] });
                var y3 = Vector<double>.Build.Dense(new double[3] { T03[0, 1], T03[1, 1], T03[2, 1] });
                var z3 = Vector<double>.Build.Dense(new double[3] { T03[0, 2], T03[1, 2], T03[2, 2] });
                var a = Vector<double>.Build.Dense(new double[3] { T[0, 2], T[1, 2], T[2, 2] });
                //% find z4 normal to the plane formed by z3 and a
                var z4 = z3.CrossProduct3D(a);
                //if(z4==null)
                //{
                //    throw new Exception("参数故障");
                //}
                if (z4.L2Norm() < 0.000001)
                {
                    if (!wrist)
                    {
                        q4 = 0;
                    }
                    else
                    {
                        q4 = -Math.PI;
                    }
                }
                else
                {
                    cq4 = z4.DotProduct(-y3);
                    sq4 = z4.DotProduct(x3);
                    q4 = Math.Atan2(sq4, cq4);
                }
                //Solve for q5
                var T34 = Dh(3, q4.ToDegree());
                var T04 = T03 * T34;
                var x4 = Vector<double>.Build.Dense(new double[3] { T04[0, 0], T04[1, 0], T04[2, 0] });
                var y4 = Vector<double>.Build.Dense(new double[3] { T04[0, 1], T04[1, 1], T04[2, 1] });
                var z5 = Vector<double>.Build.Dense(new double[3] { T[0, 2], T[1, 2], T[2, 2] });
                cq5 = z5.DotProduct(y4);
                sq5 = z5.DotProduct(-x4);
                q5 = Math.Atan2(sq5, cq5);
                //Solve for q6
                var x6 = Vector<double>.Build.Dense(new double[3] { T[0, 0], T[1, 0], T[2, 0] });
                var T45 = Dh(4, q5.ToDegree());
                var T05 = T04 * T45;
                var x5 = Vector<double>.Build.Dense(new double[3] { T05[0, 0], T05[1, 0], T05[2, 0] });
                var y5 = Vector<double>.Build.Dense(new double[3] { T05[0, 1], T05[1, 1], T05[2, 1] });
                cq6 = x6.DotProduct(-x5);
                sq6 = x6.DotProduct(-y5);
                q6 = Math.Atan2(sq6, cq6);
                //Console.WriteLine(z4.ToMatrixString());

            }
            return new double[] { q4, q5, q6 };
        }
        
        public Matrix<double> InversKinematic(double[] pos)
        {
            Vector<double> p = Vector<double>.Build.DenseOfArray(pos);
            return InversKinematic(p);
        }
        public Matrix<double> InversKinematic(RobTarget robTarget)
        {
            double[] pos = new double[] { robTarget.Trans.X, robTarget.Trans.Y, robTarget.Trans.Z, robTarget.Rot.Q1, robTarget.Rot.Q2, robTarget.Rot.Q3, robTarget.Rot.Q4 };
            return InversKinematic(pos);
        }
        public Matrix<double> InversKinematic(Vector<double> pos)
        {
            var T = Matrix<double>.Build.Dense(4, 4);
            var euler = Vector<double>.Build.Dense(3);
            var quat = Vector<double>.Build.Dense(4);
            var trans = Vector<double>.Build.Dense(new double[3] { pos[0], pos[1], pos[2] });
            var rot = Matrix<double>.Build.Dense(4, 4);
            //euler
            if (pos.Count == 6)
            {
                euler[0] = pos[3];
                euler[1] = pos[4];
                euler[2] = pos[5];
                rot = euler.Euler2Rotation();

            }
            else if (pos.Count == 7)
            {
                quat[0] = pos[3];
                quat[1] = pos[4];
                quat[2] = pos[5];
                quat[3] = pos[6];
                rot = quat.Quat2Rotation();
            }
            T = GetPosMatrixFromRotAndTrans(rot, trans);
            return InversKinematic(T);
        }
        public Matrix<double> InversKinematic(Matrix<double> T)
        {
            var L6 = D6;

            Vector<double> Pm = Vector<double>.Build.Dense(new double[3] { T[0, 3], T[1, 3], T[2, 3] });
            Vector<double> W = Vector<double>.Build.Dense(new double[3] { T[0, 2], T[1, 2], T[2, 2] });
            Pm = Pm - L6 * W;
            var q1 = Math.Atan2(Pm[1], Pm[0]);

            var q2_1 = SolveForTheta2(q1.ToDegree(), Pm);
            // euler+ Pi also a solution
            var q2_2 = SolveForTheta2((q1 + Math.PI).ToDegree(), Pm);
            if (q2_1 == null && q2_2 == null)
            {
                return null;
            }
            var q3_1 = SolveForTheta3(q1.ToDegree(), Pm);
            var q3_2 = SolveForTheta3((q1 + Math.PI).ToDegree(), Pm);
            if (q3_1 == null && q3_2 == null)
            {
                return null;
            }
            List<Vector<double>> p = new List<Vector<double>>();
            //八种可能的解
            if (q2_1 != null && q3_1 != null)
            {
                p.Add(Vector<double>.Build.Dense(new double[6] { q1, q2_1[0], q3_1[0], 0.0, 0.0, 0.0 }));
                p.Add(Vector<double>.Build.Dense(new double[6] { q1, q2_1[0], q3_1[0], 0, 0, 0 }));
                p.Add(Vector<double>.Build.Dense(new double[6] { q1, q2_1[1], q3_1[1], 0, 0, 0 }));
                p.Add(Vector<double>.Build.Dense(new double[6] { q1, q2_1[1], q3_1[1], 0, 0, 0 }));
            }
            if (q2_2 != null && q3_2 != null)
            {
                p.Add(Vector<double>.Build.Dense(new double[6] { q1 + Math.PI, q2_2[0], q3_2[0], 0, 0, 0 }));
                p.Add(Vector<double>.Build.Dense(new double[6] { q1 + Math.PI, q2_2[0], q3_2[0], 0, 0, 0 }));
                p.Add(Vector<double>.Build.Dense(new double[6] { q1 + Math.PI, q2_2[1], q3_2[1], 0, 0, 0 }));
                p.Add(Vector<double>.Build.Dense(new double[6] { q1 + Math.PI, q2_2[1], q3_2[1], 0, 0, 0 }));
            }
            foreach (var item in p)
            {
                for (int i = 0; i < 3; i++)
                {
                    item[i] = item[i].NormaliseAngleInRadian();
                }
            }

            for (int i = 0; i < p.Count;)
            {
                var q456 = SolveSphericalWrist(p[i], T, false);
                p[i][3] = q456[0];
                p[i][4] = q456[1];
                p[i][5] = q456[2];
                q456 = SolveSphericalWrist(p[i], T, true);
                p[i + 1][3] = q456[0];
                p[i + 1][4] = q456[1];
                p[i + 1][5] = q456[2];
                i += 2;
            }
            Matrix<double> Q = Matrix<double>.Build.Dense(6, p.Count);
            for (int i = 0; i < p.Count; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    p[i][j] = p[i][j].NormaliseAngleInRadian().ToDegree();
                }
                Q.SetColumn(i, p[i]);
            }
            return Q;
        }
        
        public bool IsRobTargetReachable(RobTarget target)
        {
            var solutionMatrix = InversKinematic(target.RobTarget2Matrix());
            if (solutionMatrix == null)
            { return false; }
            for (int i = 0; i < solutionMatrix.ColumnCount; i++)
            {
                var joints = solutionMatrix.Column(i);
                if (joints.IsJointsInRange(RobotsJointsRange.Irb4600_40255))
                {
                    if(!IsSingular(joints,0.0003))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

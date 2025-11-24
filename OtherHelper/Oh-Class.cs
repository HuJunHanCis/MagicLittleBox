using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace OtherHelper
{
    // 四元数+XYZ坐标
    public struct RobotPointTrans
    {
        public double Rx; //X-coordinate
        public double Ry; //Y-coordinate
        public double Rz; //Z-coordinate
        public double Q0; //First component of the orientation quaternion
        public double Qx; //Second component of the orientation quaternion
        public double Qy; //Third component of the orientation quaternion
        public double Qz; //Fourth component of the orientation quaternion

        public RobotPointTrans(double x, double y, double z, double q0, double qx, double qy, double qz)
        {
            Rx = x;
            Ry = y;
            Rz = z;
            Q0 = q0;
            Qx = qx;
            Qy = qy;
            Qz = qz;
        }
    }
    
    // 六轴角度
    public struct RobotPointJoint
    {
        public double R1;
        public double R2;
        public double R3;
        public double R4;
        public double R5;
        public double R6;
        
        public RobotPointJoint(double rax1, double rax2, double rax3, double rax4, double rax5, double rax6)
        {
            R1 = rax1;
            R2 = rax2;
            R3 = rax3;
            R4 = rax4;
            R5 = rax5;
            R6 = rax6;
        }
    }
    
    [Serializable]
    public struct JointRange
    {
        public double LowerLimit;
        public double UpperLimit;
        public double MaxSpeed;
        public JointRange(double lowerLimit, double upperLimit, double maxSpeed)
        {
            LowerLimit = lowerLimit;
            UpperLimit = upperLimit;
            MaxSpeed = maxSpeed;
        }
    }
    
    [Serializable]
    public struct RobotJointsLimits
    {
        readonly public JointRange Joint1;
        readonly public JointRange Joint2;
        readonly public JointRange Joint3;
        public readonly JointRange Joint4;
        public readonly JointRange Joint5;
        public readonly JointRange Joint6;
        public RobotJointsLimits(JointRange joint1, JointRange joint2, JointRange joint3, JointRange joint4, JointRange joint5, JointRange joint6)
        {
            Joint1 = joint1;
            Joint2 = joint2;
            Joint3 = joint3;
            Joint4 = joint4;
            Joint5 = joint5;
            Joint6 = joint6;
        }
    }
    
    [Serializable]
    public struct TrussPoint
    {
        public double Tx; //X-coordinate
        public double Ty; //Y-coordinate
        public double Tz; //Z-coordinate
        
        public TrussPoint(double x, double y, double z = 0)
        {
            Tx = x;
            Ty = y;
            Tz = z;
        }
    }
    
    public enum AbbInstructionCode
    {
        Home=1,
        StartToCut=2,
        Ignite=3,
        
        SetCurrentSpeed=4,
        SetOneCuttingSpeed=5,
        MoveOneCartisian = 6,
        MoveOneJoint = 7,
        MoveJ = 8,
        DoNothing = 0
    }
    
    // 全局坐标
    public struct PointOfPath
    {
        public RobotPointTrans? RobotPosTrans; //Position Of Robot
        public RobotPointJoint? RobotPosJoint; //Position Of Robot
        public TrussPoint TrussPos; //Position Of Truss
        // public double SpaceX; //X-coordinate in space axis
        // public double SpaceY; //Y-coordinate in space axis
        // public double SpaceZ; //Z-coordinate in space axis
        
        // public PointOfPath(RobotPointTrans rpt, RobotPointJoint rpj, TrussPoint tp)
        // {
        //     RobotPosTrans = rpt;
        //     RobotPosJoint = rpj;
        //     TrussPos = tp;
        //     // SpaceX = rpt.Rx + tp.Tx;
        //     // SpaceY = rpt.Ry + tp.Ty;
        //     // SpaceZ = rpt.Rz + tp.Tz;
        // }
        
        // 构造函数1：使用RobotPointTrans
        public PointOfPath(RobotPointTrans rpt, TrussPoint tp)
        {
            RobotPosTrans = rpt;
            RobotPosJoint = null;
            TrussPos = tp;
        }

        // 构造函数2：使用RobotPointJoint
        public PointOfPath(RobotPointJoint rpj, TrussPoint tp)
        {
            RobotPosTrans = null;
            RobotPosJoint = rpj;
            TrussPos = tp;
        }
    }
    
    public enum EnumType
    {
        [Description("未知类型")] Unknown = 0,
        [Description("板坯")] Cube = 1,
        [Description("大卷套小卷")] BigTaoSmall = 2,
        [Description("小卷套大卷")] SmallTaoBig = 3,
        [Description("外凸卷")] Concave = 4,
        [Description("厚卷")] Thick = 5,
        [Description("带皮卷")] Skin = 6
    }

    public class AllPath
    {
        public List<ItemPath> ItemPaths { get; set; }

        public AllPath(List<ItemPath> itemPaths)
        {
            ItemPaths = itemPaths;
        }

        public AllPath()
        {
            ItemPaths = new List<ItemPath>();
        }
    }

    public struct ItemPath
    {
        public EnumType Type;
        public float Diameter;
        public float Length;
        
        public List<int> IndexS;
        public List<int> IndexE;
        public List<float> FireAngle;

        public List<List<CuttingPoint>> CuttingPoints;

        public ItemPath(
            EnumType type,
            float diameter,
            float length,
            List<int> indexS,
            List<int> indexE,
            List<float> fireAngle,
            List<List<CuttingPoint>> cuttingPoints)
        {
            Type = type;
            Diameter = diameter;
            Length = length;
            IndexS = indexS;
            IndexE = indexE;
            FireAngle = fireAngle;
            CuttingPoints = cuttingPoints;
        }
    }

    public struct CuttingPoint
    {
        public double SpaceX;
        public double SpaceY;
        public double SpaceZ;
        public int PreheatTime;
        public float Thick;

        public CuttingPoint(double rx, double ry, double rz, int preheatTime, float thick)
        {
            SpaceX = rx;
            SpaceY = ry;
            SpaceZ = rz;
            PreheatTime = preheatTime;
            Thick = thick;
        }
    }
}
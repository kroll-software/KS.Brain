/*
{*******************************************************************}
{                                                                   }
{       KS-Neuron DotNet Library                                    }
{                                                                   }
{       Copyright (c) 2010 - 2016 by Kroll-Software                 }
{       All Rights Reserved                                         }
{                                                                   }
{   You receive this source code for educational                    }
{   and for research purposes only.                                 }
{                                                                   }
{   You may not use this code or any derived work for               }
{   any applications, except for research and studies.              }
{                                                                   }
{   The intention for publishing this is to document                }
{   the invention by Detlef Kroll.  Altdorf / Switzerland           }
{                                                                   } 
{   You are invited to discuss this with me.                        } 
{   Email to kroll@kroll-software.ch                                } 
{                                                                   } 
{*******************************************************************}
*/

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Security.Permissions;
using KS.Foundation;

namespace KS.Brain
{
    //[Serializable]
    //[TypeConverter(typeof(ExpandableObjectConverter))]
    //public class Location
    //{
    //    private float m_X;
    //    private float m_Y;
    //    private float m_Z;

    //    public Location()
    //    {
    //    }

    //    public Location(float x, float y, float z)
    //    {
    //        m_X = x;
    //        m_Y = y;
    //        m_Z = z;
    //    }        

    //    public float X
    //    {
    //        get
    //        {
    //            return m_X;
    //        }
    //        set
    //        {
    //            m_X = value;
    //        }
    //    }

    //    public float Y
    //    {
    //        get
    //        {
    //            return m_Y;
    //        }
    //        set
    //        {
    //            m_Y = value;
    //        }
    //    }

    //    public float Z
    //    {
    //        get
    //        {
    //            return m_Z;
    //        }
    //        set
    //        {
    //            m_Z = value;
    //        }
    //    }

    //    // 3. Wurzel
    //    private double Sqr3(double f)
    //    {
    //        return Math.Pow(f, 1.0 / 3.0);
    //    }

    //    // 3. Potenz
    //    private double Pot3(float f)
    //    {
    //        return f * f * f;
    //    }

    //    public double Distance(Location l)
    //    {
    //        return Sqr3(Pot3(Math.Abs(m_X - l.X)) + Pot3(Math.Abs(m_Y - l.Y)) + Pot3(Math.Abs(m_Z - l.Z)));

    //        //return Math.Sqrt(Math.Pow(Math.Abs(m_X - l.X), 2) + Math.Pow(Math.Abs(m_Y - l.Y), 2));
    //    }        
    //}

    public sealed class globals
    {
        // Configuration
        public static float[] ActionPotentials = {0.5f, 1.5f, 3.0f, 2.5f, 0.0f, -1f};               

        // 0 = no limit
        public static float MaxEmphasis = 0f;        

        internal static long CachedNeuronCount = -1;

        // global statics
        internal static long m_Iteration = 0;
        public static long Iteration
        {
            get
            {
                return m_Iteration;
            }
        }

        public void IncreaseIteration()
        {
            m_Iteration++;
        }

        internal static int m_ConnectionCount = 0;
        public static int ConnectionCount
        {
            get
            {
                return m_ConnectionCount;
            }
        }

        public static void IncreaseConnectionCount()
        {
            m_ConnectionCount++;
        }

        public static void DecreaseConnectionCount()
        {
            m_ConnectionCount--;
        }

        internal static double m_TotalEmphasis = 0.0;
        public static double TotalEmphasis
        {
            get
            {
                return m_TotalEmphasis;
            }
        }

        public static void IncreaseTotalEmphasis(double Increment)
        {
            m_TotalEmphasis += Increment;
        }
                
        public static void ResetGlobalStatics()
        {
            m_ConnectionCount = 0;
            m_TotalEmphasis = 0.0;
            m_Iteration = 0;
            m_SecondsRun = 0;

            m_AutoFireFactor = 1f;
            m_GlobalThreshold = 3f;
            m_GlobalAwakeness = 1.0;
        }


        private static float m_AutoFireFactor = 1f;
		public static float AutoFireFactor
        {
            get
            {
                return m_AutoFireFactor;
            }
            set
            {
                m_AutoFireFactor = value;
            }
        }

        private static float m_GlobalThreshold = 1f;
        public static float GlobalThreshold
        {
            get
            {
                return m_GlobalThreshold;
            }
            set
            {
                m_GlobalThreshold = value;
            }
        }

        private static double m_SecondsRun = 0;
        public static double SecondsRun
        {
            get
            {
                return m_SecondsRun;
            }
            set
            {
                m_SecondsRun = value;
            }
        }

        
        //public static double AdditionalThreshold = 0.0;

        // > 0 : Awake (wait for input, raise neuron activity potentials)
        // < 0 : Sleep (lern, autofire, Hebbsche Lernregel)
        private static double m_GlobalAwakeness = 1.0;
        public static double GlobalAwakeness
        {
            get
            {
                return m_GlobalAwakeness;
            }
            set
            {
                m_GlobalAwakeness = value;
            }
        }

        public static bool IsAwake
        {
            get
            {
                return m_GlobalAwakeness > 0.0;
            }
        }

        public static bool IsSleeping
        {
            get
            {
                return m_GlobalAwakeness <= 0.0;
            }
        }
    }
}

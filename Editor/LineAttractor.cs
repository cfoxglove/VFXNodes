using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.VFX.Block
{
    [VFXInfo(category = "Force")]
    class FLineAttractor : VFXBlock
    {
        public enum ForceMode
        {
            Absolute,
            Relative
        }

        public enum FalloffMode
        {
            None,
            LinearDistance,
            QuadraticDistance,
            InverseLinear,
            InverseQuadratic
        }


        [VFXSetting]
        public ForceMode Mode = ForceMode.Absolute;

        [VFXSetting]
        public FalloffMode FalloffType = FalloffMode.None;

        public override string name { get { return "LineAttractor"; } }
        public override VFXContextType compatibleContexts { get { return VFXContextType.kUpdate; } }
        public override VFXDataType compatibleData { get { return VFXDataType.kParticle; } }

        public override IEnumerable<VFXNamedExpression> parameters
        {
            get
            {
                foreach (var p in GetExpressionsFromSlots(this))
                    yield return p;

                yield return new VFXNamedExpression(VFXBuiltInExpression.DeltaTime, "deltaTime");
            }
        }

        public override IEnumerable<VFXAttributeInfo> attributes
        {
            get
            {
                yield return new VFXAttributeInfo(VFXAttribute.Velocity, VFXAttributeMode.ReadWrite);
                yield return new VFXAttributeInfo(VFXAttribute.Position, VFXAttributeMode.Read);
                yield return new VFXAttributeInfo(VFXAttribute.Mass, VFXAttributeMode.Read);
            }
        }

        public class InputProperties
        {
            [Tooltip("Strength of the attracting force")]
            public float Strength = 1.0f;

            [Tooltip("Center point of the line")]
            public Vector3 Center = new Vector3(0.0f, 0.0f, 0.0f);

            [Tooltip("Vector defining the line direction")]
            public Vector3 LineDirection = new Vector3(0.0f, 0.0f, 0.0f);
        }

        public override string source
        {
            get {
                string forceVector = "0.0";

                string preamble = @"float3 v = (dot((position), LineDirection) / length(LineDirection) * LineDirection) - position;
float d = length(v);

";

                forceVector = "(Strength * normalize(v))";


                switch(FalloffType)
                {
                    case FalloffMode.None:
                        break;
                    case FalloffMode.LinearDistance:
                        forceVector = "((Strength / d) * normalize(v))";
                        break;
                    case FalloffMode.QuadraticDistance:
                        forceVector = "((Strength / (d * d)) * normalize(v))";
                        break;
                    case FalloffMode.InverseLinear:
                        forceVector = "((Strength * d) * normalize(v))";
                        break;
                    case FalloffMode.InverseQuadratic:
                        forceVector = "((Strength * d * d) * normalize(v))";
                        break;
                }

                switch (Mode)
                {
                    case ForceMode.Absolute:
                        forceVector = "deltaTime * " + forceVector + " / mass"; //"(Force / mass) * deltaTime";
                        break;
                    case ForceMode.Relative:
                        forceVector = "(" + forceVector + " - velocity) * min(1.0f,deltaTime / mass)";
                        break;
                }

                return preamble + "velocity += " + forceVector + ";";
            }
        }
    }
}

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.VFX.Block
{
    [VFXInfo(category = "Force")]
    class FJet : VFXBlock
    {
        public enum ForceMode
        {
            Absolute,
            Relative
        }

        [VFXSetting]
        public ForceMode Mode = ForceMode.Absolute;

        public override string name { get { return "Jet"; } }
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
            [Tooltip("Defines the sharpness / shape of the jet")]
            public float Shape = 3.0f;

            [Tooltip("Strength of the jet force")]
            public float Strength = 1.0f;

            [Tooltip("Origin of the Jet")]
            public Vector3 Center = new Vector3(0.0f, 0.0f, 0.0f);

            [Tooltip("Direction of the Jet")]
            public Vector3 LineDirection = new Vector3(1.0f, 0.0f, 0.0f);
        }

        public override string source
        {
            get {
                string forceVector = "0.0";

                string preamble = @"float pDotL = max(dot(normalize(position - Center), LineDirection), 0.0);
";

                forceVector = "pow(pDotL, Shape) * Strength * LineDirection";

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

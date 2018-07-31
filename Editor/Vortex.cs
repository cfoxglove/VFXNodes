using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.VFX.Block
{
    [VFXInfo(category = "Force")]
    class FVortex : VFXBlock
    {
        public enum ForceMode
        {
            Absolute,
            Relative
        }

        public enum RotationDirection {
            Clockwise,
            CounterClockwise
        }

        [VFXSetting]
        public ForceMode Mode = ForceMode.Absolute;
        public RotationDirection RotationMode = RotationDirection.Clockwise;

        public override string name { get { return "Vortex"; } }
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
                yield return new VFXAttributeInfo(VFXAttribute.Position, VFXAttributeMode.ReadWrite);
                yield return new VFXAttributeInfo(VFXAttribute.Mass, VFXAttributeMode.Read);
            }
        }

        public class InputProperties
        {
            [Tooltip("Strength of the rotational force")]
            public float RotationStrength = 1.0f;

            [Tooltip("Strength of the force pulling toward the Vortex Axis.")]
            public float AxialStrength = 1.0f;

            [Tooltip("Axis perpendicular to the vortex rotation")]
            public Vector3 VortexAxis = new Vector3(1.0f, 0.0f, 0.0f);

            [Tooltip("Center point of the vortex.")]
            public Vector3 VortexCenter = new Vector3(0.0f, 0.0f, 0.0f);
        }

        public override string source
        {
            get {
                string forceVector = "0.0";

                switch (RotationMode)
                {
                    case RotationDirection.Clockwise:
                        forceVector = "RotationStrength * normalize(cross(VortexAxis, (position - VortexCenter)))";
                        break;
                    case RotationDirection.CounterClockwise:
                        forceVector = "RotationStrength * normalize(cross((position - VortexCenter), VortexAxis))";
                        break;
                }

                forceVector = forceVector + " + AxialStrength * (position - VortexAxis * dot(position - VortexCenter, VortexAxis) / length(VortexAxis))";

                forceVector = "deltaTime * " + forceVector + " / mass"; //"(Force / mass) * deltaTime";

                /*switch (Mode)
                {
                    case ForceMode.Absolute:
                        forceVector = "deltaTime * " + forceVector + " / mass"; //"(Force / mass) * deltaTime";
                        break;
                    case ForceMode.Relative:
                        forceVector = "(Force - velocity) * min(1.0f,deltaTime / mass)";
                        break;
                }*/

                //return "velocity += " + forceVector + ";";

                Debug.Log(forceVector);
                return "velocity += " + forceVector + ";";
            }
        }
    }
}

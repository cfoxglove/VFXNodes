using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.VFX.Operator
{
    [VFXInfo(category = "Math/Wave")]
    class CubicPulse : VFXOperatorNumericUnified, IVFXOperatorNumericUnifiedConstrained
    {
        public class InputProperties
        {
            [Tooltip("x-axis input (value) driving the pulse")]
            public float input = 0.5f;
            [Tooltip("center point of the pulse along the x-axis")]
            public float center = 0.5f;
            [Tooltip("width of the pulse")]
            public float width = 0.2f;
        }

        public override sealed string name { get { return "Cubic Pulse"; } }

        public IEnumerable<int> slotIndicesThatMustHaveSameType
        {
            get
            {
                return Enumerable.Range(0, 4);
            }
        }

        public IEnumerable<int> slotIndicesThatCanBeScalar
        {
            get
            {
                return Enumerable.Range(0, 3);
            }
        }

        protected sealed override ValidTypeRule typeFilter
        {
            get
            {
                return ValidTypeRule.allowEverythingExceptInteger;
            }
        }

        protected override sealed VFXExpression[] BuildExpression(VFXExpression[] inputExpression)
        {
            // https://thebookofshaders.com/edit.php#05/cubicpulse.frag
            // x0 = clamp(x, c - w, c + w)
            // 1.0 - (abs(x0 - c) / w)^2 * (3.0 - 2.0 * abs(x0 - c) / w)

            //first, clamp the input
            var clamp_x = VFXOperatorUtility.Clamp(inputExpression[0], inputExpression[1] - inputExpression[2], inputExpression[1] + inputExpression[2]);

            var a = new VFXExpressionAbs(clamp_x - inputExpression[1]) / inputExpression[2];
            var b = new VFXValue<float>(3.0f) - new VFXExpressionMul(new VFXValue<float>(2.0f), a);
            var res = new VFXValue<float>(1.0f) - a * a * b;
            return new[] { res };
        }
    }
}

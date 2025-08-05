// Copyright 2020 The Tilt Brush Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
#define USE_TILT_BRUSH_CPP // Specifies that some functions will use TiltBrushCpp.dll.
#endif

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace TiltBrush
{
    static public class MathUtils
    {
        static public class TiltBrushCpp
        {
#if USE_TILT_BRUSH_CPP
            [DllImport("TiltBrushCpp")] unsafe public static extern void TransformVector3AsPoint(
                Matrix4x4 mat, int iVert, int iVertEnd, Vector3* v3);
            [DllImport("TiltBrushCpp")] unsafe public static extern void TransformVector3AsVector(
                Matrix4x4 mat, int iVert, int iVertEnd, Vector3* v3);
            [DllImport("TiltBrushCpp")] unsafe public static extern void TransformVector3AsZDistance(
                float scale, int iVert, int iVertEnd, Vector3* v3);
            [DllImport("TiltBrushCpp")] unsafe public static extern void TransformVector4AsPoint(
                Matrix4x4 mat, int iVert, int iVertEnd, Vector4* v4);
            [DllImport("TiltBrushCpp")] unsafe public static extern void TransformVector4AsVector(
                Matrix4x4 mat, int iVert, int iVertEnd, Vector4* v4);
            [DllImport("TiltBrushCpp")] unsafe public static extern void TransformVector4AsZDistance(
                float scale, int iVert, int iVertEnd, Vector4* v4);
            [DllImport("TiltBrushCpp")] unsafe public static extern void GetBoundsFor(
                Matrix4x4 m, int iVert, int iVertEnd, Vector3* v3, Vector3* center, Vector3* size);
#endif

        }

        /// Decomposes a matrix into T, R, and uniform scale.
        ///
        /// It is an error to pass a matrix that cannot be decomposed this way;
        /// in particular, it is an error to pass a matrix with non-uniform scale.
        /// This error will pass undetected, and you will get undefined results.
        ///
        /// Extraction of uniform scale from the matrix will have small
        /// floating-point errors.
        static public void DecomposeMatrix4x4(
            Matrix4x4 m,
            out Vector3 translation,
            out Quaternion rotation,
            out float uniformScale)
        {
            translation = m.GetColumn(3);
            Vector3 fwd = m.GetColumn(2); // shorthand for m * Vector3.forward
            Vector3 up = m.GetColumn(1);  // shorthand for m * Vector3.up

            // Use triple product to determine if det(m) < 0 (detects a mirroring)
            float scaleSign = Mathf.Sign(Vector3.Dot(m.GetColumn(0),
                Vector3.Cross(m.GetColumn(1),
                    m.GetColumn(2))));
            rotation = Quaternion.LookRotation(fwd * scaleSign, up * scaleSign);

            // Which axis (or row) to use is arbitrary, but I'm going to standardize
            // on using the x axis.
            double x0 = m.m00;
            double x1 = m.m10;
            double x2 = m.m20;
            uniformScale = (float)Math.Sqrt(x0 * x0 + x1 * x1 + x2 * x2) * scaleSign;
        }

        /// Returns a qDelta such that:
        /// - qDelta's axis of rotation is |axis|
        /// - q1 ~= qDelta * q0 (as much as is possible, given the constraint)
        ///
        /// NOTE: for convenience, ensures quats are in the same hemisphere.
        /// This means that if you really do want to examine the "long way around" rotation
        /// (ie, delta angle > 180) then this function will do the wrong thing.
        static public Quaternion ConstrainRotationDelta(Quaternion q0, Quaternion q1, Vector3 axis)
        {
            // Bad things happen if they're not in the same hemisphere (rotation
            // goes the long way around and contains too much of "axis")
            if (Quaternion.Dot(q0, q1) < 0)
            {
                q1 = q1.Negated();
            }

            axis = axis.normalized;
            var adjust = q1 * Quaternion.Inverse(q0);
            // Constrain rotation to passed axis
            Vector3 lnAdjust = adjust.Log().Im();
            lnAdjust = axis * (Vector3.Dot(axis, lnAdjust));
            return new Quaternion(lnAdjust.x, lnAdjust.y, lnAdjust.z, 0).Exp();
        }

        // A simplified version of TwoPointObjectTransformation. The following properties are true:
        //   1. The object-local-space direction between the left and right hands remains constant.
        //   2. The object-local-space position of LerpUnclamped(left, right, constraintPositionT) remains
        //      constant.
        //   3. obj1 has the same scale as obj0.
        //   4. (Corollary of 1-3) The object-local-space positions of left and right remain constant, if
        //      the distance between them does not change.
        public static TrTransform TwoPointObjectTransformationNoScale(
            TrTransform gripL0, TrTransform gripR0,
            TrTransform gripL1, TrTransform gripR1,
            TrTransform obj0, float constraintPositionT)
        {
            // Vectors from left-hand to right-hand
            Vector3 vLR0 = (gripR0.translation - gripL0.translation);
            Vector3 vLR1 = (gripR1.translation - gripL1.translation);

            Vector3 pivot0;
            TrTransform xfDelta;
            {
                pivot0 = Vector3.LerpUnclamped(gripL0.translation, gripR0.translation, constraintPositionT);
                var pivot1 = Vector3.LerpUnclamped(gripL1.translation, gripR1.translation, constraintPositionT);
                xfDelta.translation = pivot1 - pivot0;

                xfDelta.translation = Vector3.LerpUnclamped(
                    gripL1.translation - gripL0.translation,
                    gripR1.translation - gripR0.translation,
                    constraintPositionT);
                // TODO: check edge cases:
                // - |vLR0| or |vLR1| == 0 (ie, from and/or to are undefined)
                // - vLR1 == vLR0 * -1 (ie, infinite number of axes of rotation)
                xfDelta.rotation = Quaternion.FromToRotation(vLR0, vLR1);
                xfDelta.scale = 1;
            }

            Quaternion deltaL = ConstrainRotationDelta(gripL0.rotation, gripL1.rotation, vLR0);
            Quaternion deltaR = ConstrainRotationDelta(gripR0.rotation, gripR1.rotation, vLR0);
            xfDelta = TrTransform.R(Quaternion.Slerp(deltaL, deltaR, 0.5f)) * xfDelta;

            // Set pivot point
            xfDelta = xfDelta.TransformBy(TrTransform.T(pivot0));
            return xfDelta * obj0;
        }

        // Helper for TwoPointObjectTransformationNonUniformScale.
        // Scale the passed position along axis, about center.
        private static Vector3 ScalePosition(
            Vector3 position, float amount, Vector3 scaleCenter, Vector3 axis)
        {
            Vector3 relativePosition = position - scaleCenter;
            // Decompose relativePosition into vAlong and vAcross
            Vector3 vAlong = Vector3.Dot(relativePosition, axis) * axis;
            Vector3 vAcross = relativePosition - vAlong;
            // Recompose relativePosition, scaling the "vAlong" portion
            vAlong *= amount;
            relativePosition = vAlong + vAcross;
            return scaleCenter + relativePosition;
        }

        /// Solve the quadratic equation.
        /// Returns false and NaNs if there are no (real) solutions.
        /// Otherwise, returns true.
        /// It's guaranteed that r0 <= r1.
        static public bool SolveQuadratic(float a, float b, float c, out float r0, out float r1)
        {
            // See https://people.csail.mit.edu/bkph/articles/Quadratics.pdf
            float discriminant = b * b - 4 * a * c;
            if (discriminant < 0)
            {
                r0 = r1 = float.NaN;
                return false;
            }
            float q = -.5f * (b + Mathf.Sign(b) * Mathf.Sqrt(discriminant));
            float ra = q / a;
            float rb = c / q;
            if (ra < rb)
            {
                r0 = ra;
                r1 = rb;
            }
            else
            {
                r0 = rb;
                r1 = ra;
            }
            return true;
        }

        /// Transform a subset of an array of Vector3 elements as points.
        /// Pass:
        ///   m        - the transform to apply.
        ///   iVert    - start of verts to transform
        ///   iVertEnd - end (not inclusive) of verts to transform
        ///   v3       - the array of vectors to transform
        public static void TransformVector3AsPoint(Matrix4x4 mat, int iVert, int iVertEnd,
                                                   Vector3[] v3)
        {
#if USE_TILT_BRUSH_CPP
            unsafe
            {
                fixed (Vector3* v3Fixed = v3)
                {
                    TiltBrushCpp.TransformVector3AsPoint(mat, iVert, iVertEnd, v3Fixed);
                }
            }
#else
            for (int i = iVert; i < iVertEnd; i++)
            {
                v3[i] = mat.MultiplyPoint(v3[i]);
            }
#endif
        }

        /// Transform a subset of an array of Vector3 elements as vectors.
        /// Pass:
        ///   m        - the transform to apply.
        ///   iVert    - start of verts to transform
        ///   iVertEnd - end (not inclusive) of verts to transform
        ///   v3       - the array of vectors to transform
        public static void TransformVector3AsVector(Matrix4x4 mat, int iVert, int iVertEnd,
                                                    Vector3[] v3)
        {
#if USE_TILT_BRUSH_CPP
            unsafe
            {
                fixed (Vector3* v3Fixed = v3)
                {
                    TiltBrushCpp.TransformVector3AsVector(mat, iVert, iVertEnd, v3Fixed);
                }
            }
#else
            for (int i = iVert; i < iVertEnd; i++)
            {
                v3[i] = mat.MultiplyVector(v3[i]);
            }
#endif
        }

        /// Transform a subset of an array of Vector3 elements as z distances.
        /// Pass:
        ///   m        - the transform to apply.
        ///   iVert    - start of verts to transform
        ///   iVertEnd - end (not inclusive) of verts to transform
        ///   v3       - the array of vectors to transform
        public static void TransformVector3AsZDistance(float scale, int iVert, int iVertEnd,
                                                       Vector3[] v3)
        {
#if USE_TILT_BRUSH_CPP
            unsafe
            {
                fixed (Vector3* v3Fixed = v3)
                {
                    TiltBrushCpp.TransformVector3AsZDistance(scale, iVert, iVertEnd, v3Fixed);
                }
            }
#else
            for (int i = iVert; i < iVertEnd; i++)
            {
                v3[i] = new Vector3(v3[i].x, v3[i].y, scale * v3[i].z);
            }
#endif
        }

        /// Transform a subset of an array of Vector4 elements as points.
        /// Pass:
        ///   m        - the transform to apply.
        ///   iVert    - start of verts to transform
        ///   iVertEnd - end (not inclusive) of verts to transform
        ///   v4       - the array of vectors to transform
        public static void TransformVector4AsPoint(Matrix4x4 mat, int iVert, int iVertEnd,
                                                   Vector4[] v4)
        {
#if USE_TILT_BRUSH_CPP
            unsafe
            {
                fixed (Vector4* v4Fixed = v4)
                {
                    TiltBrushCpp.TransformVector4AsPoint(mat, iVert, iVertEnd, v4Fixed);
                }
            }
#else
            for (int i = iVert; i < iVertEnd; i++)
            {
                Vector3 p = mat.MultiplyPoint(new Vector3(v4[i].x, v4[i].y, v4[i].z));
                v4[i] = new Vector4(p.x, p.y, p.z, v4[i].w);
            }
#endif
        }

        /// Transform a subset of a array of Vector4 elements as vectors.
        /// Pass:
        ///   m        - the transform to apply.
        ///   iVert    - start of verts to transform
        ///   iVertEnd - end (not inclusive) of verts to transform
        ///   v4       - the array of vectors to transform
        public static void TransformVector4AsVector(Matrix4x4 mat, int iVert, int iVertEnd,
                                                    Vector4[] v4)
        {
#if USE_TILT_BRUSH_CPP
            unsafe
            {
                fixed (Vector4* v4Fixed = v4)
                {
                    TiltBrushCpp.TransformVector4AsVector(mat, iVert, iVertEnd, v4Fixed);
                }
            }
#else
            for (int i = iVert; i < iVertEnd; i++)
            {
                Vector3 vNew = mat.MultiplyVector(new Vector3(v4[i].x, v4[i].y, v4[i].z));
                v4[i] = new Vector4(vNew.x, vNew.y, vNew.z, v4[i].w);
            }
#endif
        }

        /// Transform a subset of a array of Vector4 elements as z distances.
        /// Pass:
        ///   m        - the transform to apply.
        ///   iVert    - start of verts to transform
        ///   iVertEnd - end (not inclusive) of verts to transform
        ///   v4       - the array of vectors to transform
        public static void TransformVector4AsZDistance(float scale, int iVert, int iVertEnd,
                                                       Vector4[] v4)
        {
#if USE_TILT_BRUSH_CPP
            unsafe
            {
                fixed (Vector4* v4Fixed = v4)
                {
                    TiltBrushCpp.TransformVector4AsZDistance(scale, iVert, iVertEnd, v4Fixed);
                }
            }
#else
            for (int i = iVert; i < iVertEnd; i++)
            {
                v4[i] = new Vector4(v4[i].x, v4[i].y, scale * v4[i].z, v4[i].w);
            }
#endif
        }

        /// Computes a new orientation frame using parallel transport.
        ///
        /// Pass:
        ///   tangent -
        ///     Tangent direction; usually points from the previous frame position to this one.
        ///     Must be unit-length.
        ///   previousFrame - The orientation of the previous frame; may be null.
        ///   bootstrapOrientation -
        ///     A hint, used when previousFrame == null. One of its axes will be used to calculate
        ///     one of the resulting frame's normals.
        ///
        /// Returns a new frame such that:
        ///   - Forward is aligned with tangent.
        ///   - Change in orientation is all swing, no twist (twist defined about the tangent).
        public static Quaternion ComputeMinimalRotationFrame(
            Vector3 tangent,
            Quaternion? previousFrame,
            Quaternion bootstrapOrientation)
        {
            Debug.Assert(Mathf.Abs(tangent.magnitude - 1) < 1e-4f);
            if (previousFrame == null)
            {
                // Create a new one. We need 2 vectors, so pick the 2nd from
                // the bootstrap orientation.
                Vector3 desiredUp = bootstrapOrientation * Vector3.up;
                if (Vector3.Dot(desiredUp, tangent) < .01f)
                {
                    // Close to collinear; LookRotation will give a rubbish orientation
                    desiredUp = bootstrapOrientation * Vector3.right;
                }
                return Quaternion.LookRotation(tangent, desiredUp);
            }

            Vector3 nPrevTangent = previousFrame.Value * Vector3.forward;
            Quaternion minimal = Quaternion.FromToRotation(nPrevTangent, tangent);
            return minimal * previousFrame.Value;
        }

        /// Returns a random int spanning the full range of ints.
        public static int RandomInt()
        {
            // It's a bit tricky to do with Random.Range -- do you pass (0x80000000, 0x7fffffff)?
            // or (0, 0xffffffff)? What about the fact that the upper end is exclusive?
            uint low = (uint)UnityEngine.Random.Range(0, 0x10000);
            uint high = (uint)UnityEngine.Random.Range(0, 0x10000);
            return unchecked((int)((high << 16) ^ low));
        }
    } // MathUtils
}     // TiltBrush

﻿using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using System.Numerics;
using System.Runtime.CompilerServices;
using Quaternion = BepuUtilities.Quaternion;

namespace BepuPhysics.CollisionDetection.CollisionTasks
{
    public struct ConvexCompoundContinuations<TCompound> : IConvexCompoundContinuationHandler<NonconvexReduction> where TCompound : ICompoundShape
    {
        public CollisionContinuationType CollisionContinuationType => CollisionContinuationType.NonconvexReduction;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref NonconvexReduction CreateContinuation<TCallbacks>(
            ref CollisionBatcher<TCallbacks> collisionBatcher, int childCount, BufferPool pool, in BoundsTestedPair pair, out int continuationIndex)
            where TCallbacks : struct, ICollisionCallbacks
        {
            return ref collisionBatcher.NonconvexReductions.CreateContinuation(childCount, pool, out continuationIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void ConfigureContinuationChild<TCallbacks>(
            ref CollisionBatcher<TCallbacks> collisionBatcher, ref NonconvexReduction continuation, int continuationChildIndex, in BoundsTestedPair pair, int childIndex,
            out int compoundChildType, out void* compoundChildShapeData, out Vector3 convexToChild, out Quaternion childOrientation)
            where TCallbacks : struct, ICollisionCallbacks
        {
            ref var compoundChild = ref Unsafe.AsRef<TCompound>(pair.B).GetChild(childIndex);
            ref var continuationChild = ref continuation.Children[continuationChildIndex];
            Compound.GetRotatedChildPose(compoundChild.LocalPose, pair.OrientationB, out var rotatedChildPose);
            compoundChildType = compoundChild.ShapeIndex.Type;
            collisionBatcher.Shapes[compoundChildType].GetShapeData(compoundChild.ShapeIndex.Index, out compoundChildShapeData, out _);
            convexToChild = pair.OffsetB + rotatedChildPose.Position;
            childOrientation = rotatedChildPose.Orientation;
            if (pair.FlipMask < 0)
            {
                continuationChild.ChildIndexA = childIndex;
                continuationChild.ChildIndexB = 0;
                continuationChild.OffsetA = rotatedChildPose.Position;
                continuationChild.OffsetB = default;
            }
            else
            {
                continuationChild.ChildIndexA = 0;
                continuationChild.ChildIndexB = childIndex;
                continuationChild.OffsetA = default;
                continuationChild.OffsetB = rotatedChildPose.Position;
            }
        }

    }
}

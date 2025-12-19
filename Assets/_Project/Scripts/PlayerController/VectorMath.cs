

using UnityEngine;

public class VectorMath
{
    /// <summary>
    /// 计算由法向量定义的平面上两个向量之间的有符号角度,判断是从 vector1 向左还是向右旋转到 vector2。
    /// </summary>
    /// <param name="vector1">第一个向量</param>
    /// <param name="vector2">第二个向量</param>
    /// <param name="planeNormal">用于计算角度的平面法向量。</param>
    /// <returns>向量之间以度为单位的有符号角度。</returns>
    public static float GetAngle(Vector3 vector1, Vector3 vector2, Vector3 planeNormal)
    {
        var angle = Vector3.Angle(vector1, vector2);
        var sign = Mathf.Sign(Vector3.Dot(planeNormal, Vector3.Cross(vector1, vector2)));
        return angle * sign;
    }

    /// <summary>
    /// 计算向量和归一化方向的点积。
    /// </summary>
    /// <param name="vector">要投影的向量。</param>
    /// <param name="direction">要投影到的方向向量。</param>
    /// <returns>向量和方向的点积。</returns>
    public static float GetDotProduct(Vector3 vector, Vector3 direction) =>
        Vector3.Dot(vector, direction.normalized);

    /// <summary>
    /// 移除向量中与给定向量方向相同的分量。
    /// </summary>
    /// <param name="vector">要移除分量的向量。</param>
    /// <param name="direction">应该被移除分量的方向向量。</param>
    /// <returns>已移除指定方向分量的向量。</returns>
    public static Vector3 RemoveDotVector(Vector3 vector, Vector3 direction)
    {
        direction.Normalize();
        return vector - direction * Vector3.Dot(vector, direction);
    }

    /// <summary>
    /// 提取并返回向量中与给定向量方向相同的分量。
    /// </summary>
    /// <param name="vector">要提取分量的向量。</param>
    /// <param name="direction">要沿其提取的向量方向。</param>
    /// <returns>向量中与给定向量方向相同的分量。</returns>
    public static Vector3 ExtractDotVector(Vector3 vector, Vector3 direction)
    {
        direction.Normalize();
        return direction * Vector3.Dot(vector, direction);
    }

    /// <summary>
    /// 使用指定的向上方向将向量旋转到由法向量定义的平面上。
    /// </summary>
    /// <param name="vector">要旋转到平面上的向量。</param>
    /// <param name="planeNormal">目标平面的法向量。</param>
    /// <param name="upDirection">用于确定旋转的当前"上"方向。</param>
    /// <returns>旋转到指定平面上后的向量。</returns>
    public static Vector3 RotateVectorOntoPlane(Vector3 vector, Vector3 planeNormal, Vector3 upDirection)
    {
        // Calculate rotation;
        var rotation = Quaternion.FromToRotation(upDirection, planeNormal);

        // Apply rotation to vector;
        vector = rotation * vector;

        return vector;
    }

    /// <summary>
    /// 将给定点投影到由起始位置和方向向量定义的线上。
    /// 找到线上最接近 point 的点。这是几何投影的直接应用。
    /// </summary>
    /// <param name="lineStartPosition">线的起始位置。</param>
    /// <param name="lineDirection">线的方向向量，应该是归一化的。</param>
    /// <param name="point">要投影到线上的点。</param>
    /// <returns>线上最接近原始点的投影点。</returns>
    public static Vector3 ProjectPointOntoLine(Vector3 lineStartPosition, Vector3 lineDirection, Vector3 point)
    {
        var projectLine = point - lineStartPosition;
        var dotProduct = Vector3.Dot(projectLine, lineDirection);

        return lineStartPosition + lineDirection * dotProduct;
    }

    /// <summary>
    /// 在给定时间间隔内以指定速度将向量朝目标向量递增。
    /// 实现向量（位置、速度、颜色等）以恒定速度平滑地向目标向量靠近，而不是瞬间到达。
    /// </summary>
    /// <param name="currentVector">要递增的当前向量。</param>
    /// <param name="speed">朝目标向量移动的速度。</param>
    /// <param name="deltaTime">移动的时间间隔。</param>
    /// <param name="targetVector">要接近的目标向量。</param>
    /// <returns>按指定速度和时间间隔朝目标向量递增后的新向量。</returns>
    public static Vector3 IncrementVectorTowardTargetVector(Vector3 currentVector, float speed, float deltaTime,
        Vector3 targetVector)
    {
        return Vector3.MoveTowards(currentVector, targetVector, speed * deltaTime);
    }
}
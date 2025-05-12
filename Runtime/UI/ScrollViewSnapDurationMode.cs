namespace Aurora.Unity.UI
{
    /// <summary>
    /// 如何计算 <see cref="ScrollView"/> 自动吸附的耗时。
    /// </summary>
    public enum ScrollViewSnapDurationMode
    {
        /// <summary>
        /// 使用固定的吸附时间。
        /// </summary>
        Fixed,

        /// <summary>
        /// 根据距离和速度计算吸附时间。
        /// </summary>
        Dynamic
    }
}

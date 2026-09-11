namespace CodeIcf.Input
{
    public abstract class AbstractNSInputHistory
    {
        /// <summary>このデータが有効かのフラグ</summary>
        public bool IsEnable { get; protected set; }
        /// <summary>このデータを取得した時間</summary>
        public float GetDataTime { get; protected set; }
    }
}